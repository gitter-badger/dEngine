// Texture.cs - dEngine
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
using System.IO;
using dEngine.Graphics;
using dEngine.Instances.Attributes;
using dEngine.Utility;
using dEngine.Utility.Texture;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WIC;
using Bitmap = SharpDX.WIC.Bitmap;
using PixelFormat = SharpDX.WIC.PixelFormat;

#pragma warning disable 1591

namespace dEngine.Data
{
    /// <summary>
    /// A 2D texture.
    /// </summary>
    [TypeId(22)]
    public class Texture : AssetBase
    {
        // .PNG
        internal const uint MagicPNG = 0x89 | ('P' << 8) | ('N' << 16) | ('G' << 24);
        // ÿØÿà
        internal const uint MagicJPEG = 0xFF | (0xD8 << 8);
        // BM
        internal const uint MagicBMP = 'B' | ('M' << 8);
        // GIF89
        internal const uint MagicGIF = 'G' | ('I' << 8) | ('F' << 16) | (89 << 24);
        // RIFF
        internal const int MagicRIFF = ('R' | ('I' << 8) | ('F' << 16) | ('F' << 24));
        internal readonly List<DepthStencilView> DsvArraySlices;

        internal readonly List<RenderTargetView> RtvArraySlices;
        internal readonly List<ShaderResourceView> SrvArraySlices;

        [InstMember(1)]
        private byte[] _bytes;

        private Texture2D _nativeTexture;

        /// <summary>
        /// Creates an empty texture.
        /// </summary>
        public Texture()
        {
            RtvArraySlices = new List<RenderTargetView>();
            SrvArraySlices = new List<ShaderResourceView>();
            DsvArraySlices = new List<DepthStencilView>();
        }

        /// <summary>
        /// Creates a texture with an existing <see cref="Texture2D" />.
        /// </summary>
        public Texture(Texture2D texture) : this()
        {
            NativeTexture = texture;

            LoadArraySlices(NativeTexture.Description.BindFlags);

            IsLoaded = true;
        }

        /// <summary>
        /// Loads a bitmap into a texture.
        /// </summary>
        public Texture(System.Drawing.Bitmap bitmap, BindFlags flags) : this()
        {
            LoadBitmap(bitmap, flags);
            LoadArraySlices(flags);

            IsLoaded = true;
        }

        /// <summary>
        /// Creates a texture with the given width, height and format.
        /// </summary>
        public Texture(int width, int height, Format format, bool mipmap, BindFlags bindFlags = BindFlags.ShaderResource,
            int arraySize = 1) : this()
        {
            var flags = ResourceOptionFlags.None;

            if (mipmap)
                flags |= ResourceOptionFlags.GenerateMipMaps;

            NativeTexture = new Texture2D(Renderer.Device, new Texture2DDescription
            {
                Width = width,
                Height = height,
                ArraySize = arraySize,
                BindFlags = bindFlags,
                Usage = ResourceUsage.Default,
                Format = format,
                MipLevels = mipmap ? 0 : 1,
                OptionFlags = flags,
                CpuAccessFlags = CpuAccessFlags.None,
                SampleDescription = new SampleDescription(1, 0)
            });

            LoadArraySlices(bindFlags);

            if (mipmap)
                Renderer.Context.GenerateMips(SrvArraySlices[0]);

            IsLoaded = true;
        }

        internal Texture(Texture2D texture, DepthStencilViewDescription depthStencilViewDescription,
            ShaderResourceViewDescription shaderResourceViewDescription) : this()
        {
            NativeTexture = texture;
            DsvArraySlices.Add(new DepthStencilView(Renderer.Device, texture, depthStencilViewDescription) { Tag = this, DebugName = Name });
            SrvArraySlices.Add(new ShaderResourceView(Renderer.Device, texture, shaderResourceViewDescription) { Tag = this, DebugName = Name });

            IsLoaded = true;
        }

        /// <summary>
        /// The <see cref="Texture2D" />
        /// </summary>
        public Texture2D NativeTexture
        {
            get { return _nativeTexture; }
            private set
            {
                _nativeTexture = value;
                value.DebugName = Name;
                value.Tag = this;
                UpdateDimensions();
            }
        }

        /// <inheritdoc />
        public override ContentType ContentType => ContentType.Texture;

