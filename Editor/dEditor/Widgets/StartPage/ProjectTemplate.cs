// ProjectTemplate.cs - dEditor
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
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace dEditor.Widgets.StartPage
{
	public class ProjectTemplate
	{
		public ProjectTemplate(string name, Action loadTemplate)
		{
			Name = name;

			var thumbnail =
				new BitmapImage(new Uri($"/dEditor;component/Content/Templates/{name}/thumbnail.png", UriKind.Relative));
			thumbnail.Freeze();
			Thumbnail = thumbnail;
			Load = loadTemplate;
		}

		public string Name { get; }
		public Action Load { get; }
		public ImageSource Thumbnail { get; }
	}
}