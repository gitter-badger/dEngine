// RenderObject.cs - dEngine
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
using dEngine.Data;
using dEngine.Graphics.Structs;
using dEngine.Instances;
using dEngine.Instances.Materials;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace dEngine.Graphics
{
    /// <summary>
    /// An object which holds and renders a collection of instances for a given mesh.
    /// </summary>
    public class RenderObject : IDisposable
    {
        private static readonly byte[] _emptyInstanceBytes = new byte[InstanceRenderData.Stride];
        private readonly List<IRenderable> _renderables;
        private bool _bufferDirty;
        private DataStream _instanceBufferCpu;
        private int _sizeInBytes;

        /// <summary>
        /// Creates a <see cref="RenderObject" /> from a <see cref="Geometry" />.
        /// </summary>
        /// <param name="name">An identifier for debug purposes.</param>
        /// <param name="geometry">The mesh to use.</param>
        /// <param name="material">The material to use.</param>
        public RenderObject(string name, Geometry geometry, Material material = null)
        {
            Name = name;
            Bindings = new VertexBufferBinding[geometry.IsSkinned ? 3 : 2];
            Geometry = geometry;
            Material = material;

            Bindings[0] = geometry.VertexBufferBinding;
            if (geometry.IsSkinned)
                Bindings[2] = geometry.VertexWeightBufferBinding;

            _renderables = new List<IRenderable>();
        }

        internal Material Material { get; }

        /// <summary>
        /// The name of this object. Used for debug purposes.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The total number of instances this object contains.
        /// </summary>
        public int InstanceCount => _renderables.Count;

        /// <summary>
        /// Determines if the buffers for this object have been built.
        /// </summary>
        public bool IsBuilt { get; private set; }

        internal Geometry Geometry { get; }

        internal Buffer InstanceBufferGpu { get; private set; }

        internal VertexBufferBinding[] Bindings { get; }

        /// <summary>
        /// If true, this object has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <inheritdoc />
        public void Dispose()
        {
            IsDisposed = true;
            InstanceBufferGpu?.Dispose();
        }

        /// <summary>
        /// Builds the buffer objects.
        /// </summary>
        /// <remarks>
        /// Must be called after <see cref="Renderer.Init" /> has finished.
        /// </remarks>
        public void Build()
        {
            if (Renderer.IsInitialized == false)
                throw new InvalidOperationException(
                    "RenderBufferObjects cannot be built before the GraphicsManager is initalized.");

            ExpandInstanceBuffers();

            IsBuilt = true;
        }

        private void ExpandInstanceBuffers()
        {
            if (IsDisposed)
                return;

            var stride = InstanceRenderData.Stride;
            var length = stride * Math.Max(_renderables.Count, 1); // buffers with a size of 0 are invalid

            var gpuBuffer = InstanceBufferGpu;

            if (gpuBuffer?.IsDisposed != false || length > gpuBuffer.Description.SizeInBytes)
            {
                var newLength = length * 2;

                gpuBuffer?.Dispose();

                // create GPU upload buffer
                InstanceBufferGpu = new Buffer(Renderer.Device, new BufferDescription
                {
                    SizeInBytes = newLength,
                    StructureByteStride = stride,
                    BindFlags = BindFlags.VertexBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    Usage = ResourceUsage.Dynamic,
                    OptionFlags = ResourceOptionFlags.None
                });

                gpuBuffer = InstanceBufferGpu;

                // create new CPU buffer
                var cpuBuffer = new DataStream(newLength, true, true);

                lock (this)
                {
                    // copy data from old buffer to the new one.
                    if (_instanceBufferCpu != null)
                    {
                        _instanceBufferCpu.Position = 0;
                        if (_instanceBufferCpu.Length > cpuBuffer.Length)
                            throw new InvalidOperationException("Invalid buffer sizes");
                        _instanceBufferCpu.CopyTo(cpuBuffer);
                        _instanceBufferCpu.Dispose();
                    }

                    _instanceBufferCpu = cpuBuffer;
                }

                Bindings[1] = new VertexBufferBinding(gpuBuffer, InstanceRenderData.Stride, 0);
            }
        }

        /// <summary>
        /// Uploads instance data to the GPU.
        /// </summary>
        private void Update(ref DeviceContext context)
        {
            //if (!IsBuilt)
            //	return;

            lock (this) // lock use of _instanceBufferCpu, as it is accessed from various threads.
            {
                DataStream stream;
                context.MapSubresource(InstanceBufferGpu, MapMode.WriteDiscard, MapFlags.None, out stream);
                _instanceBufferCpu.Position = 0;
                stream.Write(_instanceBufferCpu.DataPointer, 0, _sizeInBytes);
                context.UnmapSubresource(InstanceBufferGpu, 0);
                stream.Dispose();
            }

            _bufferDirty = false;
        }

        /// <summary>
        /// Draws instances to the current render target.
        /// </summary>
        public void Draw(ref DeviceContext context, ref Camera camera, bool pix = true)
        {
            if (!IsBuilt)
                Build();

            if (_bufferDirty)
                Update(ref context);

            if (InstanceCount == 0)
                return;

            if (pix) PixHelper.BeginEvent(Color.Red, Name);

            var newTopology = Geometry.PrimitiveTopology;

            if (newTopology != context.InputAssembler.PrimitiveTopology)
            {
                context.InputAssembler.PrimitiveTopology = Geometry.PrimitiveTopology;
            }

            context.InputAssembler.SetVertexBuffers(0, Bindings);
            context.InputAssembler.SetIndexBuffer(Geometry.IndexBuffer, Format.R32_UInt, 0);
            
            context.DrawIndexedInstanced(Geometry.IndexCount, InstanceCount, 0, 0, 0);

            if (pix) PixHelper.EndEvent();
        }

        /// <summary>
        /// Adds an <see cref="IRenderable" /> instance to the instance list.
        /// </summary>
        public void Add(IRenderable renderable)
        {
            lock (this)
            {
                renderable.RenderIndex = _renderables.Count;
                renderable.RenderObject = this;
                _renderables.Add(renderable);
                _sizeInBytes += InstanceRenderData.Stride;
                ExpandInstanceBuffers();
                UpdateInstance(renderable);
            }
        }

        /// <summary>
        /// Remove an <see cref="IRenderable" /> instance from the instance list.
        /// </summary>
        public void Remove(IRenderable renderable)
        {
            lock (this)
            {
                try
                {
                    var index = renderable.RenderIndex;

                    if (_renderables.Count == 0)
                        return;

                    var lastIndex = _renderables.Count - 1;

                    if (lastIndex == -1 && lastIndex < _renderables.Count)
                        return;

                    var lastItem = _renderables[lastIndex];

                    _renderables[index] = lastItem; // replace the removing instance with the last instance
                    lastItem.RenderIndex = index;
                    _renderables.RemoveAt(lastIndex); // delete the last intance

                    renderable.RenderIndex = -1;
                    renderable.RenderObject = null;
                    _sizeInBytes -= InstanceRenderData.Stride;
                    UpdateInstance(lastItem);
                    _instanceBufferCpu.Position = _sizeInBytes;
                }
                catch (IndexOutOfRangeException e)
                {
                    Debug.Fail(e.Message);
                }
            }
        }

        /// <summary>
        /// Clears the instance list.
        /// </summary>
        public void Clear()
        {
            lock (this)
            {
                _renderables.ForEach(r =>
                {
                    r.RenderIndex = -1;
                    r.RenderObject = null;
                });
                _renderables.Clear();
                _sizeInBytes = 0;
            }
        }

        internal void UpdateInstance(IRenderable instance)
        {
            lock (this)
            {
                if (_instanceBufferCpu == null) return;
                if (instance.RenderIndex == -1) return;
                _instanceBufferCpu.Position = InstanceRenderData.Stride * instance.RenderIndex;
                _instanceBufferCpu.Write(instance.RenderData);
                _bufferDirty = true;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"RBO_{Name}";
        }
    }
}