        /// <summary>
        /// Determines if this is a staging texture.
        /// </summary>
        public bool IsStaging { get; set; }

        /// <summary>
        /// The width of the texture.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The height of the texture.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The texel size of the texture.
        /// </summary>
        public Vector4 TexelSize { get; private set; }

        public string Name
        {
            get { return NativeTexture.DebugName; }
            set
            {
                NativeTexture.DebugName = value;

                foreach (var rtv in RtvArraySlices)
                    rtv.DebugName = value;
                foreach (var srv in SrvArraySlices)
                    srv.DebugName = value;
                foreach (var dsv in DsvArraySlices)
                    dsv.DebugName = value;
            }
        }

        private void LoadArraySlices(BindFlags bindFlags)
        {
            LoadArraySlices(bindFlags.HasFlag(BindFlags.RenderTarget), bindFlags.HasFlag(BindFlags.ShaderResource),
                bindFlags.HasFlag(BindFlags.DepthStencil));
        }

        private void LoadArraySlices(bool rtvFlag, bool srvFlag, bool dsvFlag)
        {
            var arraySize = NativeTexture.Description.ArraySize;
            var cubemap = NativeTexture.Description.OptionFlags.HasFlag(ResourceOptionFlags.TextureCube);

            if (cubemap)
            {
                if (rtvFlag)
                    RtvArraySlices.Add(new RenderTargetView(Renderer.Device, _nativeTexture));
                if (srvFlag)
                    SrvArraySlices.Add(new ShaderResourceView(Renderer.Device, _nativeTexture));
                if (dsvFlag)
                    throw new ArgumentException();
                return;
            }

            if (rtvFlag)
            {
                var format = NativeTexture.Description.Format;

                switch (format)
                {
                    case Format.R32_Typeless:
                        format = Format.R32_Float;
                        break;
                    case Format.R24G8_Typeless:
                    case Format.R24_UNorm_X8_Typeless:
                        format = Format.R24_UNorm_X8_Typeless;
                        break;
                }

                for (int i = 0; i < arraySize; i++)
                {
                    var rtvDesc = new RenderTargetViewDescription { Format = format };

                    if (arraySize == 1)
                    {
                        rtvDesc.Dimension = RenderTargetViewDimension.Texture2D;
                        rtvDesc.Texture2D.MipSlice = 0;
                    }
                    else
                    {
                        rtvDesc.Dimension = RenderTargetViewDimension.Texture2DArray;
                        rtvDesc.Texture2DArray.ArraySize = 1;
                        rtvDesc.Texture2DArray.FirstArraySlice = i;
                        rtvDesc.Texture2DArray.MipSlice = 0;
                    }

                    var rtv = new RenderTargetView(Renderer.Device, NativeTexture, rtvDesc) { Tag = this, DebugName = Name};

                    RtvArraySlices.Add(rtv);
                }
            }

            if (srvFlag)
            {
                var format = NativeTexture.Description.Format;

                switch (format)
                {
                    case Format.R32_Typeless:
                        format = Format.R32_Float;
                        break;
                    case Format.R24_UNorm_X8_Typeless:
                    case Format.R24G8_Typeless:
                        format = Format.R24_UNorm_X8_Typeless;
                        break;
                }

                for (int i = 0; i < arraySize; i++)
                {
                    var srvDesc = new ShaderResourceViewDescription { Format = format };

                    if (arraySize == 1)
                    {
                        srvDesc.Dimension = ShaderResourceViewDimension.Texture2D;
                        srvDesc.Texture2D.MipLevels = -1;
                        srvDesc.Texture2D.MostDetailedMip = 0;
                    }
                    else
                    {
                        srvDesc.Dimension = ShaderResourceViewDimension.Texture2DArray;
                        srvDesc.Texture2DArray.ArraySize = 1;
                        srvDesc.Texture2DArray.FirstArraySlice = i;
                        srvDesc.Texture2DArray.MipLevels = -1;
                        srvDesc.Texture2DArray.MostDetailedMip = 0;
                    }

                    var srv = new ShaderResourceView(Renderer.Device, NativeTexture, srvDesc) { Tag = this, DebugName = Name };

                    SrvArraySlices.Add(srv);
                }
            }

            if (dsvFlag)
            {
                for (int i = 0; i < arraySize; i++)
                {
                    var format = NativeTexture.Description.Format;

                    switch (format)
                    {
                        case Format.R32_Typeless:
                            format = Format.D32_Float;
                            break;
                        case Format.R24G8_Typeless:
                        case Format.R24_UNorm_X8_Typeless:
                            format = Format.D24_UNorm_S8_UInt;
                            break;
                    }

                    var dsvDesc = new DepthStencilViewDescription { Format = format };

                    if (arraySize == 1)
                    {
                        dsvDesc.Dimension = DepthStencilViewDimension.Texture2D;
                        dsvDesc.Texture2D.MipSlice = 0;
                    }
                    else
                    {
                        dsvDesc.Dimension = DepthStencilViewDimension.Texture2DArray;
                        dsvDesc.Texture2DArray.ArraySize = 1;
                        dsvDesc.Texture2DArray.FirstArraySlice = i;
                        dsvDesc.Texture2DArray.MipSlice = 0;
                    }

                    var dsv = new DepthStencilView(Renderer.Device, NativeTexture, dsvDesc) { Tag = this, DebugName = Name };

                    DsvArraySlices.Add(dsv);
                }
            }
        }

