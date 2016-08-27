// PhysicsDebugDraw.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using BulletSharp;
using C5;
using dEngine.Graphics.Structs;
using dEngine.Instances;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Matrix = BulletSharp.Math.Matrix;
using Point = dEngine.Graphics.Structs.Point;

namespace dEngine.Graphics
{
    internal class PointArray : ArrayList<Point>
    {
        public Point[] Array => array;
    }

    internal class PhysicsDebugDraw : DebugDraw
    {
        private static readonly GfxShader LineShader = Shaders.Get("Line");
        private static readonly GfxShader.Pass LinePass = LineShader.GetPass("Main");
        private static RenderObject _capsuleHeadRO;
        private static RenderObject _capsuleBodyRO;
        private readonly PointArray _points;
        private bool _bufferDirty;
        private Camera _camera;

        private DeviceContext _context;
        private int _lastLength = -1;
        private Buffer _lineBuffer;
        private VertexBufferBinding _lineBufferBinding;
        private int _lineCount;

        static PhysicsDebugDraw()
        {
            /*
            var capsuleStream = ContentProvider.DownloadStream("internal://content/meshes/capsule2.dae").Result;

            var capsuleScene = ContentProvider.AssimpContext.ImportFileFromStream(capsuleStream, PostProcessSteps.None, ".dae");

            var headGeo = capsuleScene.Meshes.Find(m => m.Name == "Head");
            var bodyGeo = capsuleScene.Meshes.Find(m => m.Name == "Body");

            _capsuleHeadRO = new RenderObject("CapsuleHead", headGeo);
            _capsuleBodyRO = new RenderObject("CapsuleBody", bodyGeo);
            */
        }

        /// <summary />
        public PhysicsDebugDraw()
        {
            _points = new PointArray();
        }

        public override DebugDrawModes DebugMode { get; set; } = DebugDrawModes.DrawWireframe;

        /*
        public override void DrawCapsule(float radius, float halfHeight, int upAxis, ref Matrix transform,
            ref BulletSharp.Math.Vector3 color)
        {
            _capsuleHeadRO.Add(new CapsuleHead(radius, halfHeight, upAxis, ref transform, ref color));
            _capsuleBodyRO.Add(new CapsuleBody(radius, halfHeight, upAxis, ref transform, ref color));
        }
        */

        public void Draw(ref DeviceContext context, ref Camera camera)
        {
            _context = context;
            _camera = camera;

            /*
            _capsuleHeadRO.Draw(ref _context, ref _camera);
            _capsuleHeadRO.Draw(ref _context, ref _camera);
            _capsuleBodyRO.Draw(ref _context, ref _camera);

            _capsuleHeadRO.Clear();
            _capsuleBodyRO.Clear();
            */

            if (_lineCount > 0)
            {
                if (_bufferDirty)
                {
                    var length = Point.Stride * 2 * _lineCount;

                    if (length > _lastLength)
                    {
                        _lineBuffer = Buffer.Create(Renderer.Device, BindFlags.VertexBuffer, _points.Array, 0,
                            ResourceUsage.Dynamic, CpuAccessFlags.Write);
                        _lineBufferBinding = new VertexBufferBinding(_lineBuffer, Point.Stride, 0);
                        _lastLength = length;
                    }
                    else
                    {
                        DataStream stream;
                        context.MapSubresource(_lineBuffer, MapMode.WriteDiscard, MapFlags.None, out stream);

                        var count = _points.Count;
                        for (int i = 0; i < count; i++)
                            stream.Write(_points[i]);

                        context.UnmapSubresource(_lineBuffer, 0);
                        _bufferDirty = false;
                    }
                }

                LinePass.Use(ref context);
                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
                context.InputAssembler.SetVertexBuffers(0, _lineBufferBinding);
                context.Draw(_lineCount * 2, _points.Offset);

                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList; // reset topology
                _points.Clear();
                _lineCount = 0;
            }
        }

        public override void DrawLine(ref BulletSharp.Math.Vector3 from, ref BulletSharp.Math.Vector3 to,
            ref BulletSharp.Math.Vector3 color)
        {
            var pointA = new Point(ref from, ref color);
            var pointB = new Point(ref to, ref color);

            _points.Add(pointA);
            _points.Add(pointB);

            _lineCount++;

            _bufferDirty = true;
        }

        public override void Draw3dText(ref BulletSharp.Math.Vector3 location, string textString)
        {
        }

        public override void ReportErrorWarning(string warningString)
        {
            Engine.Logger.Warn(warningString);
        }

        internal class CapsuleHead : CapsulePart
        {
            public CapsuleHead(float radius, float halfHeight, int upAxis, ref Matrix transform,
                ref BulletSharp.Math.Vector3 color) : base(radius, halfHeight, upAxis, ref transform, ref color)
            {
            }

            public override void UpdateRenderData()
            {
                base.UpdateRenderData();
                _renderData.Size = new Vector3(_radius);
            }
        }

        internal class CapsuleBody : CapsulePart
        {
            public CapsuleBody(float radius, float halfHeight, int upAxis, ref Matrix transform,
                ref BulletSharp.Math.Vector3 color) : base(radius, halfHeight, upAxis, ref transform, ref color)
            {
            }

            public override void UpdateRenderData()
            {
                base.UpdateRenderData();
                _renderData.Size = new Vector3(_radius, _halfHeight * 2, _radius);
            }
        }

        internal abstract class CapsulePart : IRenderable
        {
            protected readonly BulletSharp.Math.Vector3 _color;
            protected readonly float _halfHeight;
            protected readonly float _radius;
            protected readonly Matrix _transform;
            protected InstanceRenderData _renderData;

            protected CapsulePart(float radius, float halfHeight, int upAxis, ref Matrix transform,
                ref BulletSharp.Math.Vector3 color)
            {
                _radius = radius;
                _halfHeight = halfHeight;
                _transform = transform;
                _color = color;
                throw new System.NotImplementedException();
            }

            public RenderObject RenderObject { get; set; }
            public int RenderIndex { get; set; }

            public InstanceRenderData RenderData
            {
                get { return _renderData; }
                set { _renderData = value; }
            }

            public virtual void UpdateRenderData()
            {
                _renderData.ModelMatrix = (SharpDX.Matrix)(CFrame)_transform;
                _renderData.Colour = new Colour(_color.X, _color.Y, _color.Z);
            }
        }
    }
}