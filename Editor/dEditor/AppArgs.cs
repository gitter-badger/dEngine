// AppArgs.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;

namespace dEditor
{
	public class AppArgs
	{
		public bool UseClientShell { get; set; }
        public string API { get; set; }

		public static AppArgs Parse(string[] args)
		{
			if (args.Length % 2 != 0)
				throw new ArgumentOutOfRangeException(nameof(args), "Args must be in key-value pairs. (divisible by 2)");

			var type = typeof(AppArgs);
			var argsObj = new AppArgs();

			for (int i = 0; i < args.Length; i += 2)
			{
				var name = args[i].TrimStart('/', '-');
				var value = args[i + 1];

				var property = type.GetProperty(name);

				if (property == null)
				{
					Debug.WriteLine($"No argument defined in AppArgs for \"{name}\"");
					continue;
				}

				property.SetValue(argsObj, Convert.ChangeType(value, property.PropertyType));
			}

			return argsObj;
		}
	}
}