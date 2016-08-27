// ReplayService.cs - dEngine
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
using System.Text;
using dEngine.Instances;
using dEngine.Instances.Attributes;

namespace dEngine.Services
{
	/// <summary>
	/// A service for recording and playing replays.
	/// </summary>
	/// <remarks>
	/// A replay is a stream of actions used to recreate game simulations. Checkpoints can be made, which contain the entire
	/// state at the time of the given frame.
	/// </remarks>
	[TypeId(138), ExplorerOrder(-1)]
	public sealed class ReplayService : Service
	{
		private ReplayFrame _frame;
		private bool _isRecording;
		private Stream _stream;
	    private BinaryWriter _writer;

	    /// <inheritdoc />
		public ReplayService()
		{
        }

	    private void OnHeartbeat(double t)
        {
            NextFrame();
        }

		/// <summary>
		/// If true, the replay service is currently recording frames.
		/// </summary>
		public bool IsRecording
		{
			get { return _isRecording; }
			private set
			{
				_isRecording = value;
				NotifyChanged();
			}
		}

		/// <summary>
		/// If true, the replay service is currently playing back a recording.
		/// </summary>
		public bool IsPlaying { get; private set; }

		internal static object GetExisting()
		{
			return DataModel.GetService<ReplayService>();
		}

		/// <summary>
		/// Serializes current frame to stream and creates a new replay frame.
		/// </summary>
		private void NextFrame()
		{
			if (!IsRecording)
				throw new InvalidOperationException("Could not save frame: recording not started.");

			//_replayTypeModel.SerializeWithLengthPrefix(_stream, _frame, typeof(ReplayFrame), PrefixStyle.Base128,
			//	_frame.FrameNumber);

			_frame = new ReplayFrame(_frame.FrameNumber + 1);
		}

		/// <summary>
		/// Starts recording replay actions.
		/// </summary>
		public void BeginRecording()
		{
			if (IsRecording)
				throw new InvalidOperationException("Could not begin recording: recording already started.");
			if (IsPlaying)
				throw new InvalidOperationException("Could not begin recording: playback in progress.");

			_stream = new MemoryStream();
            _writer = new BinaryWriter(_stream, Encoding.Unicode, true);
			_frame = new ReplayFrame(0);

            _writer.Write("dEngineDemo".ToCharArray());
            _writer.Write(Game.Workspace.Physics.SolverSeed);

            RunService.Service.Heartbeat.Disconnect(OnHeartbeat);
            IsRecording = true;
		}

		/// <summary>
		/// Makes the current frame a checkpoint.
		/// </summary>
		public void MakeCheckpoint()
		{
			if (!IsRecording)
				throw new InvalidOperationException("Could not set checkpoint: recording not started.");

			_frame.IsCheckpoint = true;
		}

		/// <summary>
		/// Stops the current recording and returns the stream.
		/// </summary>
		public Stream EndRecording()
		{
			if (!IsRecording)
				throw new InvalidOperationException("Could not end recording: recording already ended.");

			IsRecording = false;

			var stream = _stream;
			stream.Seek(0, SeekOrigin.Begin);

			_frame = null;
			_stream = null;

            RunService.Service.Heartbeat.Connect(OnHeartbeat);
            return stream;
		}

		/// <summary>
		/// Adds the given <see cref="ReplayAction" /> to the current frame.
		/// </summary>
		/// <param name="action"></param>
		public void RecordAction(ReplayAction action)
		{
			if (!IsRecording)
				throw new InvalidOperationException("Could not record action: recording not started.");

			_frame.Actions.Add(action);
		}

		/// <summary>
		/// A replay frame.
		/// </summary>
		public class ReplayFrame : IDataType
		{
			/// <summary>
			/// A list of replay actions performed this frame.
			/// </summary>
			public readonly List<ReplayAction> Actions;

			/// <summary>
			/// The frame number.
			/// </summary>
			public readonly int FrameNumber;

			/// <summary>
			/// Determines if this frame is a checkpoint.
			/// </summary>
			public bool IsCheckpoint;

			/// <summary>
			/// Initializes a new <see cref="ReplayFrame" />.
			/// </summary>
			/// <param name="frame"></param>
			public ReplayFrame(int frame)
			{
				FrameNumber = frame;
				Actions = new List<ReplayAction>();
			}

		    public void Load(BinaryReader reader)
		    {
		        throw new NotImplementedException();
		    }

		    public void Save(BinaryWriter writer)
		    {
                writer.Write(FrameNumber);
                writer.Write(IsCheckpoint);
                foreach (var action in Actions)
                {
                    action.Save(writer);
                }
            }
		}

		/// <summary>
		/// Base class for replay actions.
		/// </summary>
		public abstract class ReplayAction : IDataType
		{
			/// <summary>
			/// Performs the action.
			/// </summary>
			public abstract void Perform();
        
            /// <summary>
            /// Reads the action from a stream.
            /// </summary>
		    public abstract void Load(BinaryReader reader);
            /// <summary>
            /// Writes the action from to a stream.
            /// </summary>
		    public abstract void Save(BinaryWriter writer);
		}

		/// <summary>
		/// An input action.
		/// </summary>
		internal class InputAction : ReplayAction
		{
			public InputDeviceType InputDeviceType;
			public InputObject InputObject;

			public InputAction(InputDeviceType deviceType, InputObject io)
			{
				InputDeviceType = deviceType;
				InputObject = io;
            }

		    /// <summary>
		    /// Reads the action from a stream.
		    /// </summary>
		    public override void Load(BinaryReader reader)
            {
                InputDeviceType = (InputDeviceType)reader.Read();
                var inputObject = new InputObject();
                inputObject.Load(reader);
		        InputObject = inputObject;
            }

		    /// <summary>
		    /// Writes the action from to a stream.
		    /// </summary>
		    public override void Save(BinaryWriter writer)
		    {
		        writer.Write((byte)InputDeviceType);
		        InputObject.Save(writer);

		    }

            /// <inheritdoc />
            public override void Perform()
			{
				switch (InputDeviceType)
				{
					case InputDeviceType.Keyboard:
						InputService.Process(ref InputObject);
						break;
				}
			}
		}
	}
}