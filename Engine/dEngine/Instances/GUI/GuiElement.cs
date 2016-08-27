// GuiElement.cs - dEngine
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
using dEngine.Graphics;
using dEngine.Instances.Attributes;
using dEngine.Services;
using SharpDX;
using SharpDX.Direct2D1;

namespace dEngine.Instances
{
    /// <summary>
    /// Base class for all 2D GUI elements.
    /// </summary>
    [TypeId(66)]
    public abstract class GuiElement : GuiBase2D
    {
        private AlignmentX _alignmentX;
        private AlignmentY _alignmentY;
        private Brush _backgroundBrush;
        private Colour _backgroundColour;
        private Brush _borderBrush;
        private Colour _borderColour;
        private RoundedRectangle _borderRectangle;
        private float _borderThickness;
        private bool _clipping;
        private LayerCollector _container;
        private float _cornerRadius;
        private int _frameIndex;
        private UDim2 _position;
        private float _rotation;

        protected RoundedRectangle _roundedRectangle;
        private int _shadowBlur;
        private DropShadowEffect _shadowEffect;
        private Vector2 _shadowOffset;
        private int _shadowWidth;
        private UDim2 _size;
        private bool _visible;
        private int _zIndex;

        internal RectangleF LayoutRect;
        internal RectangleF RenderRect;

        /// <inheritdoc />
        protected GuiElement()
        {
            Position = new UDim2(0, 0, 0, 0);
            Size = new UDim2(0, 100, 0, 100);
            CornerRadius = 0;
            ZIndex = 0;
            Visible = true;
            BorderThickness = 1;
            BorderColour = Colour.Black;
            BackgroundColour = Colour.White;
            IsHitTestVisible = true;

            Measured = new Signal(this);
            MouseButton1Click = new Signal(this);
            MouseButton1Down = new Signal<int, int>(this);
            MouseButton1Up = new Signal<int, int>(this);
            MouseButton2Click = new Signal(this);
            MouseButton2Down = new Signal<int, int>(this);
            MouseButton2Up = new Signal<int, int>(this);
            MouseEnter = new Signal<int, int>(this);
            MouseLeave = new Signal<int, int>(this);
            MouseMove = new Signal<int, int>(this);
            MouseWheel = new Signal<int, int, float>(this);
            GotFocus = new Signal(this);
            LostFocus = new Signal(this);

            MouseEnter.Event += OnMouseEnter;

            Renderer.InvokeResourceDependent(CreateResources);
        }

        /// <summary>
        /// Determines if the mouse is currently hovering over this element.
        /// </summary>
        public bool IsMouseOver { get; set; }

        /// <summary>
        /// Sets the transparency of <see cref="BackgroundColour" />.
        /// </summary>
        public float BackgroundTransparency
        {
            get { return _backgroundColour.A; }
            set
            {
                BackgroundColour = new Colour(_backgroundColour.R, _backgroundColour.G, _backgroundColour.B, 1 - value);
            }
        }

        /// <summary>
        /// The colour of the background.
        /// </summary>
        [InstMember(1), EditorVisible("Appearance")]
        public virtual Colour BackgroundColour
        {
            get { return _backgroundColour; }
            set
            {
                if (_backgroundColour == value) return;
                _backgroundColour = value;
                _backgroundBrush = Renderer.Brushes?.Get(value);
                NotifyChanged();
            }
        }

        /// <summary>
        /// The Position of the element relative to its parent.
        /// </summary>
        [InstMember(2), EditorVisible("Data")]
        public UDim2 Position
        {
            get { return _position; }
            set
            {
                if (_position == value) return;
                _position = value;
                Measure();
                NotifyChanged();
            }
        }

        /// <summary>
        /// The size of the element.
        /// </summary>
        [InstMember(3), EditorVisible("Data")]
        public UDim2 Size
        {
            get { return _size; }
            set
            {
                if (_size == value) return;
                _size = value;
                Measure();
                NotifyChanged();
            }
        }

        public bool Focused { get; internal set; }

