// UnitTestDetector.cs - dEngine
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
using System.Linq;

namespace dEngine.Utility
{
	/// <summary>
	/// Detects if we are running inside a unit test.
	/// </summary>
	public static class UnitTestDetector
	{
		static UnitTestDetector()
		{
			string testAssemblyName = "Microsoft.VisualStudio.QualityTools.UnitTestFramework";
			UnitTestDetector.IsInUnitTest = AppDomain.CurrentDomain.GetAssemblies()
				.Any(a => a.FullName.StartsWith(testAssemblyName));
		}

		public static bool IsInUnitTest { get; private set; }
	}
}