// FBX.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Assimp;
using dEngine.Data;
using dEngine.Graphics.Structs;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Serializer.V1;
using dEngine.Services;
using SharpDX;
using SharpDX.Direct3D;
using Bone = dEngine.Instances.Bone;
using Material = dEngine.Instances.Materials.Material;
using AssimpMaterial = Assimp.Material;

#pragma warning disable 1591

namespace dEngine.Utility.FileFormats.Model
{
    /// <summary>
    /// Helper class for converting FBX files to dEngine assets.
    /// </summary>
    public static class FBX
    {
        /// <summary>
        /// Enum for normal import methods.
        /// </summary>
        public enum NormalImportMethod
        {
            /// <summary>
            /// The importer will compute normals and tangents.
            /// </summary>
            ComputeNormals,

            /// <summary>
            /// The importer will compute smooth normals and tangents.
            /// </summary>
            ComputeNormalsSmooth,

            /// <summary>
            /// The importer will import normals but compute tangents.
            /// </summary>
            ImportNormals,

            /// <summary>
            /// The importer will import normals and tangents.
            /// </summary>
            ImportNormalsAndTangents
        }

        public static ImportResult Import(string filePath, ImportSettings settings)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return Import(stream, settings, Path.GetExtension(filePath), filePath);
            }
        }

        /// <summary>
        /// Imports an FBX file.
        /// </summary>
        /// <param name="stream">The FBX stream.</param>
        /// <param name="settings">An import settings object.</param>
        /// <param name="format">The filetype of the mesh.</param>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>An <see cref="ImportResult" /> object.</returns>
        public static ImportResult Import(Stream stream, ImportSettings settings, string format, string filePath)
        {
            var sceneName = Path.GetFileNameWithoutExtension(filePath);

            Debug.Assert(sceneName != null, "sceneName != null");

            var steps = PostProcessSteps.ValidateDataStructure
                        | PostProcessSteps.GenerateUVCoords
                        | PostProcessSteps.Triangulate
                        | PostProcessSteps.SortByPrimitiveType
                        | PostProcessSteps.FlipUVs
                        | PostProcessSteps.FlipWindingOrder
                        | PostProcessSteps.LimitBoneWeights
                        | PostProcessSteps.ImproveCacheLocality
                        | PostProcessSteps.RemoveComponent
                        | PostProcessSteps.RemoveRedundantMaterials;

            if (!settings.KeepOverlappingVertices) steps |= PostProcessSteps.JoinIdenticalVertices;
            if (!settings.ImportAsSkeletal) steps |= PostProcessSteps.Debone;
            if (settings.MergeMeshes) steps |= PostProcessSteps.OptimizeMeshes;

            switch (settings.NormalImportMethod)
            {
                case NormalImportMethod.ComputeNormals:
                    steps |= PostProcessSteps.GenerateNormals;
                    steps |= PostProcessSteps.CalculateTangentSpace;
                    break;
                case NormalImportMethod.ComputeNormalsSmooth:
                    steps |= PostProcessSteps.GenerateSmoothNormals;
                    steps |= PostProcessSteps.CalculateTangentSpace;
                    break;
                case NormalImportMethod.ImportNormals:
                    steps |= PostProcessSteps.CalculateTangentSpace;
                    break;
                case NormalImportMethod.ImportNormalsAndTangents:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var scene = ContentProvider.AssimpContext.ImportFileFromStream(stream, steps, format);
            Skeleton skeleton = null;
            var importedSkeleton = false;
            var importedAnimation = false;
            var importedMesh = false;
            var importedMaterials = false;
            var geometries = new Dictionary<string, Geometry>(scene.MeshCount);
            var animations = new AnimationData[scene.AnimationCount];
            var materials = new Material[scene.MaterialCount];
            var textures = new Data.Texture[scene.TextureCount];

            if (settings.ImportAsSkeletal)
            {
                if (settings.Skeleton == null)
                {
                    SkeletonNodeData nodeData;

                    if (!FindSkeleton(ref scene, out nodeData))
                        throw new InvalidOperationException("Could not import skeleton.");

                    skeleton = ImportSkeleton(ref nodeData);
                    importedSkeleton = true;
                }
                else
                {
                    skeleton = settings.Skeleton;
                }
            }

            if (scene.HasMaterials)
            {
                importedMaterials = true;

                if (filePath != null)
                {
                    var fileFolder = Path.GetDirectoryName(filePath);
                    for (var i = 0; i < scene.MaterialCount; i++)
                    {
                        var mat = scene.Materials[i];

                        var material = new Material();



                        materials[i] = material;
                    }
                }
            }

            for (var i = 0; i < scene.MeshCount; i++)
            {
                var mesh = scene.Meshes[i];
                var meshName = string.IsNullOrWhiteSpace(mesh.Name) ? $"{sceneName} ({i})" : mesh.Name;

                var vertices = new Vertex[mesh.VertexCount];
                WeightedVertex[] weights = null;

                if (mesh.HasBones && settings.ImportAsSkeletal)
                    weights = new WeightedVertex[mesh.VertexCount];

                for (var j = 0; j < mesh.VertexCount; j++)
                {
                    var texCoordChannel = mesh.HasTextureCoords(0)
                        ? mesh.TextureCoordinateChannels[0][j]
                        : new Vector3D(0, 0, 0);
                    var vertexColourChannel = mesh.HasVertexColors(0)
                        ? mesh.VertexColorChannels[0][j]
                        : new Color4D(0, 0, 0, 1);

                    var tangent = mesh.HasTangentBasis ? mesh.Tangents[j] : new Vector3D(0, 0, 0);
                    var bitangent = mesh.HasTangentBasis ? mesh.Tangents[j] : new Vector3D(0, 0, 0);

                    vertices[j] = new Vertex
                    {
                        Position = (Vector3)mesh.Vertices[j],
                        Normal = (Vector3)mesh.Normals[j],
                        Colour = vertexColourChannel,
                        TexCoord = new Vector2(texCoordChannel.X, texCoordChannel.Y),
                        Tangent = (Vector3)tangent,
                        BiTangent = (Vector3)bitangent
                    };

                    // TODO: add bone data
                }

                importedMesh = true;
                geometries[mesh.Name] = new Geometry(meshName, vertices.ToArray(), mesh.GetIndices(), weights?.ToArray(),
                    PrimitiveTopology.TriangleList)
                { MaterialIndex = mesh.MaterialIndex };
            }

            if (settings.MergeMeshes && !settings.ImportAsSkeletal)
            {
                var vertices = new Vertex[0];
                var weights = new WeightedVertex[0];
                var indices = new int[0];

                foreach (var mesh in geometries.Values)
                {
                    var lastVertices = vertices;
                    vertices = new Vertex[lastVertices.Length + mesh.Vertices.Length];
                    lastVertices.CopyTo(vertices, 0);
                    mesh.Vertices.CopyTo(vertices, lastVertices.Length);

                    var lastWeights = weights;
                    weights = new WeightedVertex[lastWeights.Length + (mesh.Weights?.Length ?? 0)];
                    lastWeights.CopyTo(weights, 0);
                    mesh.Weights?.CopyTo(weights, lastWeights.Length);

                    var lastIndices = indices;
                    indices = new int[indices.Length + mesh.Indices.Length];
                    lastIndices.CopyTo(indices, 0);
                    mesh.Indices.Select(i => i + lastVertices.Length).ToArray().CopyTo(indices, lastIndices.Length);

                    mesh.Dispose();
                }

                geometries.Clear();
                geometries[sceneName] = new Geometry(sceneName, vertices, indices, weights);
            }

            for (var i = 0; i < scene.AnimationCount; i++)
            {
                var animation = scene.Animations[i];

                animations[i] = new AnimationData();
                importedAnimation = true;

                // TODO: import animations
            }

            string path;

            if (settings.ContentPath == null)
            {
                path = Path.Combine(Engine.TempPath, Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(path);
            }
            else
            {
                path = settings.ContentPath;
            }

            #region File Creation

            foreach (var material in materials)
            {
                var materialPath = Path.Combine(path, $"{material.Name}.material");
                if (materialPath == null) continue;
                using (var file = File.Create(materialPath))
                {
                    Inst.Serialize(material, file);
                }
            }

            foreach (var geo in geometries.Values)
            {
                var meshPath = GetFilePath(sceneName, path, settings);
                if (meshPath == null) continue;
                using (var file = File.Create(meshPath))
                {
                    geo.Material = materials[geo.MaterialIndex];
                    geo.Save(file);
                }
            }

            if (importedSkeleton)
            {
                var skeletonPath = GetFilePath(sceneName, path, settings);
                if (skeletonPath != null)
                {
                    using (var file = File.Create(skeletonPath))
                    {
                        Inst.Serialize(skeleton, file);
                    }
                }
            }

            foreach (var animation in animations)
            {
                var animPath = GetFilePath(sceneName, path, settings);
                if (animPath == null) continue;
                using (var file = File.Create(animPath))
                {
                    animation.Save(file);
                }
            }

            #endregion

            if (settings.ContentPath != null)
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    File.Move(file, Path.Combine(settings.ContentPath, Path.GetFileName(file)));
                }
            }

            return new ImportResult(geometries, animations, materials, skeleton, importedSkeleton, importedAnimation,
                importedMesh, importedMaterials);
        }

        private static string ToContentId(string file, string contentPath, string scheme)
        {
            return $"{scheme}://{file.Substring(contentPath.Length + 1)}";
        }

        private static string GetFilePath(string name, string importPath, ImportSettings settings)
        {
            var path = $"{importPath}/{name}.bin";

            if (!File.Exists(path))
                return path;

            if (settings.ContentPath == null)
                return path;

            var result = MessageBox.Show($"An asset already exists at the import location: {path}\nOverwrite?",
                "Overwrite File", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

            switch (result)
            {
                case DialogResult.None:
                case DialogResult.Cancel:
                    //cancelled = true;
                    return null;
                case DialogResult.Yes:
                    //cancelled = false;
                    return path;
                case DialogResult.No:
                    //cancelled = false;
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Skeleton ImportSkeleton(ref SkeletonNodeData nodeData)
        {
            var skeleton = new Skeleton();
            var rootBone = ImportBones(nodeData.RootNode, nodeData.RootNode.Parent, null, ref nodeData);
            skeleton.RootBone = rootBone;
            return skeleton;
        }

        private static Bone ImportBones(Node aiNode, Node aiParent, Bone parent, ref SkeletonNodeData nodeData)
        {
            Debug.Assert(aiNode != null);
            Debug.Assert(aiParent != null);

            Bone bone = null;
            if (!aiNode.Name.Contains("_$AssimpFbx$"))
            {
                const string mangling = "_$AssimpFbxNull$";

                if (aiNode.Name.Contains(mangling))
                {
                    bone = new Bone
                    {
                        Name = aiNode.Name.Replace(mangling, string.Empty),
                        CFrame = (CFrame)ToSharpDX(GetRelativeTransform(aiNode, aiParent))
                    };
                }
                else if (nodeData.Nodes.Contains(aiNode))
                {
                    bone = new Bone
                    {
                        Name = aiNode.Name
                    };

                    Matrix offsetMatrix;
                    Matrix parentOffsetMatrix;
                    var isOffsetMatrixValid = nodeData.DeformationBones.TryGetValue(aiNode.Name, out offsetMatrix);
                    var isParentOffsetMatrixValid = nodeData.DeformationBones.TryGetValue(aiParent.Name,
                        out parentOffsetMatrix);
                    if (isOffsetMatrixValid && isParentOffsetMatrixValid)
                    {
                        bone.CFrame = (CFrame)(Matrix.Invert(offsetMatrix) * parentOffsetMatrix);
                    }
                    else if (isOffsetMatrixValid && (aiNode == nodeData.RootNode || aiParent == nodeData.RootNode))
                    {
                        bone.CFrame = (CFrame)(Matrix.Invert(offsetMatrix));
                    }
                    else
                    {
                        bone.CFrame = (CFrame)ToSharpDX(GetRelativeTransform(aiNode, aiParent));
                    }
                }
            }

            if (bone != null)
            {
                bone.Parent = parent;

                aiParent = aiNode;
                parent = bone;
            }

            foreach (var child in aiNode.Children)
                ImportBones(child, aiParent, parent, ref nodeData);

            return bone;
        }

        private static Matrix4x4 GetRelativeTransform(Node node, Node ancestor)
        {
            Debug.Assert(node != null);

            // Get transform of node relative to ancestor.
            var transform = node.Transform;
            var parent = node.Parent;
            while (parent != null && parent != ancestor)
            {
                transform *= parent.Transform;
                parent = parent.Parent;
            }

            if (parent == null && ancestor != null)
                throw new ArgumentException($"Node \"{ancestor.Name}\" is not an ancestor of \"{node.Name}\".");

            return transform;
        }

        private static bool FindSkeleton(ref Scene scene, out SkeletonNodeData nodeData)
        {
            if (!scene.HasMeshes)
                throw new NotSupportedException(
                    "The FBX importer requires meshes; a skeleton can not be imported on its own.");

            nodeData = new SkeletonNodeData
            {
                Nodes = new List<Node>(),
                DeformationBones = FindDeformationBones(ref scene)
            };

            if (nodeData.DeformationBones.Count == 0)
                return false;

            var rootBones = new HashSet<Node>();
            foreach (var boneName in nodeData.DeformationBones.Keys)
                rootBones.Add(FindRootBone(ref scene, boneName));

            if (rootBones.Count > 1)
                throw new NotSupportedException("Multiple skeletons are not supported.");

            nodeData.RootNode = rootBones.First();

            GetSubtree(nodeData.RootNode, nodeData.Nodes);

            return true;
        }

        private static void GetSubtree(Node node, List<Node> list)
        {
            Debug.Assert(node != null);
            Debug.Assert(list != null);

            list.Add(node);
            foreach (var child in node.Children)
                GetSubtree(child, list);
        }

        private static Node FindRootBone(ref Scene scene, string boneName)
        {
            Debug.Assert(scene != null);
            Debug.Assert(!string.IsNullOrEmpty(boneName));

            var node = scene.RootNode.FindNode(boneName);
            Debug.Assert(node != null, "Node referenced by mesh not found in model.");

            var rootBone = node;
            while (node != scene.RootNode && !node.HasMeshes)
            {
                if (!node.Name.Contains("$AssimpFbx$"))
                    rootBone = node;

                node = node.Parent;
            }

            return rootBone;
        }

        private static Dictionary<string, Matrix> FindDeformationBones(ref Scene scene)
        {
            Debug.Assert(scene != null);

            var offsetMatrices = new Dictionary<string, Matrix>();
            if (scene.HasMeshes)
                foreach (var mesh in scene.Meshes)
                    if (mesh.HasBones)
                        foreach (var bone in mesh.Bones)
                            if (!offsetMatrices.ContainsKey(bone.Name))
                                offsetMatrices[bone.Name] = ToSharpDX(bone.OffsetMatrix);

            return offsetMatrices;
        }

        private static Matrix ToSharpDX(Matrix4x4 matrix)
        {
            var result = Matrix.Identity;

            result.M11 = matrix.A1;
            result.M12 = matrix.B1;
            result.M13 = matrix.C1;
            result.M14 = matrix.D1;

            result.M21 = matrix.A2;
            result.M22 = matrix.B2;
            result.M23 = matrix.C2;
            result.M24 = matrix.D2;

            result.M31 = matrix.A3;
            result.M32 = matrix.B3;
            result.M33 = matrix.C3;
            result.M34 = matrix.D3;

            result.M41 = matrix.A4;
            result.M42 = matrix.B4;
            result.M43 = matrix.C4;
            result.M44 = matrix.D4;

            return result;
        }

        /// <summary>
        /// The result of the import.
        /// </summary>
        public struct ImportResult
        {
            public readonly Dictionary<string, Geometry> Meshes;
            public readonly AnimationData[] Animations;
            public readonly Material[] Materials;
            public readonly Skeleton Skeleton;

            public readonly bool HasSkeleton;
            public readonly bool HasAnimations;
            public readonly bool HasMeshes;
            public readonly bool HasMaterials;
            public bool WasCancelled;

            public ImportResult(Dictionary<string, Geometry> meshes, AnimationData[] animations,
                Material[] materials,
                Skeleton skeleton, bool importedSkeleton, bool importedAnimations, bool importedMeshes,
                bool importedMaterials)
            {
                Meshes = meshes;
                Animations = animations;
                Materials = materials;
                Skeleton = skeleton;
                HasSkeleton = importedSkeleton;
                HasAnimations = importedAnimations;
                HasMeshes = importedMeshes;
                HasMaterials = importedMaterials;
                WasCancelled = false;
            }

            public static ImportResult Cancelled => new ImportResult { WasCancelled = true };
        }

        class SkeletonNodeData
        {
            public Node RootNode { get; set; }
            public List<Node> Nodes { get; set; }
            public Dictionary<string, Matrix> DeformationBones { get; set; }
        }

        /// <summary>
        /// FBX import settings.
        /// </summary>
        public class ImportSettings
        {
            private bool _importAsSkeletal;

            public ImportSettings(string contentPath = null)
            {
                ContentPath = contentPath;
            }

            public string ContentPath { get; }

            /// <summary>
            /// Determines if the mesh should be treated a a Sketal or Static mesh.
            /// </summary>
            [DisplayName("Import as Skeletal"), EditorVisible("Mesh")]
            public bool ImportAsSkeletal
            {
                get { return _importAsSkeletal; }
                set
                {
                    _importAsSkeletal = value;
                    ImportAsSkeletalChanged?.Invoke(value);
                }
            }

            /// <summary>
            /// Determines whether or not duplicate vertices will be removed.
            /// </summary>
            [DisplayName("Keep Overlapping Vertices"), EditorVisible("Mesh")]
            public bool KeepOverlappingVertices { get; set; } = false;

            /// <summary>
            /// Determines whether or not duplicate vertices will be removed.
            /// </summary>
            [DisplayName("Normal Import Method"), EditorVisible("Mesh")]
            public NormalImportMethod NormalImportMethod { get; set; } = NormalImportMethod.ImportNormals;

            /// <summary>
            /// Determines if meshes should be merged.
            /// </summary>
            [DisplayName("Merge Meshes"), EditorVisible("Static")]
            public bool MergeMeshes { get; set; } = true;

            public event Action<bool> ImportAsSkeletalChanged;

            #region Skeletal

            /// <summary>
            /// The existing skeleton to use for this mesh. If none is provided, the skeleton will be imported from the file.
            /// </summary>
            [DisplayName("Skeleton"), EditorVisible("Skeletal")]
            public Skeleton Skeleton { get; set; }

            /// <summary>
            /// Determines if the skeleton reference pose should be updated.
            /// </summary>
            [DisplayName("Update Reference Pose"), EditorVisible("Skeletal")]
            public bool UpdateReferencePose { get; set; }

            /// <summary>
            /// Determines if the first frame should be used as the reference pose.
            /// </summary>
            [DisplayName("Use First Frame As Reference Pose"), EditorVisible("Skeletal")]
            public bool UseFirstFrameAsReferencePose { get; set; }

            /// <summary>
            /// If enabled, morph target swill be imported from the FBX file.
            /// </summary>
            [DisplayName("Import Morph Targets"), EditorVisible("Skeletal")]
            public bool ImportMorphTargets { get; set; }

            #endregion

            #region Static

            #endregion
        }
    }
}