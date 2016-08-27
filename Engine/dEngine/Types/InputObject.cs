// InputObject.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.


using System.IO;

namespace dEngine
{
    /// <summary>
    /// An object which describes an input action.
    /// </summary>
    public class InputObject : IDataType
    {
        /// <summary>
        /// Determines if this input has been handled.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// The pressed key.
        /// </summary>
        [InstMember(1)] public Key Key { get; private set; }

        /// <summary>
        /// The state of the input.
        /// </summary>
        [InstMember(2)] public InputState InputState { get; private set; }

        /// <summary>
        /// The type of input.
        /// </summary>
        [InstMember(3)] public InputType InputType { get; private set; }

        /// <summary>
        /// Describes the positional value.
        /// </summary>
        [InstMember(4)] public Vector3 Position { get; private set; }

        /// <summary>
        /// Describes the delta for mouse/stick movements.
        /// </summary>
        [InstMember(5)] public Vector3 Delta { get; private set; }

        internal InputObject(Key key, InputState inputState, InputType inputType)
        {
            Key = key;
            InputState = inputState;
            InputType = inputType;
            Position = Vector3.Zero;
            Delta = Vector3.Zero;
        }

        internal InputObject(Key key, InputState inputState, InputType inputType, Vector3 position, Vector3 delta)
        {
            Key = key;
            InputState = inputState;
            InputType = inputType;
            Position = position;
            Delta = delta;
        }

        /// <summary/>
        public InputObject()
        {
        }

        /// <summary/>
        public void Load(BinaryReader reader)
        {
            Key = (Key)reader.ReadInt32();
            InputState = (InputState)reader.ReadByte();
            InputType = (InputType)reader.ReadByte();
            var position = new Vector3();
            var delta = new Vector3();
            position.Load(reader);
            delta.Load(reader);
            Position = position;
            Delta = delta;
        }

        /// <summary/>
        public void Save(BinaryWriter writer)
        {
            writer.Write((int)Key);
            writer.Write((byte)InputState);
            writer.Write((byte)InputType);
            Position.Save(writer);
            Delta.Save(writer);
        }
    }
}