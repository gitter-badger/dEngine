// Shaders.cs - dEngine
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using dEngine.Settings.Global;
using dEngine.Utility.Native;
using LZ4;

namespace dEngine.Graphics
{
    internal static class Shaders
    {
        private static ConcurrentDictionary<string, GfxShader> _shaders;
        
        private static readonly char[] _cacheMagic = "SHADERCACHE".ToCharArray();

        internal static void Init()
        {
            _shaders = new ConcurrentDictionary<string, GfxShader>();

            var shaderCache = RenderSettings.ShaderCache;

            if (string.IsNullOrEmpty(shaderCache) || !File.Exists(shaderCache))
            {
                CompileShaders();

                shaderCache = Path.Combine(Engine.TempPath, "Shaders.cache");

                CacheShaders(shaderCache);
            }
            else
            {
                LoadShaders();
            }
        }

        private static void CompileShaders()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var serializer = new XmlSerializer(typeof(GfxShader));

            foreach (var manifest in assembly.GetManifestResourceNames())
            {
                if (manifest.EndsWith(".shader"))
                {
                    var stream = assembly.GetManifestResourceStream(manifest);

                    using (var reader = new StreamReader(stream))
                    {
                        string xml = reader.ReadLine();
                        string source = "\n";

                        if (!xml.StartsWith("<Shader"))
                            continue;

                        string line;
                        do
                        {
                            line = reader.ReadLine();
                            xml += line;
                            xml += '\n';
                            source += "\n";
                        } while (line?.EndsWith("Shader>") == false);

                        source += reader.ReadToEnd();

                        var shader = (GfxShader)serializer.Deserialize(new StringReader(xml));
                        shader.UpdatePassDictionary();
                        shader.Source = source;
                        shader.Passes.ForEach(p => p.Compile(source, manifest));
                        _shaders[shader.Name] = shader;
                    }
                }
            }
        }

        private static void CacheShaders(string shaderCache)
        {
            try
            {
                using (var stream = File.Create(shaderCache))
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(_cacheMagic);

                    foreach (var shader in _shaders.Values)
                    {
                        var shaderStream = new MemoryStream();
                        shader.Save(new BinaryWriter(shaderStream, Encoding.Default, true));
                        var inputBuffer = shaderStream.GetBuffer();
                        var outputBuffer = LZ4Codec.Encode(inputBuffer, 0, (int)shaderStream.Length);

                        writer.Write(outputBuffer.Length);
                        writer.Write((int)shaderStream.Length);
                        writer.Write(outputBuffer);
                    }
                }
            }
            catch (Exception e)
            {
                shaderCache = string.Empty;
                Engine.Logger.Error(e, "Could not create shader cache.");
            }

            RenderSettings.ShaderCache = shaderCache;
        }

        private static void LoadShaders()
        {
            using (var cache = File.OpenRead(RenderSettings.ShaderCache))
            using (var reader = new BinaryReader(cache))
            {
                var magic = reader.ReadChars(_cacheMagic.Length);

                if (VisualC.CompareMemory(magic, _cacheMagic, magic.Length) != 0)
                    throw new InvalidDataException("The shader cache did not match the magic bytes.");

                do
                {
                    var compressedLength = reader.ReadInt32();
                    var decompressedLength = reader.ReadInt32();
                    var inputBuffer = reader.ReadBytes(compressedLength);
                    var outputBuffer = LZ4Codec.Decode(inputBuffer, 0, compressedLength, decompressedLength);

                    using (var shaderStream = new MemoryStream(outputBuffer))
                    using (var shaderReader = new BinaryReader(shaderStream))
                    {
                        var name = shaderReader.ReadString();
                        var passCount = shaderReader.ReadInt32();
                        var passes = new List<GfxShader.Pass>(passCount);
                        for (int i = 0; i < passCount; i++)
                        {
                            var pass = new GfxShader.Pass();
                            pass.Load(shaderReader);
                            passes[i] = pass;
                        }
                        var source = shaderReader.ReadString();
                        var shader = new GfxShader(name, passes, source);
                        shader.Passes.ForEach(p => p.Load());
                        _shaders[name] = shader;
                    }
                } while (cache.Position < cache.Length);
            }
        }

        internal static GfxShader Get(string name)
        {
            GfxShader shader;
            _shaders.TryGetValue(name, out shader);
            return shader;
        }

        public static void DisposeAll()
        {
            foreach (var shader in _shaders.Values)
            {
                shader.Dispose();
            }
            _shaders.Clear();

        }
    }
}