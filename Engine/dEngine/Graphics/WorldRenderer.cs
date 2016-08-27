// RenderObjectProvider.cs - dEngine
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
using System.Diagnostics;
using C5;
using dEngine.Data;
using dEngine.Graphics.States;
using dEngine.Instances;
using dEngine.Instances.Materials;
using dEngine.Services;
using SharpDX.Direct3D11;

namespace dEngine.Graphics
{
    using static Renderer;

    /// <summary>
    /// A container class for holding <see cref="RenderObject" />s for a specific <see cref="IWorld" />.
    /// </summary>
    public class WorldRenderer : IDisposable
    {
        private readonly RenderObject _cubeRenderer;
        private readonly RenderObject _cylinderRenderer;
        private readonly RenderObject _sphereRenderer;
        private readonly RenderObject _wedgeRenderer;

        private readonly object _locker = new object();
        private readonly PartDistanceComparer _partDistanceComparer;
        private readonly TreeDictionary<GeoMatPair, RenderObject> _renderObjects;
        private readonly List<RenderObject> _renderObjects2;

        internal readonly RenderObject SkyboxRO;
        private TreeSet<Part> _transparentSet;

        internal WorldRenderer()
        {
            _partDistanceComparer = new PartDistanceComparer(this);
            _renderObjects = new TreeDictionary<GeoMatPair, RenderObject>();
            _renderObjects2 = new List<RenderObject>();
            _transparentSet = new TreeSet<Part>(_partDistanceComparer);

            _cubeRenderer = new RenderObject("Cube", Primitives.CubeGeometry);
            _cylinderRenderer = new RenderObject("Cylinder", Primitives.CylinderGeometry);
            _sphereRenderer = new RenderObject("Sphere", Primitives.SphereGeometry);
            _wedgeRenderer = new RenderObject("Wedge", Primitives.WedgeGeometry);

            SkyboxRO = new RenderObject("Skybox", Primitives.CubeGeometry);
        }

        /// <summary>
        /// Gets a renderer for the given mesh name.
        /// </summary>
        public RenderObject this[Geometry geometry, Material material]
        {
            get
            {
                lock (_locker)
                {
                    Debug.Assert(geometry != null, "geometry != null");
                    RenderObject ro;
                    var tuple = new GeoMatPair(geometry, material);
                    if (!_renderObjects.Contains(tuple))
                    {
                        _renderObjects[tuple] = ro = new RenderObject(geometry.Name, geometry, material);
                        _renderObjects2.Add(ro);
                    }
                    else
                    {
                        ro = _renderObjects[tuple];
                    }
                    return ro;
                }
            }
            set
            {
                lock (_locker)
                {
                    _renderObjects[new GeoMatPair(geometry, material)] = value;
                }
            }
        }

        /// <summary>
        /// Gets a renderer for the given shape.
        /// </summary>
        public RenderObject this[Shape shape]
        {
            get
            {
                switch (shape)
                {
                    case Shape.Cube:
                        return _cubeRenderer;
                    case Shape.Sphere:
                        return _sphereRenderer;
                    case Shape.Cylinder:
                        return _cylinderRenderer;
                    case Shape.Wedge:
                        return _wedgeRenderer;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(shape), shape, null);
                }
            }
        }

        /// <summary />
        public void Dispose()
        {
            _cubeRenderer.Dispose();
            _cylinderRenderer.Dispose();
            _sphereRenderer.Dispose();
            _wedgeRenderer.Dispose();
            SkyboxRO.Dispose();

            lock (_locker)
            {
                foreach (var ro in _renderObjects)
                {
                    ro.Value.Dispose();
                }
            }
        }

        internal static void DrawSky(ref Camera camera)
        {
            var skybox = Game.Lighting.Skybox;
            var skyboxRO = Game.Workspace.RenderObjectProvider.SkyboxRO;

            SkyboxPass.Use(ref Context);

            var skyboxConstants = Game.Lighting.SkyboxConstantBuffer;
            skyboxConstants.Update(ref Context);
            Context.VertexShader.SetConstantBuffers(0, camera.Constants.Buffer, skyboxConstants.Buffer);

            lock (skybox)
            {
                Context.PixelShader.SetShaderResources(0,
                    skybox.Cubemap?.Texture,
                    skybox.Starfield);

                skyboxRO.Draw(ref Context, ref camera);
            }

            DepthStencilView dsv = camera.DepthStencilBuffer;
            if (dsv != null)
                Context.ClearDepthStencilView(dsv, DepthStencilClearFlags.Depth, 1, 0);
        }
        
        internal void Draw(ref DeviceContext context, ref Camera camera, bool isShadowPass)
        {
            if (!isShadowPass)
            {
                context.OutputMerger.SetBlendState(BlendStates.Disabled);
                context.OutputMerger.SetDepthStencilState(DepthStencilStates.Standard);
                context.Rasterizer.State = RasterizerStates.BackFaceCull;

                DrawSky(ref camera);

                MainPass.Use(ref context);
                Context.VertexShader.SetConstantBuffers(0, camera.Constants, Lighting.LightingConstantBuffer);
                Context.PixelShader.SetConstantBuffers(0, camera.Constants, Lighting.LightingConstantBuffer, Shadows.ReceiverConstants);
                Context.PixelShader.SetShaderResources(0, Shadows.ShadowMap);
            }

            _cubeRenderer.Draw(ref context, ref camera);
            _cylinderRenderer.Draw(ref context, ref camera);
            _sphereRenderer.Draw(ref context, ref camera);
            _wedgeRenderer.Draw(ref context, ref camera);

            lock (_locker)
            {
                for (var i = 0; i < _renderObjects2.Count; i++)
                {
                    _renderObjects2[i].Draw(ref context, ref camera);
                }

                _partDistanceComparer.CameraPosition = camera.CFrame.p;
            }
        }

        private class GeoMatPair : IEquatable<GeoMatPair>, IComparable<GeoMatPair>
        {
            // ReSharper disable once MemberCanBePrivate.Local
            public readonly Geometry Geometry;
            // ReSharper disable once MemberCanBePrivate.Local
            public readonly Material Material;

            public GeoMatPair(Geometry geometry, Material material)
            {
                Geometry = geometry;
                Material = material;
            }

            public int CompareTo(GeoMatPair other)
            {
                return Equals(other) ? 0 : 1;
            }

            public bool Equals(GeoMatPair other)
            {
                return Equals(Geometry, other.Geometry) && ReferenceEquals(Material, other.Material);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Geometry?.GetHashCode() ?? 0) * 397) ^ (Material?.GetHashCode() ?? 0);
                }
            }
        }

        internal class PartDistanceComparer : IComparer<Part>
        {
            private readonly WorldRenderer _provider;

            public PartDistanceComparer(WorldRenderer provider)
            {
                _provider = provider;
            }

            public Vector3 CameraPosition { get; set; }

            public int Compare(Part x, Part y)
            {
                lock (_provider._locker)
                {
                    var xDist = (x.Position - CameraPosition).magnitude;
                    var yDist = (y.Position - CameraPosition).magnitude;
                    return xDist.CompareTo(yDist);
                }
            }
        }
    }
}