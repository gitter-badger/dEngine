// TextTest.cs - dEngine
// Copyright (C) 2016-2016 DanDevPC (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using dEngine.Graphics;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using Factory2D = SharpDX.Direct2D1.Factory;

namespace dEngine.Instances
{
    internal class CustomTextRenderer : TextRenderer
    {
        private readonly Factory2D _factory;
        public Brush FillBrush;
        public Brush OutlineBrush;
        public float StrokeWidth;

        public CustomTextRenderer(Factory2D factory)
        {
            _factory = factory;
        }

        public bool IsPixelSnappingDisabled(object clientDrawingContext)
        {
            return false;
        }

        public RawMatrix3x2 GetCurrentTransform(object clientDrawingContext)
        {
            return Renderer.Context2D.Transform;
        }

        public float GetPixelsPerDip(object clientDrawingContext)
        {
            return Renderer.Context2D.DotsPerInch.Width/96.0f;
        }

        public Result DrawGlyphRun(object clientDrawingContext, float baselineOriginX, float baselineOriginY,
            MeasuringMode measuringMode, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription,
            ComObject clientDrawingEffect)
        {
            using (var path = new PathGeometry(_factory))
            using (var sink = path.Open())
            {
                glyphRun.FontFace.GetGlyphRunOutline(glyphRun.FontSize, glyphRun.Indices, glyphRun.Advances,
                    glyphRun.Offsets, glyphRun.IsSideways, (glyphRun.BidiLevel % 2) > 0, sink);
                sink.Close();

                var matrix = new Matrix3x2(1.0f, 0.0f, 0.0f, 1.0f, Location.X + baselineOriginX, Location.Y + baselineOriginY);
                using (var transformedGeometry = new TransformedGeometry(_factory, path, matrix))
                {
                    Renderer.Context2D.AntialiasMode = AntialiasMode.Aliased;
                    Renderer.Context2D.DrawGeometry(transformedGeometry, OutlineBrush, StrokeWidth);
                    Renderer.Context2D.AntialiasMode = AntialiasMode.PerPrimitive;
                    Renderer.Context2D.FillGeometry(transformedGeometry, FillBrush);
                }
            }
            return Result.Ok;
        }

        public Result DrawUnderline(object clientDrawingContext, float baselineOriginX, float baselineOriginY, ref Underline underline,
            ComObject clientDrawingEffect)
        {
            return new Result();
        }

        public Result DrawStrikethrough(object clientDrawingContext, float baselineOriginX, float baselineOriginY,
            ref Strikethrough strikethrough, ComObject clientDrawingEffect)
        {
            return new Result();
        }

        public Result DrawInlineObject(object clientDrawingContext, float originX, float originY, InlineObject inlineObject,
            bool isSideways, bool isRightToLeft, ComObject clientDrawingEffect)
        {
            return new Result();
        }

        IDisposable ICallbackable.Shadow { get; set; }
        public SharpDX.Vector2 Location;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }

        ~CustomTextRenderer()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            ICallbackable callbackable = this;
            if (callbackable.Shadow == null)
                return;
            callbackable.Shadow.Dispose();
            callbackable.Shadow = null;
        }
    }
}