        /// <summary>
        /// The radius of each corner.
        /// </summary>
        [InstMember(4), EditorVisible("Data")]
        public float CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                if (_cornerRadius == value) return;
                _cornerRadius = Math.Max(0, value);
                Arrange();
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines if this element can be rendered.
        /// </summary>
        [InstMember(5), EditorVisible("Data")]
        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (_visible == value) return;
                _visible = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines if overflowing descendants are clipped.
        /// </summary>
        [InstMember(6), EditorVisible("Behaviour")]
        public bool ClipDescendants
        {
            get { return _clipping; }
            set
            {
                if (_clipping == value) return;
                _clipping = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The layer that this element is drawn at.
        /// </summary>
        [InstMember(7), EditorVisible("Data")]
        public int ZIndex
        {
            get { return _zIndex; }
            set
            {
                if (_zIndex == value) return;

                value = Math.Max(0, value);

                Container?.RemoveZIndexed(this);

                _zIndex = value;

                if (value > 0)
                    Container?.AddZIndexed(this);

                NotifyChanged();
            }
        }

        /// <summary>
        /// The clockwise rotation of the gui in degrees.
        /// </summary>
        [InstMember(8), EditorVisible("Data")]
        public float Rotation
        {
            get { return _rotation; }
            set
            {
                if (_rotation == value) return;
                _rotation = value;

                NotifyChanged();
            }
        }

        /// <summary>
        /// The thickness of the border.
        /// </summary>
        [InstMember(9), EditorVisible("Appearance")]
        public float BorderThickness
        {
            get { return _borderThickness; }
            set
            {
                if (value == _borderThickness)
                    return;

                _borderThickness = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// The colour of the border.
        /// </summary>
        [InstMember(10), EditorVisible("Appearance")]
        public Colour BorderColour
        {
            get { return _borderColour; }
            set
            {
                if (value == _borderColour) return;
                _borderColour = value;
                _borderBrush = Renderer.Brushes?.Get(value);
                NotifyChanged();
            }
        }

        /// <summary>
        /// The index for the parent frame. Used for <see cref="Stack" /> and <see cref="Flex" />.
        /// </summary>
        [InstMember(11), EditorVisible("Data")]
        public int FrameIndex
        {
            get { return _frameIndex; }
            set
            {
                if (_frameIndex == value) return;
                _frameIndex = value;

                var stack = Parent as Stack;
                stack?.Update(this);

                NotifyChanged();
            }
        }

        /// <summary>
        /// The horizontal alignment of the element.
        /// </summary>
        [InstMember(12), EditorVisible("Data")]
        public AlignmentX AlignmentX
        {
            get { return _alignmentX; }
            set
            {
                if (value == _alignmentX) return;
                _alignmentX = value;
                Measure();
                NotifyChanged();
            }
        }

        /// <summary>
        /// The vertical alignment of the element.
        /// </summary>
        [InstMember(13), EditorVisible("Data")]
        public AlignmentY AlignmentY
        {
            get { return _alignmentY; }
            set
            {
                if (value == _alignmentY) return;
                _alignmentY = value;
                Measure();
                NotifyChanged();
            }
        }

        /// <summary>
        /// Determines whether this element is ingored by hit tests.
        /// </summary>
        [InstMember(15), EditorVisible("Behaviour")]
        public bool IsHitTestVisible
        {
            get { return _isHitTestVisible; }
            set
            {
                if (value == _isHitTestVisible) return;
                _isHitTestVisible = value;
                Measure();
                NotifyChanged();
            }
        }

        /// <summary />
        [EditorVisible("Appearance")]
        public Vector2 ShadowOffset
        {
            get { return _shadowOffset; }
            set
            {
                if (value == _shadowOffset)
                    return;

                _shadowOffset = value;

                if (_shadowEffect != null)
                    _shadowEffect.Offset = value;

                NotifyChanged(nameof(ShadowOffset));
            }
        }

        /// <summary />
        [EditorVisible("Appearance")]
        public int ShadowWidth
        {
            get { return _shadowWidth; }
            set
            {
                if (value == _shadowWidth)
                    return;

                _shadowWidth = value;

                if (_shadowEffect != null)
                    _shadowEffect.WideningRadius = value;

                NotifyChanged(nameof(ShadowWidth));
            }
        }

        /// <summary />
        [EditorVisible("Appearance")]
        public int ShadowBlur
        {
            get { return _shadowBlur; }
            set
            {
                if (value == _shadowBlur)
                    return;

                _shadowBlur = value;

                if (_shadowEffect != null)
                    _shadowEffect.BlurSize = value;

                NotifyChanged(nameof(ShadowBlur));
            }
        }

        /// <summary>
        /// The container that this element is a descendant of.
        /// </summary>
        [EditorVisible("Data")]
        public LayerCollector Container
        {
            get { return _container; }
            private set
            {
                if (value == _container)
                    return;

                if (_container != null)
                {
                    lock (_container)
                    {
                        _container.ZIndexedElements.Remove(this);
                    }
                }

                _container = value;

                if (value != null)
                {
                    if (ZIndex > 0)
                    {
                        lock (_container)
                        {
                            value.ZIndexedElements.Add(this);
                        }
                    }
                }

                NotifyChanged(nameof(Container));
            }
        }

        /// <summary>
        /// Gets the absolute size of the element, with scale taken into account.
        /// </summary>
        [EditorVisible("Data")]
        public override Vector2 AbsoluteSize { get; internal set; }

        /// <summary>
        /// Gets the absolute size of the element, with scale taken into account.
        /// </summary>
        [EditorVisible("Data")]
        public override Vector2 AbsolutePosition { get; internal set; }

        /// <summary>
        /// Determines if this element can be focused.
        /// </summary>
        public bool Focusable { get; protected set; }

        /// <summary>
        /// Determines if the text box is focused.
        /// </summary>
        /// <returns></returns>
        public bool IsFocused()
        {
            return Focused;
        }

        /// <summary>
        /// Invoked on <see cref="MouseEnter" />.
        /// </summary>
        protected virtual void OnMouseEnter(int i, int i1)
        {
            IsMouseOver = true;
        }

        /// <summary>
        /// Invoked on <see cref="MouseLeave" />.
        /// </summary>
        protected virtual void OnMouseLeave(int i, int i1)
        {
            IsMouseOver = false;
        }

        /// <summary>
        /// Callback to create D2D resources.
        /// </summary>
        protected virtual void CreateResources()
        {
            //_shadowEffect = new DropShadowEffect();
        }

        /// <inheritdoc />
        protected override void OnAncestryChanged(Instance child, Instance parent)
        {
            base.OnAncestryChanged(child, parent);
            Instance ancestor = this;

            if (child == this)
            {
                Measure();
            }

            while (ancestor != null)
            {
                ancestor = ancestor.Parent;
                var container = ancestor as LayerCollector;
                Container = container;
                if (container != null)
                    break;
            }
        }

        /// <inheritdoc />
        public void Focus()
        {
            InputService.FocusedElement = this;
        }

        /// <inheritdoc />
        public void Unfocus()
        {
            InputService.FocusedElement = null;
        }

        /// <summary>
        /// Updates <see cref="AbsoluteSize" /> and <see cref="AbsolutePosition" />.
        /// </summary>
        public virtual void Measure()
        {
            var parentGui = Parent as GuiBase2D;
            var parentSize = parentGui?.AbsoluteSize ?? (parentGui?.AbsoluteSize ?? new Vector2());
            var parentPosition = parentGui?.AbsolutePosition ?? new Vector2();

            AbsoluteSize = Size.toAbsolute(parentSize).round();

            var position = Position;

            var xScaleOffset = 0.0f;
            var yScaleOffset = 0.0f;
            var xAbsoluteOffset = 0;
            var yAbsoluteOffset = 0;
            var xMult = 1;
            var yMult = 1;

            if (_alignmentX == AlignmentX.Right)
            {
                xScaleOffset = 1;
                xAbsoluteOffset = (int)-AbsoluteSize.X;
                xMult = -1;
            }
            else if (_alignmentX == AlignmentX.Center)
            {
                xScaleOffset = 0.5f;
                xAbsoluteOffset = (int)-AbsoluteSize.X / 2;
            }

            if (_alignmentY == AlignmentY.Bottom)
            {
                yScaleOffset = 1;
                yAbsoluteOffset = (int)-AbsoluteSize.Y;
                yMult = -1;
            }
            else if (_alignmentY == AlignmentY.Middle)
            {
                yScaleOffset = 0.5f;
                yAbsoluteOffset = (int)-AbsoluteSize.Y / 2;
            }

            position = new UDim2(xScaleOffset + position.Scale.X * xMult, xAbsoluteOffset + position.Absolute.X * xMult,
                yScaleOffset + position.Scale.Y * xMult, yAbsoluteOffset + position.Absolute.Y * yMult);

            AbsolutePosition = (parentPosition + position.toAbsolute(parentSize)).round();

            Measured?.Fire();

            Arrange();

            foreach (var kv in Children)
            {
                var el = kv as GuiElement;
                el?.Measure();
            }
        }

        /// <summary>
        /// Updates <see cref="RenderRect" />.
        /// </summary>
        public virtual void Arrange()
        {
            LayoutRect = new RectangleF(AbsolutePosition.x, AbsolutePosition.y, AbsoluteSize.x, AbsoluteSize.y);
            RenderRect = new RectangleF(-AbsoluteSize.x / 2, -AbsoluteSize.y / 2, AbsoluteSize.x, AbsoluteSize.y);
            var borderRect = new RectangleF(-AbsoluteSize.x / 2 - .5f, -AbsoluteSize.y / 2 - .5f, AbsoluteSize.x,
                AbsoluteSize.y);

            _roundedRectangle = new RoundedRectangle
            {
                RadiusX = CornerRadius,
                RadiusY = CornerRadius,
                Rect = RenderRect
            };

            borderRect = RenderRect;
            borderRect.Inflate(_borderThickness, _borderThickness);

            _borderRectangle = new RoundedRectangle
            {
                RadiusX = CornerRadius,
                RadiusY = CornerRadius,
                Rect = RenderRect
            };
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();
            _shadowEffect?.Dispose();
        }

        internal override bool CheckCanDraw()
        {
            return !IsDestroyed && Visible;
        }

        internal virtual void Draw()
        {
            if (_backgroundColour.A == 0)
                return;

            var collector = Container;

            if (collector == null)
                return;

            lock (collector)
            {
                var context = Renderer.Context2D;

                if (_borderThickness > 0 && _borderColour.a > 0)
                    context.DrawRoundedRectangle(_borderRectangle, _borderBrush, _borderThickness * 2);

                if (_cornerRadius > 0)
                    context.FillRoundedRectangle(_roundedRectangle, _backgroundBrush);
                else
                    context.FillRectangle(RenderRect, _backgroundBrush);
            }
        }

        #region Events

        /// <summary>
        /// Fired when the element is resized or repositioned.
        /// </summary>
        public readonly Signal Measured;

        /// <summary>
        /// Fired when the mouse enters this element.
        /// </summary>
        public readonly Signal<int, int> MouseEnter;

        /// <summary>
        /// Fired when the mouse leaves this element.
        /// </summary>
        public readonly Signal<int, int> MouseLeave;

        /// <summary>
        /// Fired when the mouse moves inside this element.
        /// </summary>
        public readonly Signal<int, int> MouseMove;

        /// <summary>
        /// Fired when the mouse scrolls inside this element.
        /// </summary>
        public readonly Signal<int, int, float> MouseWheel;

        /// <summary>
        /// Fired when this element gains focus.
        /// </summary>
        public readonly Signal GotFocus;

        /// <summary>
        /// Fired when this element loses focus.
        /// </summary>
        public readonly Signal LostFocus;

        /// <summary>
        /// Fires when the mouse has fully left clicked the button.
        /// </summary>
        public readonly Signal MouseButton1Click;

        /// <summary>
        /// Fires when the left mouse button has been pressed over this element.
        /// </summary>
        public readonly Signal<int, int> MouseButton1Down;

        /// <summary>
        /// Fires when the left mouse button has been released over this element.
        /// </summary>
        public readonly Signal<int, int> MouseButton1Up;

        /// <summary>
        /// Fires when the mouse has fully right clicked the button.
        /// </summary>
        public readonly Signal MouseButton2Click;

        /// <summary>
        /// Fires when the right mouse button has been pressed over this element.
        /// </summary>
        public readonly Signal<int, int> MouseButton2Down;

        /// <summary>
        /// Fires when the right mouse button has been released over this element.
        /// </summary>
        public readonly Signal<int, int> MouseButton2Up;

        private bool _isHitTestVisible;

        #endregion
    }
}