        private void UpdateDimensions()
        {
            var w = NativeTexture.Description.Width;
            var h = NativeTexture.Description.Height;
            Width = w;
            Height = h;
            TexelSize = new Vector4(1.0f / w, 1.0f / h, w, h);
        }

        public static implicit operator Texture2D(Texture texture)
        {
            return texture?.NativeTexture;
        }

        public static implicit operator RenderTargetView(Texture texture)
        {
            return texture?.RtvArraySlices.Count > 0 ? texture.RtvArraySlices[0] : null;
        }

        public static implicit operator ShaderResourceView(Texture texture)
        {
            return texture?.SrvArraySlices.Count > 0 ? texture.SrvArraySlices[0] : null;
        }

        public static implicit operator DepthStencilView(Texture texture)
        {
            return texture?.DsvArraySlices.Count > 0 ? texture.DsvArraySlices[0] : null;
        }

        protected override void OnLoad(BinaryReader reader)
        {
            lock (Renderer.Context)
            {
                var staging = IsStaging;

                Dispose(false);
                _disposed = false;

                var stream = reader.BaseStream;

                var fourCC = reader.ReadUInt32();
                var twoCC = (ushort)fourCC;
                stream.Position = 0;

                Texture2D texture;

                if (fourCC == HDRImage.Magic)
                {
                    texture = new HDRImage(stream, staging).Texture2D;
                }
                else if (fourCC == MagicPNG || fourCC == MagicBMP || fourCC == MagicGIF || twoCC == MagicJPEG ||
                         twoCC == MagicBMP || fourCC == MagicRIFF || fourCC == DDSImage.Magic)
                {
                    using (
                        var decoder = new BitmapDecoder(Renderer.ImagingFactory, stream,
                            DecodeOptions.CacheOnDemand))
                    {
                        var frame = decoder.GetFrame(0);

                        using (var converter = new FormatConverter(Renderer.ImagingFactory))
                        {
                            converter.Initialize(frame, PixelFormat.Format32bppPRGBA);

                            int stride = frame.Size.Width * 4;
                            using (var buffer = new DataStream(frame.Size.Height * stride, true, true))
                            {
                                converter.CopyPixels(stride, buffer);

                                texture = new Texture2D(Renderer.Device, new Texture2DDescription
                                {
                                    Width = frame.Size.Width,
                                    Height = frame.Size.Height,
                                    ArraySize = 1,
                                    BindFlags =
                                        staging ? BindFlags.None : (BindFlags.ShaderResource | BindFlags.RenderTarget),
                                    Usage = staging ? ResourceUsage.Staging : ResourceUsage.Default,
                                    Format = Format.R8G8B8A8_UNorm,
                                    MipLevels = 1,
                                    OptionFlags = 0,
                                    CpuAccessFlags = staging ? CpuAccessFlags.Read : CpuAccessFlags.None,
                                    SampleDescription = new SampleDescription(1, 0)
                                }, new DataRectangle(buffer.DataPointer, stride));
                            }
                        }
                    }
                }
                else
                {
                    throw new NotSupportedException("Unknown texture format or corrupt data.");
                }

                NativeTexture = texture;
                if (texture.Description.BindFlags.HasFlag(BindFlags.ShaderResource))
                    SrvArraySlices.Add(new ShaderResourceView(Renderer.Device, texture) { Tag = this, DebugName = Name });
            }

            IsLoaded = true;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            foreach (var rtv in RtvArraySlices)
                rtv.Dispose();
            foreach (var srv in SrvArraySlices)
                srv.Dispose();
            foreach (var dsv in DsvArraySlices)
                dsv.Dispose();

            NativeTexture?.Dispose();

            RtvArraySlices.Clear();
            SrvArraySlices.Clear();
            DsvArraySlices.Clear();

            _disposed = true;
        }

