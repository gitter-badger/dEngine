// GfxShader.cs - dEngine
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using dEngine.Graphics.States;
using dEngine.Utility.Extensions;
using JetBrains.Annotations;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

#pragma warning disable 1591
#pragma warning disable 649

namespace dEngine.Graphics
{
    [XmlRoot(ElementName = "Shader")]
    public class GfxShader : IDisposable, IDataType
    {
        private Dictionary<string, Pass> _passDictionary;

        internal GfxShader()
        {
        }

        internal GfxShader(string name, List<Pass> passes, string source)
        {
            Passes = new List<Pass>(passes);
            Source = source;
            UpdatePassDictionary();
        }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlArray, XmlArrayItem(typeof(Pass))]
        public List<Pass> Passes { get; set; }

        [XmlIgnore]
        public string Source { get; set; }

        public void UpdatePassDictionary()
        {
            _passDictionary = Passes.ToDictionary(pass => pass.Name, pass => pass);
        }

        public void Load(BinaryReader reader)
        {
            _passDictionary = new Dictionary<string, Pass>();
            Name = reader.ReadString();
            var passCount = reader.ReadInt32();
            for (int i = 0; i < passCount; i++)
            {
                var pass = new Pass();
                pass.Load(reader);
                Passes[i] = pass;
                _passDictionary[pass.Name] = pass;
            }
            Source = reader.ReadString();
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Name);
            var passCount = Passes.Count;
            writer.Write(passCount);
            for (int i = 0; i < passCount; i++)
            {
                Passes[i].Save(writer);
            }
            writer.Write(Source);
        }

        public void Dispose()
        {
            foreach (var pass in _passDictionary.Values)
            {
                pass.Dispose();
            }

            _passDictionary.Clear();
        }

        public override string ToString()
        {
            return $"{Name} (Shader)";
        }

        public Pass GetPass(string passName = "Main")
        {
            return _passDictionary[passName];
        }

        [Serializable]
        public class Define
        {
            [XmlText]
            public string Text { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        [Serializable]
        public class Pass : IDataType, IEquatable<Pass>, IDisposable
        {
            public Pass()
            {
                _vertexBytecode = new byte[0];
                _pixelBytecode = new byte[0];
                _geometryBytecode = new byte[0];
                _computeBytecode = new byte[0];
                Blend = new Blend();
                Defines = new List<Define>();
            }

            [XmlIgnore]
            public bool IsCompiled { get; private set; }

            [XmlAttribute]
            public string Name { get; set; }

            [XmlElement]
            public Blend Blend { get; set; }

            [XmlElement]
            public CullMode CullMode { get; set; } = CullMode.Back;

            [XmlElement]
            public FillMode FillMode { get; set; } = FillMode.Solid;

            [XmlElement]
            public bool ZWrite { get; set; } = true;

            [XmlAttribute]
            public string VertexShader { get; set; }

            [XmlAttribute]
            public string PixelShader { get; set; }

            [XmlAttribute]
            public string GeometryShader { get; set; }

            [XmlAttribute]
            public string ComputeShader { get; set; }

            [XmlIgnore]
            public SortedDictionary<string, object> Parameters { get; } = new SortedDictionary<string, object>();

            [XmlArray, XmlArrayItem(typeof(Define))]
            public List<Define> Defines { get; set; }
            /*
             * lend { get; set; }
                public CullMode CullMode { get; set; }
                public FillMode FillMode { get; set; }
                public bool ZWrite { get; set; } = true;
             */

            private void SaveBytecode(BinaryWriter writer, ref byte[] bytecode)
            {
                writer.Write(bytecode.Length);
            }

            private void LoadBytecode(BinaryReader reader, out byte[] bytecode)
            {
                var byteCount = reader.ReadInt32();
                bytecode = reader.ReadBytes(byteCount);
            }

            public void Load(BinaryReader reader)
            {
                Name = reader.ReadString();
                var blend = new Blend();
                blend.Load(reader);
                Blend = blend;
                CullMode = (CullMode)reader.ReadByte();
                FillMode = (FillMode)reader.ReadByte();
                ZWrite = reader.ReadBoolean();

                LoadBytecode(reader, out _vertexBytecode);
                LoadBytecode(reader, out _pixelBytecode);
                LoadBytecode(reader, out _geometryBytecode);
                LoadBytecode(reader, out _computeBytecode);
            }

            public void Save(BinaryWriter writer)
            {
                writer.Write(Name);
                Blend.Save(writer);
                writer.Write((byte)CullMode);
                writer.Write((byte)FillMode);
                writer.Write(ZWrite);

                SaveBytecode(writer, ref _vertexBytecode);
                SaveBytecode(writer, ref _pixelBytecode);
                SaveBytecode(writer, ref _geometryBytecode);
                SaveBytecode(writer, ref _computeBytecode);
            }

            public void Dispose()
            {
                _vertexShader?.Dispose();
                _pixelShader?.Dispose();
                _geometryShader?.Dispose();
                _computeShader?.Dispose();
            }

            public bool Equals(Pass other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(Name, other.Name);
            }

            public override string ToString()
            {
                return $"{Name} (Pass)";
            }
            
            private byte[] CompileShaderDef(string definition, string source, string fileName = null)
            {
#if DEBUG
                const ShaderFlags shaderFlags = ShaderFlags.Debug;
#else
                const ShaderFlags shaderFlags = ShaderFlags.OptimizationLevel3;
#endif

                var macroArray = Defines.Select(x => new ShaderMacro(x.Text, 1)).ToArray();

                using (var includer = new Includer())
                {
                    var split = definition.Split(' ');
                    var result = ShaderBytecode.Compile(source, split[0], split[1], shaderFlags, EffectFlags.None,
                        macroArray, includer, fileName);
                    /*
                    foreach (var msg in result.Message.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        Renderer.Logger.Info(msg);
                        Debug.WriteLine(msg);
                    }
                    */

                    if (result.Bytecode == null)
                        throw new Exception($"Could not compile shader \"{Name}\". \n\n{result.Message}");

                    return result.Bytecode;
                }
            }

            /// <summary>
            /// Loads bytecode into a shader object.
            /// </summary>
            public void Load()
            {
                if (_vertexBytecode.Length > 0)
                {
                    _vertexShader?.Dispose();
                    _vertexShader = new VertexShader(Renderer.Device, _vertexBytecode);
                }
                if (_pixelBytecode.Length > 0)
                {
                    _pixelShader?.Dispose();
                    _pixelShader = new PixelShader(Renderer.Device, _pixelBytecode);
                }
                if (_geometryBytecode.Length > 0)
                {
                    _geometryShader?.Dispose();
                    _geometryShader = new GeometryShader(Renderer.Device, _geometryBytecode);
                }
                if (_computeBytecode.Length > 0)
                {
                    _computeShader?.Dispose();
                    _computeShader = new ComputeShader(Renderer.Device, _computeBytecode);
                }

                // bytecode no longer needed, so unload it
                _vertexBytecode = new byte[0];
                _pixelBytecode = new byte[0];
                _geometryBytecode = new byte[0];
                _computeBytecode = new byte[0];
            }

            /// <summary>
            /// Compiles the shader source into bytecode.
            /// </summary>
            public void Compile(string source, string fileName = null)
            {
                if (!string.IsNullOrEmpty(VertexShader))
                {
                    _vertexBytecode = CompileShaderDef(VertexShader, source, fileName);
                    _vertexShader = new VertexShader(Renderer.Device, _vertexBytecode);
                    var elements = CreateShaderInputLayout(_vertexBytecode);
                    _inputLayout = new InputLayout(Renderer.Device, _vertexBytecode, elements);
                }

                if (!string.IsNullOrEmpty(PixelShader))
                {
                    _pixelBytecode = CompileShaderDef(PixelShader, source, fileName);
                    _pixelShader = new PixelShader(Renderer.Device, _pixelBytecode);
                }

                if (!string.IsNullOrEmpty(GeometryShader))
                {
                    _geometryBytecode = CompileShaderDef(GeometryShader, source, fileName);
                    _geometryShader = new GeometryShader(Renderer.Device, _geometryBytecode);
                }

                if (!string.IsNullOrEmpty(ComputeShader))
                {
                    _computeBytecode = CompileShaderDef(ComputeShader, source, fileName);
                    _computeShader = new ComputeShader(Renderer.Device, _computeBytecode);
                }

                _rasterizerState?.Dispose();
                _rasterizerState = new RasterizerState(Renderer.Device, new RasterizerStateDescription
                {
                    CullMode = CullMode,
                    FillMode = FillMode,
                    SlopeScaledDepthBias = 0,
                    DepthBias = 0,
                    DepthBiasClamp = 0,
                    IsDepthClipEnabled = true
                });
            }

            private static InputElement[] CreateShaderInputLayout(byte[] bytecode)
            {
                using (var reflection = new ShaderReflection(bytecode))
                {
                    var shaderDesc = reflection.Description;

                    var elements = new InputElement[shaderDesc.InputParameters];

                    for (var i = 0; i < shaderDesc.InputParameters; i++)
                    {
                        var paramDesc = reflection.GetInputParameterDescription(i);
                        var usageMask = (int)paramDesc.UsageMask & 255;

                        var format = Format.Unknown;

                        if (usageMask == 1)
                        {
                            if (paramDesc.ComponentType == RegisterComponentType.UInt32) format = Format.R32_UInt;
                            else if (paramDesc.ComponentType == RegisterComponentType.SInt32) format = Format.R32_SInt;
                            else if (paramDesc.ComponentType == RegisterComponentType.Float32) format = Format.R32_Float;
                        }
                        else if (usageMask <= 3)
                        {
                            if (paramDesc.ComponentType == RegisterComponentType.UInt32) format = Format.R32G32_UInt;
                            else if (paramDesc.ComponentType == RegisterComponentType.SInt32)
                                format = Format.R32G32_SInt;
                            else if (paramDesc.ComponentType == RegisterComponentType.Float32)
                                format = Format.R32G32_Float;
                        }
                        else if (usageMask <= 7)
                        {
                            if (paramDesc.ComponentType == RegisterComponentType.UInt32) format = Format.R32G32B32_UInt;
                            else if (paramDesc.ComponentType == RegisterComponentType.SInt32)
                                format = Format.R32G32B32_SInt;
                            else if (paramDesc.ComponentType == RegisterComponentType.Float32)
                                format = Format.R32G32B32_Float;
                        }
                        else if (usageMask <= 15)
                        {
                            if (paramDesc.ComponentType == RegisterComponentType.UInt32)
                                format = Format.R32G32B32A32_UInt;
                            else if (paramDesc.ComponentType == RegisterComponentType.SInt32)
                                format = Format.R32G32B32A32_SInt;
                            else if (paramDesc.ComponentType == RegisterComponentType.Float32)
                                format = Format.R32G32B32A32_Float;
                        }
                        else
                        {
                            throw new Exception("Invalid usage mask");
                        }

                        var classification = InputClassification.PerVertexData;

                        if (paramDesc.SemanticName.Substring(0, 2) == "I_")
                            classification = InputClassification.PerInstanceData;

                        var slot = classification == InputClassification.PerVertexData ? 0 : 1;

                        var element = new InputElement(paramDesc.SemanticName, paramDesc.SemanticIndex, format,
                            InputElement.AppendAligned, slot, classification, slot);
                        elements[i] = element;
                    }

                    return elements;
                }
            }

            public void Use(ref DeviceContext context)
            {
                if (Renderer.CurrentPass == this)
                    return;

                context.Rasterizer.State = _rasterizerState;
                context.InputAssembler.InputLayout = _inputLayout;

                if (_vertexShader != null)
                    context.VertexShader.Set(_vertexShader);
                if (_pixelShader != null)
                    context.PixelShader.Set(_pixelShader);
                if (_geometryBytecode != null)
                    context.GeometryShader.Set(_geometryShader);
                if (_computeBytecode != null)
                    context.ComputeShader.Set(_computeShader);

                context.PixelShader.SetSamplers(0, SamplerStates.States);

                Renderer.CurrentPass = this;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Pass)obj);
            }

            public override int GetHashCode()
            {
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                return Name?.GetHashCode() ?? 0;
            }

            #region

            private byte[] _vertexBytecode;
            private byte[] _pixelBytecode;
            private byte[] _computeBytecode;
            private byte[] _geometryBytecode;

            private VertexShader _vertexShader;
            private PixelShader _pixelShader;
            private ComputeShader _computeShader;
            private GeometryShader _geometryShader;
            private InputLayout _inputLayout;
            private RasterizerState _rasterizerState;

            #endregion
        }

        public class Blend : IDataType
        {
            public BlendOperation AlphaBlendOperation;
            public BlendOperation BlendOperation;
            public ColorWriteMaskFlags ColourWriteMask;
            public BlendOption DestinationAlphaBlend;
            public BlendOption DestinationBlend;
            public BlendOption SourceAlphaBlend;
            public BlendOption SourceBlend;

            public void Load(BinaryReader reader)
            {
                AlphaBlendOperation = (BlendOperation)reader.ReadByte();
                BlendOperation = (BlendOperation)reader.ReadByte();
                ColourWriteMask = (ColorWriteMaskFlags)reader.ReadByte();
                DestinationAlphaBlend = (BlendOption)reader.ReadByte();
                DestinationBlend = (BlendOption)reader.ReadByte();
                SourceAlphaBlend = (BlendOption)reader.ReadByte();
                SourceBlend = (BlendOption)reader.ReadByte();
            }

            public void Save(BinaryWriter writer)
            {
                writer.Write((byte)AlphaBlendOperation);
                writer.Write((byte)BlendOperation);
                writer.Write((byte)ColourWriteMask);
                writer.Write((byte)DestinationAlphaBlend);
                writer.Write((byte)DestinationBlend);
                writer.Write((byte)SourceAlphaBlend);
                writer.Write((byte)SourceBlend);
            }
        }

        internal class Includer : Include
        {
            private Stream _includeStream;
            public IDisposable Shadow { get; set; }

            public Stream Open(IncludeType type, string fileName, Stream parentStream)
            {
                var test = Assembly.GetExecutingAssembly().GetManifestResourceNames();

                var content =
                    Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream($"dEngine.Graphics.Shaders.{fileName}.shader")
                        .ReadString();
                content = Regex.Replace(content, @"[^\u0000-\u007F]", string.Empty);
                _includeStream = new MemoryStream(Encoding.ASCII.GetBytes(content));
                return _includeStream;
            }

            public void Close(Stream stream = null)
            {
                Dispose();
            }

            public void Dispose()
            {
                _includeStream?.Dispose();
            }
        }
    }
}