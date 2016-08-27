// Inst.cs - dEngine
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
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using dEngine.Data;
using dEngine.Instances;
using dEngine.Instances.Attributes;
using dEngine.Instances.Materials;
using dEngine.Services;
using dEngine.Utility;
using dEngine.Utility.Extensions;
using dEngine.Utility.Native;
using Dynamitey;
using LZ4;
using Microsoft.Scripting.Utils;

#pragma warning disable 1591

// ReSharper disable BuiltInTypeReferenceStyle

namespace dEngine.Serializer.V1
{
    /*
172
     */
    /// <summary>
    /// Inst is a custom serialization format for <see cref="Instance" />s.
    /// </summary>
    public static class Inst
    {
        /// <summary>
        /// The version string.
        /// </summary>
        public const string Version = "v1";

        private static readonly char[] _file = "INSTBIN".ToCharArray();
        private static readonly char[] _end = "END".ToCharArray();

        private static Dictionary<int, CachedType> Types;

        internal static Dictionary<string, CachedType> TypeDictionary;

        internal static int HighestTypeId;

        public enum DataType : byte
        {
            Unknown,
            String,
            Boolean,
            Int16,
            Int32,
            Int64,
            Single,
            Double,
            UserData,
            Content,
            Enum,
            Referent,
            BinaryData,
            InstanceId,
            FontFamily,
            Invalid = 255,
        }

        private static void PrintAvailableIds(int upTo)
        {
            string str = "";
            for (int i = 0; i < upTo; i++)
            {
                if (Types.Values.All(t => t.TypeId != i))
                    str += i.ToString() + "\n";
            }
            Debug.WriteLine(str);
        }

        static Inst()
        {
            Init();
        }

        internal static void Init()
        {
            Types = new Dictionary<int, CachedType>();
            TypeDictionary = new Dictionary<string, CachedType>();

            var typesMissingIDs = new List<Type>();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!typeof(Instance).IsAssignableFrom(type))
                    continue;

                var typeId = type.GetCustomAttribute<TypeIdAttribute>()?.Id;
                if (typeId.HasValue)
                {
#if DEBUG
                    CachedType existingType;
                    if (Types.TryGetValue(typeId.Value, out existingType))
                        throw new InvalidOperationException(
                            $"Two types share the same ID. (trying to add {type} as {typeId.Value}, but was already added as {existingType.Name})");
#endif
                    CacheType(type, typeId.Value);
                    HighestTypeId = Math.Max(typeId.Value, HighestTypeId);
                }
                else if (type.IsPublic)
                {
                    typesMissingIDs.Add(type);
                }
            }

