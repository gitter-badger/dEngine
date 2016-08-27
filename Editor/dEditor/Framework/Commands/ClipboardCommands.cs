// ClipboardCommands.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using dEngine;
using dEngine.Instances;
using dEngine.Serializer.V1;
using dEngine.Services;
using dEngine.Utility;
using MoreLinq;
using Key = System.Windows.Input.Key;

namespace dEditor.Framework.Commands
{
	public abstract class ClipboardCommand : Framework.Command
	{
		protected static void Copy(ICollection<Instance> items)
		{
			var data = new MemoryStream(256);
			using (var writer = new BinaryWriter(data))
			{
				data.Position += 4;
				int itemCount = 0;
				items.ForEach(item =>
				{
					Inst.Serialize(item, data);
					itemCount++;
				});
				data.Position = 0;
				writer.Write(itemCount);
				Clipboard.SetData("deditor-clipboard", data);
			}
		}

		protected static bool CanCopy()
		{
			return SelectionService.SelectionCount > 0 && SelectionService.All(x => x.Archivable && !x.ParentLocked);
		}

		protected static void Paste(Instance parent)
		{
			if (!Clipboard.ContainsData("deditor-clipboard"))
				return;
			SelectionService.Service.ClearSelection();
			var data = (Stream)Clipboard.GetData("deditor-clipboard");

			using (var reader = new BinaryReader(data))
			{
				var itemCount = reader.ReadInt32();
				for (int i = 0; i < itemCount; i++)
				{
					var item = Inst.Deserialize(data);
					item.Parent = parent;
					SelectionService.Service.Select(item);
				}
			}
		}
	}

	public class CopyCommand : ClipboardCommand
	{
		public override string Name => "Copy";
		public override string Text => "Copies the selected object.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.C, ModifierKeys.Control);

		public override bool CanExecute(object parameter)
		{
			return CanCopy();
		}

		public override void Execute(object parameter)
		{
			Copy(SelectionService.ToList());
		}
	}

	public class CutCommand : ClipboardCommand
	{
		public override string Name => "Cut";
		public override string Text => "Copies and deletes the selected object.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.X, ModifierKeys.Control);

		public override bool CanExecute(object parameter)
		{
			return CanCopy();
		}

		public override void Execute(object parameter)
		{
			try
			{
				var selection = SelectionService.ToList();
				Copy(selection);
				selection.ForEach(x => x.Destroy());
			}
			catch (ParentException e)
			{
				MessageBox.Show(e.Message, "dEditor", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}
	}

	public class PasteCommand : ClipboardCommand
	{
		public override string Name => "Paste";
		public override string Text => "Pastes object from clipboard.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.C, ModifierKeys.Control);

		public override bool CanExecute(object parameter)
		{
			return Clipboard.ContainsData(".deditor-clipboard");
		}

		public override void Execute(object parameter)
		{
			try
			{
				Instance parent = null;

				var last = SelectionService.Last();

				if (last != null)
				{
					if (last is Service)
						parent = last;
					else if (last.Parent is Service)
						parent = last.Parent;
				}

				parent = parent ?? Game.Workspace;
				Paste(parent);
			}
			catch (ParentException e)
			{
				MessageBox.Show(e.Message, "dEditor", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}
	}

	public class PasteIntoCommand : ClipboardCommand
	{
		public override string Name => "Paste Into";
		public override string Text => "Pastes an object from clipboard into the selected object.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.V, ModifierKeys.Shift | ModifierKeys.Control);

		public override bool CanExecute(object parameter)
		{
			return SelectionService.Any() && Clipboard.ContainsData("deditor-clipboard");
		}

		public override void Execute(object parameter)
		{
			try
			{
				var parent = SelectionService.Last();
				Paste(parent);
			}
			catch (ParentException e)
			{
				MessageBox.Show(e.Message, "dEditor", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}
	}

	public class DuplicateCommand : ClipboardCommand
	{
		public override string Name => "Duplicate";
		public override string Text => "Duplicates the selected objects.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.D, ModifierKeys.Control);

		public override bool CanExecute(object parameter)
		{
			return CanCopy();
		}

		public override void Execute(object parameter)
		{
			try
			{
				SelectionService.ForEach(x => x.Clone().Parent = x.Parent);
			}
			catch (ParentException e)
			{
				MessageBox.Show(e.Message, "dEditor", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}
	}

	public class DeleteCommand : ClipboardCommand
	{
		public override string Name => "Delete";
		public override string Text => "Deletes the selected objects.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.Delete, ModifierKeys.None);

		public override bool CanExecute(object parameter)
		{
			return SelectionService.Any();
		}

		public override void Execute(object parameter)
		{
			try
			{
				SelectionService.ForEach(x => x.Destroy());
			}
			catch (ParentException e)
			{
				MessageBox.Show(e.Message, "dEditor", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}
	}
}