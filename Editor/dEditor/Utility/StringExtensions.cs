// StringExtensions.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;
using dEditor.Framework;

namespace dEditor.Utility
{
	public static class StringExtensions
	{
		private static readonly int _contentLength = "Content/".Length;

		public static string ToContentId(this string path, Project project = null)
		{
			project = project ?? Project.Current;

			return !path.StartsWith(project.ContentPath)
				? $"file://{path}"
				: $"content://{path.Substring(project.ContentPath.Length + 1)}";

			/*
            var contentUri = new Uri(project.ContentPath, UriKind.Absolute);
            var uri = new Uri(url, UriKind.Absolute);
            var relUri = contentUri.MakeRelativeUri(uri).ToString().Substring(_contentLength);

            return $"content://{relUri}";
            */
		}

		public static bool ValidateFileName(this string fileName)
		{
			var unsupported =
				new Regex("(^(PRN|AUX|NUL|CON|COM[1-9]|LPT[1-9]|(\\.+)$)(\\..*)?$)|(([\\x00-\\x1f\\\\?*:\";‌​|/<>])+)|([\\. ]+)",
					RegexOptions.IgnoreCase);
			return !unsupported.IsMatch(fileName);
		}
	}
}