        private void LoadBitmap(System.Drawing.Bitmap bitmap, BindFlags flags)
        {
            var texDesc = new Texture2DDescription
            {
                Width = bitmap.Width,
                Height = bitmap.Height,
                ArraySize = 1,
                BindFlags = flags,
                Usage = ResourceUsage.Immutable,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.R8G8B8A8_UNorm,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0)
            };

            var rect = DirectXUtil.LoadBitmapData(bitmap);

            NativeTexture = new Texture2D(Renderer.Device, texDesc, rect);
        }

        internal T QueryInterface<T>() where T : ComObject
        {
            return NativeTexture.QueryInterface<T>();
        }

        /// <inheritdoc />
        protected override void BeforeSerialization()
        {
            base.BeforeSerialization();
            _bytes = GetBytesPBGRA();
        }

        /// <inheritdoc />
        protected override void AfterDeserialization()
        {
            // TODO: Load PBGRA bytes
            Load(new MemoryStream(_bytes));
        }

        /// <summary>
        /// Copies the texture to a byte array in PBGRA format.
        /// </summary>
        /// <returns>The byte array.</returns>
        public byte[] GetBytesPBGRA()
        {
            var context = Renderer.Context;

            lock (Renderer.Locker)
            {
                Texture2D stagingTex;

                var width = NativeTexture.Description.Width;
                var height = NativeTexture.Description.Height;

                if (!NativeTexture.Description.Usage.HasFlag(ResourceUsage.Staging))
                {
                    stagingTex = new Texture2D(Renderer.Device, new Texture2DDescription
                    {
                        Width = width,
                        Height = height,
                        MipLevels = 1,
                        ArraySize = 1,
                        Format = NativeTexture.Description.Format,
                        Usage = ResourceUsage.Staging,
                        SampleDescription = new SampleDescription(1, 0),
                        BindFlags = BindFlags.None,
                        CpuAccessFlags = CpuAccessFlags.Read,
                        OptionFlags = ResourceOptionFlags.None
                    });
                    context.CopyResource(NativeTexture, stagingTex);
                }
                else
                {
                    stagingTex = NativeTexture;
                }

                DataStream dataStream;
                var dataBox = context.MapSubresource(
                    stagingTex,
                    0,
                    0,
                    MapMode.Read,
                    SharpDX.Direct3D11.MapFlags.None,
                    out dataStream);

                var dataRectangle = new DataRectangle
                {
                    DataPointer = dataStream.DataPointer,
                    Pitch = dataBox.RowPitch
                };

                var bitmap = new Bitmap(
                    Renderer.ImagingFactory,
                    width,
                    height,
                    WICTranslate.GUIDFromFormat(NativeTexture.Description.Format),
                    dataRectangle);

                var bytes = new byte[height * (width * 4)];

                using (var converter = new FormatConverter(Renderer.ImagingFactory))
                {
                    converter.Initialize(bitmap, PixelFormat.Format32bppPBGRA);
                    converter.CopyPixels(bytes, width * 4);
                }

                return bytes;
            }
        }

        internal Bitmap1 GetD2DBitmap()
        {
            var surface = QueryInterface<Surface1>();

            return new Bitmap1(Renderer.Context2D, surface,
                new BitmapProperties1(
                    new SharpDX.Direct2D1.PixelFormat(surface.Description.Format,
                        SharpDX.Direct2D1.AlphaMode.Premultiplied),
                    96,
                    96, BitmapOptions.Target));
        }

        /// <summary>
        /// Resizes an existing texture.
        /// </summary>
        public void Resize(int x, int y)
        {
            var desc = NativeTexture.Description;
            desc.Width = x;
            desc.Height = y;
            NativeTexture = new Texture2D(Renderer.Device, desc);
        }
    }
}