            if (typesMissingIDs.Count > 0)
            {
                var str = "";
                foreach (var type in typesMissingIDs)
                    str += $"{type} could be {++HighestTypeId}\n";
                var msg = $"There are public Instance types that are missing type IDs: {str}";
                throw new Exception(msg);
            }
        }

        /// <summary>
        /// Serializes an <see cref="Instance" />.
        /// </summary>
        /// <param name="instance">The instance to serialize.</param>
        /// <param name="output">The output stream.</param>
        /// <param name="clone">Determines if the object is being cloned.</param>
        /// <param name="includeWorkspaceInGame">
        /// Determines whether or not <see cref="Workspace" /> should be filtered out when
        /// serializing the <see cref="DataModel" />.
        /// </param>
        public static void Serialize(Instance instance, Stream output, bool clone = false,
            bool includeWorkspaceInGame = true)
        {
            using (var writer = new BinaryWriter(output, Encoding.UTF8, true))
            {
                writer.Write(_file);
                writer.Write(Version);

                var context = new Context
                {
                    IsClone = clone,
                    IncludeWorkspace = includeWorkspaceInGame,
                    Root = instance
                };
                context.Traverse(instance);
                writer.Write(context.TotalTypes);
                writer.Write(context.TotalObjects);
                writer.Write(new byte[8], 0, 8);

                foreach (var kvp in context.TypeRecords)
                {
                    context.WriteFileRecord(kvp.Value, writer);
                }

                // Write Properties
                foreach (var record in context.TypeRecords)
                {
                    var propertyRecords = record.Value.Properties.Values;
                    writer.Write(propertyRecords.Count);
                    foreach (var prop in propertyRecords)
                    {
                        foreach (var referent in record.Value.Referents.Values)
                        {
                            var obj = referent.Instance;
                            var data = prop.Property.Get.FastDynamicInvoke(obj);
                            prop.Data.Add(data);
                        }

                        context.WriteFileRecord(prop, writer);
                    }
                }

                writer.Write(_end);

                foreach (var type in context.TypeRecords)
                    foreach (var obj in type.Value.Objects)
                        obj.Value.Instance.AfterSerialization(context);
            }
        }



        /// <summary>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="existing"></param>
        /// <param name="skipMagic"></param>
        /// <returns></returns>
        public static Instance Deserialize(Stream input, Instance existing = null, bool skipMagic = false)
        {
            using (var reader = new BinaryReader(input))
            {
                #region Header

                if (!skipMagic)
                {
                    var magic = reader.ReadChars(7);
                    if (VisualC.CompareMemory(magic, _file, 4) != 0)
                        throw new InvalidDataException("The file does not have a valid header.");
                }

                var version = reader.ReadString();
                if (version != Version)
                    throw new InvalidDataException($"The serializer ({Version}) does not support version \"{version}\"");

                var totalTypes = reader.ReadInt32();
                var totalObjects = reader.ReadInt32();
                reader.ReadBytes(8); // read reserved 8 bytes

                var context = new Context
                {
                    TotalTypes = totalTypes,
                    TotalObjects = totalObjects
                };

                #endregion

                #region Types

                var typeHeaders = new TypeRecord[totalTypes];

                for (int i = 0; i < totalTypes; i++)
                {
                    var typeRecord = new TypeRecord(context);
                    if (!ReadFileRecord(typeRecord, reader))
                        throw new InvalidDataException("Type headers misaligned");
                    typeHeaders[i] = typeRecord;
                }

                #endregion

                #region Referents & Object Creation

                var objects = new Instance[totalObjects];
                int objIndex = 0;
                for (int i = 0; i < totalTypes; i++)
                {
                    var typeRecord = typeHeaders[i];

                    foreach (var referent in typeRecord.Referents.Values)
                    {
                        var instance = (Instance)typeRecord.Type.GetInstance();
                        instance.BeforeDeserialization();
                        objects[objIndex++] = instance;
                        referent.Instance = instance;
                        if (objIndex == 0)
                            context.Root = instance;
                    }
                }

                #endregion

                #region Reading Properties

                for (int i = 0; i < totalTypes; i++)
                {
                    var typeRecord = typeHeaders[i];
                    var propCount = reader.ReadInt32();
                    for (int j = 0; j < propCount; j++)
                    {
                        var propRecord = new PropertyRecord(context, typeRecord);
                        if (!ReadFileRecord(propRecord, reader))
                            break;
                        typeRecord.Properties.Add(propRecord.EncodeTag, propRecord);
                    }
                }

                #endregion

                #region Setting Properties

                for (int pass = 0; pass < 2; pass++)
                {
                    for (int i = 0; i < totalTypes; i++)
                    {
                        var typeRecord = typeHeaders[i];

                        int j = 0;
                        foreach (var referent in typeRecord.Referents.Values)
                        {
                            var instance = referent.Instance;

                            foreach (var prop in typeRecord.Properties.Values)
                            {
                                if (prop.Name == "Parent")
                                {
                                    if (pass == 1)
                                    {
                                        continue;
                                    }
                                }
                                else if (pass == 0)
                                    continue;

                                PropertyRecord record;
                                if (typeRecord.Properties.TryGetValue(prop.EncodeTag, out record))
                                {
                                    var data = record.Data[j];
                                    var refr = data as Referent;
                                    if (refr != null)
                                        data = refr.Instance;

                                    if (prop.DataType == DataType.Content || (record.Property.PropertyType == typeof(FontFamily) && data is string)) // coerce string into content
                                    {
                                        data = Dynamic.CoerceConvert(data, record.Property.PropertyType);
                                    }

                                    //prop.Property.Set(instance, data);
                                    prop.Property.Set.FastDynamicInvoke(instance, data);
                                }

                            }
                            j++;
                        }
                    }
                }

                #endregion

                var end = reader.ReadChars(3);

                if (VisualC.CompareMemory(end, _end, 3) != 0)
                    throw new InvalidDataException("End bytes did not match.");

                for (var i = 0; i < objects.Length; i++)
                    objects[i].AfterDeserialization(context);

                return objects[0];
            }
        }

        private static bool ReadFileRecord(IFileRecord record, BinaryReader reader)
        {
            var chars = reader.ReadChars(record.Magic.Length);
            if (VisualC.CompareMemory(chars, record.Magic, record.Magic.Length) != 0)
            {
                reader.BaseStream.Position -= record.Magic.Length;
                return false;
            }

            var compressedLength = reader.ReadInt32();
            var decompressedLength = reader.ReadInt32();

            reader.BaseStream.Position += 4;

            var inputBuffer = new byte[compressedLength];
            reader.Read(inputBuffer, 0, compressedLength);

            var outputBuffer = new byte[decompressedLength];

            LZ4Codec.Decode(inputBuffer, 0, inputBuffer.Length, outputBuffer, 0, decompressedLength);

            var stream = new MemoryStream(outputBuffer);
            var recordReader = new BinaryReader(stream);
            record.Read(recordReader);

            return true;
        }

        public static Instance Clone(Instance instance)
        {
            using (var stream = new MemoryStream())
            {
                Serialize(instance, stream, clone: true);
                stream.Position = 0L;
                return Deserialize(stream, null);
            }
        }

        /// <summary>
        /// Returns the content type of the given stream.
        /// </summary>
        public static ContentType? PeekContent(Stream stream)
        {
            ContentType? type;
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                var magic = reader.ReadBytes(4);
                if (VisualC.CompareMemory(magic, AssetBase.Magic, 4) != 0)
                    type = null;
                else
                    type = (ContentType)reader.ReadByte();
            }
            stream.Position = 0;
            return type;
        }

        /// <summary>
        /// Checks if the given stream is valid instance binary.
        /// </summary>
        /// <param name="stream">The stream to check.</param>
        /// <param name="seekAfterwards">Determines if the stream's position is set back to zero.</param>
        public static bool CheckHeader(Stream stream, bool seekAfterwards = false)
        {
            var magic = new byte[7];
            stream.Read(magic, 0, magic.Length);

            var valid = VisualC.CompareMemory(magic, _file, magic.Length) == 0;

            if (seekAfterwards)
                stream.Position = 0L;

            return valid;
        }

        internal interface IFileRecord
        {
            char[] Magic { get; }
            void Read(BinaryReader reader);
            MemoryStream Write(Context context);
        }


        internal class ReferentRecord : IFileRecord
        {
            public ReferentRecord(Context context)
            {
                Context = context;
            }

            public Context Context { get; set; }

            public char[] Magic { get; } = "REFR".ToCharArray();

            public void Read(BinaryReader reader)
            {
                foreach (var typeRecord in Context.TypeRecords.Values)
                {
                    for (int i = 0; i < typeRecord.ObjectCount; i++)
                    {
                        var id = reader.ReadInt32();
                        var referent = new Referent(id);
                        Context.GlobalReferents[id] = typeRecord.Referents[id] = referent;
                    }
                }
            }

            public MemoryStream Write(Context context)
            {
                var stream = new MemoryStream();
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    foreach (var typeRecord in context.TypeRecords.Values)
                    {
                        foreach (var record in typeRecord.Referents)
                        {
                            writer.Write(record.Key);
                        }
                    }
                }
                return stream;
            }
        }

        internal static int EncodePropertyTag(short typeId, short propId)
        {
            return (propId << 16) | (typeId & 0xFFFF);
        }

        internal static void DecodePropertyTag(int encoded, out short typeId, out short propId)
        {
            typeId = (short)(encoded & 0xFFFF);
            propId = (short)((encoded >> 16) & 0xFFFF);
        }

        internal class PropertyRecord : IFileRecord
        {
            private static readonly char[] _nullChars = { 'N', 'U', 'L' };

            public PropertyRecord(Context context, TypeRecord typeRecord)
            {
                Data = new List<object>();
                TypeRecord = typeRecord;
                Context = context;
            }

            public override string ToString()
            {
                return Name;
            }

            public PropertyRecord(Context context, TypeRecord typeRecord, CachedProperty prop)
                : this(context, typeRecord)
            {
                DeclaringTypeId = prop.DeclaringTypeId;
                PropertyTag = prop.Tag;
                EncodeTag = EncodePropertyTag(DeclaringTypeId, PropertyTag);
                DataType = prop.DataType;
                PropertyType = prop.PropertyType;
                Property = prop;
                Name = prop.Name;
            }


            public string Name { get; set; }
            public short DeclaringTypeId { get; set; }
            public short PropertyTag { get; set; }
            public DataType DataType { get; set; }
            public Type PropertyType { get; set; }
            public List<object> Data { get; }
            public TypeRecord TypeRecord { get; set; }
            public Context Context { get; set; }
            public char[] Magic { get; } = "PROP".ToCharArray();
            public int EncodeTag { get; set; }
            public CachedProperty Property { get; set; }


            public MemoryStream Write(Context context)
            {
                var recordStream = new MemoryStream();
                using (var recordWriter = new BinaryWriter(recordStream, Encoding.UTF8, true))
                {
                    recordWriter.Write(EncodeTag);
                    recordWriter.Write((byte)DataType);

                    foreach (dynamic data in Data)
                    {
                        switch (DataType)
                        {
                            case DataType.String:
                                recordWriter.Write((string)data ?? string.Empty);
                                break;
                            case DataType.Content:
                                recordWriter.Write(data?.ContentId ?? string.Empty);
                                break;
                            case DataType.InstanceId:
                                var guid = (InstanceId)data;
                                guid.Save(recordWriter);
                                break;
                            case DataType.Boolean:
                                recordWriter.Write((bool)data);
                                break;
                            case DataType.Int32:
                                recordWriter.Write((int)data);
                                break;
                            case DataType.Single:
                                recordWriter.Write((float)data);
                                break;
                            case DataType.Double:
                                recordWriter.Write((double)data);
                                break;
                            case DataType.Enum:
                                recordWriter.Write((short)data);
                                break;
                            case DataType.FontFamily:
                                recordWriter.Write((string)data);
                                break;
                            case DataType.Referent:
                                var value = (Instance)data;
                                int id;
                                if (value == null)
                                    id = -1;
                                else
                                {
                                    Referent referent;
                                    var cachedType = TypeDictionary[value.ClassName];
                                    TypeRecord record;
                                    if (!context.TypeRecords.TryGetValue(cachedType, out record) || !record.Objects.TryGetValue(value, out referent))
                                        id = -1;
                                    else
                                        id = referent.ReferentId;
                                }
                                recordWriter.Write(id);
                                break;
                            case DataType.UserData:
                                var userData = ((IDataType)data);
                                if (userData != null)
                                {
                                    recordWriter.Write(GetIdFromDataType(PropertyType));
                                    userData.Save(recordWriter);
                                }
                                else
                                    recordWriter.Write(_nullChars);
                                break;
                            default:
                                throw new NotSupportedException($"Property type \"{PropertyType.Name}\" not supported.");
                        }
                    }
                }
                return recordStream;
            }

            public void Read(BinaryReader propReader)
            {
                var encode = propReader.ReadInt32();
                short declaringTypeId;
                short propertyTag;
                DecodePropertyTag(encode, out declaringTypeId, out propertyTag);
                EncodeTag = encode;
                DeclaringTypeId = declaringTypeId;
                PropertyTag = propertyTag;

                CachedProperty cached;
                if (TypeRecord.Type.TaggedProperties.TryGetValue(encode, out cached))
                {
                    Name = cached.Name;
                    Property = cached;
                    PropertyType = cached.PropertyType;
                }

                DataType = (DataType)propReader.ReadByte();

                while (propReader.BaseStream.Position < propReader.BaseStream.Length)
                {
                    switch (DataType)
                    {
                        case DataType.String:
                            Data.Add(propReader.ReadString());
                            break;
                        case DataType.Content:
                            Data.Add(propReader.ReadString());
                            break;
                        case DataType.Boolean:
                            Data.Add(propReader.ReadBoolean());
                            break;
                        case DataType.Int16:
                            Data.Add(propReader.ReadInt16());
                            break;
                        case DataType.Int32:
                            Data.Add(propReader.ReadInt32());
                            break;
                        case DataType.Int64:
                            Data.Add(propReader.ReadInt64());
                            break;
                        case DataType.Single:
                            Data.Add(propReader.ReadSingle());
                            break;
                        case DataType.Double:
                            Data.Add(propReader.ReadDouble());
                            break;
                        case DataType.Enum:
                            Data.Add(propReader.ReadInt16());
                            break;
                        case DataType.FontFamily:
                            Data.Add(new FontFamily(propReader.ReadString()));
                            break;
                        case DataType.Referent:
                            var referent = propReader.ReadInt32();
                            Data.Add(referent == -1 ? Referent.Null : Context.GlobalReferents[referent]);
                            break;
                        case DataType.UserData:
                            if (VisualC.CompareMemory(propReader.ReadBytes(3), _nullChars, 3) == 0)
                            {
                                Data.Add(null);
                            }
                            else
                            {
                                propReader.BaseStream.Position -= 3; // go back to start
                                var id = propReader.ReadByte();
                                IDataType userData;

                                switch (id)
                                {
                                    case 0:
                                        userData = new Vector3();
                                        break;
                                    case 1:
                                        userData = new Vector2();
                                        break;
                                    case 2:
                                        userData = new Vector4();
                                        break;
                                    case 3:
                                        userData = new Colour();
                                        break;
                                    case 4:
                                        userData = new Axes();
                                        break;
                                    case 5:
                                        userData = new CFrame();
                                        break;
                                    case 6:
                                        userData = new UDim2();
                                        break;
                                    case 7:
                                        userData = new ColourSequence();
                                        break;
                                    case 8:
                                        userData = new NumberSequence();
                                        break;
                                    case 9:
                                        userData = new NumberRange();
                                        break;
                                    case 10:
                                        userData = new Faces();
                                        break;
                                    case 11:
                                        userData = new Matrix3();
                                        break;
                                    case 12:
                                        userData = new PhysicalProperties();
                                        break;
                                    case 13:
                                        userData = new Plane();
                                        break;
                                    case 15:
                                        userData = new Ray();
                                        break;
                                    case 16:
                                        userData = new Region3();
                                        break;
                                    case 17:
                                        userData = new Vector3int16();
                                        break;
                                    case 18:
                                        userData = new Region3int16();
                                        break;
                                    case 19:
                                        userData = new DateTime();
                                        break;
                                    case 20:
                                        userData = new TimeSpan();
                                        break;
                                    case 21:
                                        userData = new BinaryData();
                                        break;
                                    case 22:
                                        userData = new MaterialNodeCollection();
                                        break;
                                    case 23:
                                        userData = new InstanceId();
                                        break;
                                    case 24:
                                        userData = new FontFamily();
                                        break;
                                    default:
                                        throw new IndexOutOfRangeException($"No DataType with data ID \"{id}\" found.");
                                }

                                userData.Load(propReader);
                                Data.Add(userData);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            
            private byte GetIdFromDataType(Type type) // TODO: use attributes and build a dictionary
            {
                var name = type.Name;
                switch (name)
                {
                    case nameof(Vector3):
                        return 0;
                    case nameof(Vector2):
                        return 1;
                    case nameof(Vector4):
                        return 2;
                    case nameof(Colour):
                        return 3;
                    case nameof(Axes):
                        return 4;
                    case nameof(CFrame):
                        return 5;
                    case nameof(UDim2):
                        return 6;
                    case nameof(ColourSequence):
                        return 7;
                    case nameof(NumberSequence):
                        return 8;
                    case nameof(NumberRange):
                        return 9;
                    case nameof(Faces):
                        return 10;
                    case nameof(Matrix3):
                        return 11;
                    case nameof(PhysicalProperties):
                        return 12;
                    case nameof(Plane):
                        return 13;
                    case nameof(Ray):
                        return 15;
                    case nameof(Region3):
                        return 16;
                    case nameof(Vector3int16):
                        return 17;
                    case nameof(Region3int16):
                        return 18;
                    case nameof(DateTime):
                        return 19;
                    case nameof(TimeSpan):
                        return 20;
                    case nameof(BinaryData):
                        return 21;
                    case nameof(MaterialNodeCollection):
                        return 22;
                    case nameof(InstanceId):
                        return 23;
                    case nameof(FontFamily):
                        return 24;
                    default:
                        throw new IndexOutOfRangeException($"The DataType \"{name}\" does not have a data ID.");
                }
            }
        }

        internal class TypeRecord : IFileRecord
        {
            private short _typeId;

            public TypeRecord(Context context)
            {
                Context = context;
                Properties = new Dictionary<int, PropertyRecord>();
                Referents = new Dictionary<int, Referent>();
                Objects = new Dictionary<Instance, Referent>();
            }

            public TypeRecord(Context context, CachedType type) : this(context)
            {
                Type = type;
                _typeId = type.TypeId;

                foreach (var prop in type.Properties)
                {
                    if (prop.Tag == -1)
                        continue;
                    Properties.Add(EncodePropertyTag(prop.DeclaringTypeId, prop.Tag), new PropertyRecord(context, this, prop));
                }
            }

            public Context Context { get; }
            public Dictionary<int, PropertyRecord> Properties { get; }
            public CachedType Type { get; private set; }
            public int ObjectCount { get; internal set; }
            public char[] Magic { get; } = "TYPE".ToCharArray();
            public Dictionary<int, Referent> Referents { get; private set; }
            public Dictionary<Instance, Referent> Objects { get; private set; }

            public MemoryStream Write(Context context)
            {
                var recordStream = new MemoryStream();
                using (var recordWriter = new BinaryWriter(recordStream, Encoding.UTF8, true))
                {
                    recordWriter.Write(_typeId);
                    recordWriter.Write(false); // additional data flag
                    recordWriter.Write(ObjectCount);
                    foreach (var referent in Referents)
                    {
                        recordWriter.Write(referent.Key);
                    }
                }
                return recordStream;
            }

            public void Read(BinaryReader typeReader)
            {
                var typeId = typeReader.ReadInt16();
                var hasAdditionalData = typeReader.ReadBoolean();
                ObjectCount = typeReader.ReadInt32();

                Type = Types[typeId];
                _typeId = typeId;

                for (int i = 0; i < ObjectCount; i++)
                {
                    var id = typeReader.ReadInt32();
                    var referent = new Referent(id);
                    Referents[id] = referent;
                    Context.GlobalReferents[id] = referent;
                }

                if (hasAdditionalData)
                {
                    throw new NotImplementedException();
                }
            }

            public bool Equals(TypeRecord other)
            {
                return _typeId.Equals(other._typeId);
            }

            public override string ToString()
            {
                return $"{Type.Name} ({_typeId})";
            }
        }

        internal class CachedType
        {
            internal readonly short TypeId;

            public Func<object> GetInstance;

            public override string ToString()
            {
                return Type.Name;
            }

            public CachedType(Type type)
            {
                Type = type;
                TypeId = type.GetCustomAttribute<TypeIdAttribute>().Id;
                Properties = new List<CachedProperty>();
                TaggedProperties = new Dictionary<int, CachedProperty>();

                foreach (
                    var prop in
                        Enumerable.Reverse(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy |
                                                                                             BindingFlags.Instance | BindingFlags.Static)))
                {
                    var cachedProp = new CachedProperty(prop);
                    Properties.Add(cachedProp);
                    if (cachedProp.Tag != -1)
                    {
                        var encodeId = EncodePropertyTag(cachedProp.DeclaringTypeId, cachedProp.Tag);
                        TaggedProperties.Add(encodeId, cachedProp);
                    }
                }
                IsSingleton = typeof(ISingleton).IsAssignableFrom(type);

                if (IsSingleton)
                {
                    var getExisting = Type.GetCacheableMethod("GetExisting",
                        BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

                    if (getExisting == null)
                        throw new Exception("ISingleton is missing static GetExisting() method.");

                    GetInstance = getExisting
                        .Compile();
                }
                else if (!IsAbstract && IsPublic)
                {
                    var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                        null, new Type[0], null);
                    if (ctor == null)
                        return;
                    var lambda = Expression.Lambda<Func<object>>(Expression.New(ctor));
                    GetInstance = lambda.Compile();
                }
            }

            /// <summary>
            /// A list of cached properties which can be serialized.
            /// </summary>
            public Dictionary<int, CachedProperty> TaggedProperties { get; set; }
            /// <summary>
            /// A list of cached properties.
            /// </summary>
            public List<CachedProperty> Properties { get; }

            public Type Type { get; }

            public string Name => Type.Name;
            public bool IsAbstract => Type.IsAbstract;
            public bool IsPublic => Type.IsPublic;
            public bool IsEnum => Type.IsEnum;
            public bool IsSingleton { get; }

            public Attribute GetCustomAttribute<T>() where T : Attribute
            {
                return Type.GetCustomAttribute<T>();
            }
        }

        internal class Referent
        {
            public Referent(int referentId, Instance instance = null)
            {
                Instance = instance;
                ReferentId = referentId;
            }

            public Instance Instance { get; set; }
            public int ReferentId { get; }
            public static Referent Null { get; } = new Referent(-1);

            public override string ToString()
            {
                return $"{ReferentId}-{Instance}";
            }
        }

        public class CachedProperty
        {
            public readonly DataType DataType;
            public readonly Type DeclaringType;
            public readonly Func<object, object> Get;
            public readonly Type PropertyType;
            public readonly Action<object, object> Set;
            public readonly short Tag;

            public CachedProperty(PropertyInfo info)
            {
                Name = info.Name;
                var propType = info.PropertyType;
                PropertyType = propType;
                DeclaringType = info.DeclaringType;
                Tag = info.GetCustomAttribute<InstMemberAttribute>()?.Tag ?? -1;
                IsDeprecated = info.GetCustomAttribute<ObsoleteAttribute>() != null;
                EditorVisible = info.GetCustomAttribute<EditorVisibleAttribute>();
                IsPublic = info.IsPublic();
                IsStatic = info.IsStatic();
                IsSetterPublic = info.SetMethod?.IsPublic == true;
                Range = info.GetCustomAttribute<RangeAttribute>();
                DeclaringTypeId = info.DeclaringType.GetCustomAttribute<TypeIdAttribute>()?.Id ?? -1;
                Property = info;

                Get = info.GetValueGetter()?.Compile();
                Set = info.GetValueSetter()?.Compile();

                if (propType.IsEnum)
                    DataType = DataType.Enum;
                else if (propType == typeof(string))
                    DataType = DataType.String;
                else if (propType.IsGenericType)
                {
                    if (propType.GetGenericTypeDefinition() == typeof(Content<>))
                        DataType = DataType.Content;
                }
                else if (propType == typeof(bool))
                    DataType = DataType.Boolean;
                else if (propType == typeof(int))
                    DataType = DataType.Int32;
                else if (propType == typeof(float))
                    DataType = DataType.Single;
                else if (propType == typeof(double))
                    DataType = DataType.Double;
                else if (typeof(IDataType).IsAssignableFrom(propType))
                    DataType = DataType.UserData;
                else if (typeof(Instance).IsAssignableFrom(propType))
                    DataType = DataType.Referent;
                else
                    DataType = DataType.Invalid;
                //throw new InvalidOperationException($"No DataType specified for type \"{propType}\"");
            }

            public string Name { get; }
            public bool IsDeprecated { get; private set; }
            public EditorVisibleAttribute EditorVisible { get; private set; }
            public RangeAttribute Range { get; set; }
            public bool IsPublic { get; private set; }
            public bool IsStatic { get; private set; }
            public bool IsSetterPublic { get; private set; }
            public short DeclaringTypeId { get; set; }
            public PropertyInfo Property { get; }

            public override string ToString()
            {
                return Name;
            }

            public object FastGet(object o)
            {
                return Get.FastDynamicInvoke(o);
            }

            public void FastSet(object o, object val)
            {
                Set.FastDynamicInvoke(o, val);
            }
        }

        public class Context
        {
            internal readonly OrderedDictionary<CachedType, TypeRecord> TypeRecords;
            internal Dictionary<int, Referent> GlobalReferents;

            public Context()
            {
                TypeRecords = new OrderedDictionary<CachedType, TypeRecord>();
                GlobalReferents = new Dictionary<int, Referent>();
            }

            public Instance Root { get; internal set; }
            public int TotalTypes { get; internal set; }
            public int TotalObjects { get; internal set; }
            public bool IsClone { get; internal set; }
            public bool IncludeWorkspace { get; internal set; }

            public bool AddObject(Instance instance)
            {
                var typeRecord = TypeRecords[TypeDictionary[instance.ClassName]];
                if (typeRecord.Objects.ContainsKey(instance))
                    return false; //throw new InvalidOperationException();               
                var id = typeRecord.Context.TotalObjects;
                var referent = new Referent(id, instance);
                typeRecord.Referents[id] = referent;
                typeRecord.Objects[instance] = referent;
                typeRecord.ObjectCount++;
                typeRecord.Context.TotalObjects++;
                return true;
            }

            internal void WriteFileRecord(IFileRecord record, BinaryWriter writer)
            {
                writer.Write(record.Magic);

                var recordStream = record.Write(this);
                var inputBuffer = recordStream.ToArray();
                var outputBuffer = LZ4Codec.Encode(inputBuffer, 0, inputBuffer.Length);

                writer.Write(outputBuffer.Length);
                writer.Write(recordStream.Length);
                writer.Write(outputBuffer);
            }

            internal void Traverse(Instance instance)
            {
                if (!instance.Archivable || (instance is Workspace && !IncludeWorkspace))
                    return;

                TypeRecord record;
                var typeId = instance.GetType().GetCustomAttribute<TypeIdAttribute>()?.Id;
                if (!typeId.HasValue)
                    return;
                var type = Types[typeId.Value];
                if (!TypeRecords.TryGetValue(type, out record))
                {
                    record = new TypeRecord(this, type);
                    TypeRecords.Add(type, record);
                    TotalTypes++;
                }

                var context = record.Context;

                if (!context.AddObject(instance))
                    return;

                instance.BeforeSerialization(context);

                foreach (var prop in type.Properties)
                {
                    if (typeof(Instance).IsAssignableFrom(prop.PropertyType))
                    {
                        var inst = (Instance)prop.Get.FastDynamicInvoke(instance);
                        if (inst != null && inst.IsDescendantOf(Root))
                            Traverse(inst);
                    }
                }

                foreach (var child in instance.Children)
                {
                    Traverse(child);
                }
            }
        }

        internal static CachedType CacheType(Type type, int? typeId = null)
        {
            CachedType cached;
            if (!TypeDictionary.TryGetValue(type.Name, out cached))
            {
                cached = new CachedType(type);
                if (typeId.HasValue)
                    Types.Add(typeId.Value, cached);
                TypeDictionary.Add(type.Name, cached);
            }
            return cached;
        }
    }
}