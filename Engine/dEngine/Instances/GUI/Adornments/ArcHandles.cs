// ArcHandles.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Graphics;
using dEngine.Graphics.Structs;
using dEngine.Instances.Attributes;
using dEngine.Utility;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace dEngine.Instances
{
    /// <summary>
    /// An object which draws 3D handles around the axes of <see cref="PVAdornment.Adornee" />.
    /// </summary>
    [TypeId(169), ToolboxGroup("3D GUI"), ExplorerOrder(20)]
    public class ArcHandles : BaseHandles
    {
        /// <summary>
        /// Fires when the left mouse button is pressed on a handle.
        /// </summary>
        public readonly Signal<Axis> LeftMouseButtonDown;

        /// <summary>
        /// Fires when the mouse moves while the left mouse button is held.
        /// </summary>
        public readonly Signal<Axis, float, float> MouseDrag;

        /// <summary>
        /// Fires when the mouse enters the handle.
        /// </summary>
        public readonly Signal<Axis> MouseEnter;

        /// <summary>
        /// Fires when the mouse leaves the handle.
        /// </summary>
        public readonly Signal<Axis> MouseLeave;

        /// <summary>
        /// Fires when the left mouse button is pressed on a handle.
        /// </summary>
        public readonly Signal<Axis> RightMouseButtonUp;

        private CFrame _cframe;
        private SharpDX.Ray _ray;
        private readonly ShapeAdornment[] _spheres;
        private readonly ShapeAdornment[] _cylinders;
        private BoundingBox[] _testers;
        private float[] _scales;

        private const int _segments = 30;

        /// <summary/>
        public ArcHandles()
        {
            _cframe = CFrame.Identity;
            _spheres = new ShapeAdornment[6];
            _testers = new BoundingBox[6];
            _scales = new float[6];
            _cylinders = new ShapeAdornment[_segments * 3];

            var colours = new[]
            {
                Colour.Red,
                Colour.Green,
                Colour.Blue,
                Colour.Red,
                Colour.Green,
                Colour.Blue
            };

            for (int i = 0; i < 6; i++)
            {
                _spheres[i] = new ShapeAdornment(Shape.Sphere) { Colour = colours[i] };
                _testers[i] = new BoundingBox();
            }

            for (int i = 0; i < _segments * 3; i++)
            {
                Colour colour;
                if (i < _segments)
                    colour = Colour.Red;
                else if (i < _segments * 2)
                    colour = Colour.Blue;
                else
                    colour = Colour.Green;

                _cylinders[i] = new ShapeAdornment(Shape.Cylinder) { Colour = colour };
            }

            LeftMouseButtonDown = new Signal<Axis>(this);
            RightMouseButtonUp = new Signal<Axis>(this);
            MouseDrag = new Signal<Axis, float, float>(this);
            MouseEnter = new Signal<Axis>(this);
            MouseLeave = new Signal<Axis>(this);
        }

        private static readonly Vector3 _cylinderSize = new Vector3(0.5f, 53.9f, 0.5f);

        internal override void Draw(ref DeviceContext context, ref Camera camera)
        {
            var adornee = Adornee;
            if (adornee == null || Visible == false)
                return;

            PixHelper.BeginEvent(Color.Zero, "ArcHandles");

            Renderer.AdornSelfLitPass.Use(ref context);

            var adorneeCF = adornee.CFrame;
            var adorneeSize = adornee.Size;
            GetDistanceToNormals(10, 0.6f, ref adorneeCF, ref adorneeSize, ref _scales);

            var radius = adorneeSize.max()*0.75f;

            for (int i = 0; i < 6; i++)
            {
                var sphere = _spheres[i];
                var normalId = (NormalId)i;
                sphere.CFrame = adorneeCF + (Vector3.FromNormalId(normalId) * radius);
                sphere.Size = new Vector3(_scales[i]);
                sphere.UpdateRenderData();
                sphere.Draw(ref context, ref camera);
            }

            for (int i = 0; i < 3; i++)
            {
                CFrame angle;

                if (i == 0)
                    angle = CFrame.Identity;
                else if (i == 1)
                    angle = CFrame.Angles(0, Mathf.Pi / 2, 0);
                else
                    angle = CFrame.Identity;

                for (int j = 0; j < _segments; j++)
                {
                    var cylinder = _cylinders[i * j];
                    cylinder.Size = _cylinderSize * (radius / 256);
                    var sine = Mathf.Sin((360 / _segments + 360 / _segments * j) / (180 / Mathf.Pi));
                    var cosine = Mathf.Cos((360 / _segments + 360 / _segments * j) / (180 / Mathf.Pi));
                    var cframe = (adorneeCF) + new Vector3(radius * sine, 0, radius * cosine);
                    cylinder.CFrame = cframe *
                                      CFrame.Angles(0, (360 / _segments + 360 / _segments * j) / (180 / Mathf.Pi),
                                          Mathf.Pi / 2);
                    cylinder.UpdateRenderData();
                    cylinder.Draw(ref context, ref camera);
                }
            }

            PixHelper.EndEvent();
        }

        internal class ShapeAdornment : IRenderable
        {
            public ShapeAdornment(Shape shape)
            {
                _renderObject = new RenderObject($"ArcHandles_{shape}", Primitives.GeometryFromShape(shape));
                _renderObject.Add(this);
            }

            public RenderObject RenderObject
            {
                get { return _renderObject; }
                set { _renderObject = value; }
            }

            public int RenderIndex { get; set; }
            public InstanceRenderData RenderData { get; set; }
            
            public Vector3 Size;
            public CFrame CFrame;
            public Colour Colour;
            public float Transparency;
            private RenderObject _renderObject;

            public void UpdateRenderData()
            {
                RenderData = new InstanceRenderData
                {
                    ModelMatrix = CFrame.GetModelMatrix(),
                    Size = Size,
                    Colour = Colour,
                    Transparency = Transparency
                };
                _renderObject.UpdateInstance(this);
            }


            public void Draw(ref DeviceContext context, ref Camera camera)
            {
                _renderObject.Draw(ref context, ref camera, false);
            }
        }
    }
}