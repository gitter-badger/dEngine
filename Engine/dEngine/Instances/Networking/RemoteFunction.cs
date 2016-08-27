// RemoteFunction.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using dEngine.Instances.Attributes;
using Neo.IronLua;


namespace dEngine.Instances
{
	/// <summary>
	/// A networked method.
	/// </summary>
	[TypeId(45), ToolboxGroup("Scripting")]
	public sealed class RemoteFunction : Instance
	{
		private DeliveryMethod _deliveryMethod;

		/// <inheritdoc />
		public RemoteFunction()
		{
			_deliveryMethod = DeliveryMethod.ReliableUnordered;
		}

		/// <summary>
		/// The delivery method to use.
		/// </summary>
		[InstMember(1)]
		public DeliveryMethod DeliveryMethod
		{
			get { return _deliveryMethod; }
			set
			{
				if (value == _deliveryMethod) return;
				_deliveryMethod = value;
				NotifyChanged(nameof(DeliveryMethod));
			}
		}

		/// <summary>
		/// The callback for when the server invokes this function.
		/// </summary>
		/// <remarks>
		/// This function will execute on the client everytime the server invokes it with InvokeClient()
		/// </remarks>
		public LuaMethod OnClientInvoke { get; set; }

		/// <summary>
		/// The callback for when the client invokes this function.
		/// </summary>
		/// <remarks>
		/// This function will execute on the server everytime the client invokes it with InvokeServer()
		/// </remarks>
		public LuaMethod OnServerInvoke { get; set; }

		/// <summary>
		/// Invokes the function that the provided player has bound to this instance.
		/// </summary>
		public LuaResult InvokeClient(Player player, params dynamic[] args)
		{
			return null;
		}

		/// <summary>
		/// Invokes the function that the server has bound to this instance.
		/// </summary>
		public LuaResult InvokeServer(params dynamic[] args)
		{
			return null;
		}
	}
}