// Geometry.cs - dEngine
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
using System.IO;
using dEngine.Graphics;
using dEngine.Graphics.Structs;
using dEngine.Instances.Attributes;
using dEngine.Instances.Materials;
using dEngine.Utility.Native;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace dEngine.Data
{
    /// <summary>
    /// Represents a geometry. Instances of a geometry are rendered with a <see cref="RenderObject" />.
    /// </summary>
    [TypeId(21)]
    public class Geometry : AssetBase, IEquatable<Geometry>
    {
        private static readonly char[] _robloxMeshVersion1 = "version 1.00".ToCharArray();
        private BoundingBox _boundingBox;

        /// <summary>
        /// Creates an empty geometry.
        /// </summary>
        public Geometry()
        {
            Material = Material.Smooth;
            Vertices = new Vertex[0];
            Indices = new int[0];
        }

        /// <summary>
        /// Creates a new geometry.
        /// </summary>
        /// <param name="name">The name of the geometry.</param>
        /// <param name="vertices">An array of vertices.</param>
        /// <param name="indices">An array of indices.</param>
        /// <param name="topology">The type of primitive to draw with.</param>
        /// <param name="weights">An optional array of weights, if provided this geometry will be treated as a skeletal mesh.</param>
        /// <param name="material">The material to use.</param>
        internal Geometry(string name, Vertex[] vertices, int[] indices, WeightedVertex[] weights = null,
            PrimitiveTopology topology = PrimitiveTopology.TriangleList, Material material = null)
        {
            Name = name;
            Vertices = vertices;
            Indices = indices;
            Weights = weights;
            PrimitiveTopology = topology;
            Guid = Guid.NewGuid();
            Material = material ?? Material.Smooth;

            ComputeBoundingBox();

            if ((weights != null) && (weights.Length > 0))
                IsSkinned = true;
            
            Renderer.InvokeResourceDependent(DoBuildBuffersJob);

            IsLoaded = true;
        }

        internal Buffer VertexBuffer { get; set; }
        internal Buffer VertexWeightBuffer { get; set; }
        internal Buffer IndexBuffer { get; set; }
        internal VertexBufferBinding VertexBufferBinding { get; set; }
        internal VertexBufferBinding VertexWeightBufferBinding { get; set; }

        /// <summary>
        /// The vertices.
        /// </summary>
        internal Vertex[] Vertices { get; private set; }

        /// <summary>
        /// The bone data.
        /// </summary>
        internal WeightedVertex[] Weights { get; }

        /// <summary>
        /// The indices.
        /// </summary>
        public int[] Indices { get; }

        /// <summary>
        /// The primitive topology that this geometry renders with.
        /// </summary>
        public PrimitiveTopology PrimitiveTopology { get; private set; }

        /// <summary>
        /// The name of the geometry.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The GUID of the geometry.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// The content url of the material to use.
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// If true, this mesh uses bone weights.
        /// </summary>
        public bool IsSkinned { get; private set; }

        /// <summary>
        /// The number of indices.
        /// </summary>
        public int IndexCount => Indices.Length;

        /// <summary>
        /// The number of triangle.
        /// </summary>
        public int TriangleCount => Indices.Length/2;

        /// <inheritdoc />
        public override ContentType ContentType => IsSkinned ? ContentType.SkeletalMesh : ContentType.StaticMesh;

        /// <summary>
        /// Used by the FBX importer.
        /// </summary>
        internal int MaterialIndex { get; set; }

        /// <summary>
        /// Determins if two geometries are equal.
        /// </summary>
        public bool Equals(Geometry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Guid, other.Guid);
        }

        /// <summary>
        /// Fired when the buffers are rebuilt.
        /// </summary>
        public event Action Rebuilt;
        
        private void LoadRobloxMesh(Stream stream)
        {
            using (var stringReader = new StreamReader(stream))
            {
                stringReader.ReadLine();
                var test = stringReader.ReadLine();
                var triangleCount = int.Parse(test);
                var dataLine = stringReader.ReadLine();
                dataLine = dataLine.Substring(1, dataLine.Length - 1);
                var vectors = dataLine.Split(new[] { "][" }, StringSplitOptions.RemoveEmptyEntries);

                var vertCount = triangleCount * 3;
                Vertices = new Vertex[vertCount];

                for (var j = 0; j < vertCount; j += 3)
                {
                    var positionValues = vectors[j].Split(',');
                    var normalValues = vectors[j + 1].Split(',');
                    var textureValues = vectors[j + 2].Split(',');

                    var pos = new Vector3(float.Parse(positionValues[0]), float.Parse(positionValues[1]),
                        float.Parse(positionValues[2]));
                    var normal = new Vector3(float.Parse(normalValues[0]), float.Parse(normalValues[1]),
                        float.Parse(normalValues[2]));
                    var texCoord = new Vector2(float.Parse(textureValues[0]), float.Parse(textureValues[1]));
                    Vector3 tangent;
                    Vector3 bitangent;

                    // TODO: compute tangents
                    tangent = new Vector3();
                    bitangent = new Vector3();

                    Vertices[j] = new Vertex(pos, normal, texCoord, Colour.Zero, tangent, bitangent);
                }
            }
        }

        /// <summary />
        protected override void OnSave(BinaryWriter writer)
        {
            base.OnSave(writer);
            var vertCount = Vertices.Length;
            writer.Write(vertCount);
            for (var i = 0; i < vertCount; i++)
                Vertices[i].Save(writer);
        }

        /// <summary />
        protected override void OnLoad(BinaryReader reader)
        {
            var header = reader.ReadChars(_robloxMeshVersion1.Length);
            if (VisualC.CompareMemory(header, _robloxMeshVersion1, header.Length) == 0) // special case for roblox mesh
                return;

            reader.BaseStream.Position = 0;
            base.OnLoad(reader);
            var vertCount = reader.ReadInt32();
            Vertices = new Vertex[vertCount];
            for (var i = 0; i < vertCount; i++)
            {
                var vert = new Vertex();
                vert.Load(reader);
                Vertices[i] = vert;
            }
        }

        /// <summary />
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
            }

            VertexBuffer?.Dispose();
            VertexWeightBuffer?.Dispose();
            IndexBuffer?.Dispose();

            Rebuilt = null;
            _disposed = true;
        }

        private void ComputeBoundingBox()
        {
            var result1 = new Vector3(float.MaxValue);
            var result2 = new Vector3(float.MinValue);
            for (var index = 0; index < Vertices.Length; ++index)
            {
                Vector3.Min(ref result1, ref Vertices[index].Position, out result1);
                Vector3.Max(ref result2, ref Vertices[index].Position, out result2);
            }
            _boundingBox = new BoundingBox((SharpDX.Vector3)result1, (SharpDX.Vector3)result2);
        }

        /// <inheritdoc />
        protected override void AfterDeserialization()
        {
            if (Weights != null)
                IsSkinned = true;

            ComputeBoundingBox();

            Renderer.InvokeResourceDependent(DoBuildBuffersJob);

            IsLoaded = true;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Geometry)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        private void DoBuildBuffersJob()
        {
            lock (Renderer.Locker)
            {
                BuildBuffers(Renderer.Context);
            }
        }

        internal void BuildBuffers(DeviceContext context)
        {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();

            VertexBuffer = Buffer.Create(context.Device, BindFlags.VertexBuffer, Vertices);
            VertexBufferBinding = new VertexBufferBinding(VertexBuffer, Vertex.Stride, 0);
            IndexBuffer = Buffer.Create(context.Device, BindFlags.IndexBuffer, Indices);

            if (IsSkinned)
            {
                VertexWeightBuffer?.Dispose();
                VertexWeightBuffer = Buffer.Create(context.Device, BindFlags.VertexBuffer, Weights);
                VertexWeightBufferBinding = new VertexBufferBinding(VertexWeightBuffer, WeightedVertex.Stride, 0);
            }

            Rebuilt?.Invoke();
        }

        internal Vector3 GetOffset()
        {
            return (Vector3)((_boundingBox.Minimum + _boundingBox.Maximum)/2);
        }

        internal Vector3 GetSize()
        {
            return (Vector3)(_boundingBox.Maximum - _boundingBox.Minimum);
        }
    }
}