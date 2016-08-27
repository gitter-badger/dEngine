// CsgCommands.cs - dEditor
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
using System.Linq;
using System.Windows.Input;
using dEngine;
using dEngine.Instances;
using dEngine.Services;
using MoreLinq;
using Key = System.Windows.Input.Key;

namespace dEditor.Framework.Commands
{
	public abstract class CsgCommand : Framework.Command
	{
		public CsgCommand()
		{
			Game.Selection.SelectionChanged.Event +=
				() => Editor.Current.Dispatcher.InvokeAsync(UpdateCanExecute);
		}
	}

	public class UnionCommand : CsgCommand
	{
		public override string Name => "Union";
		public override string Text => "Merge selected parts.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.G, ModifierKeys.Control | ModifierKeys.Shift);

		public override bool CanExecute(object parameter)
		{
			return SelectionService.Any(x => x is Part);
		}

		public override void Execute(object parameter)
		{
			var selection = SelectionService.OfType<Part>();
			SolidModelingManager.Union(selection);
		}
	}

	public class NegateCommand : CsgCommand
	{
		public override string Name => "Negate";
		public override string Text => "Negates the part.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.N, ModifierKeys.Control | ModifierKeys.Shift);

		public override bool CanExecute(object parameter)
		{
			return SelectionService.Any(x => x is Part);
		}

		public override void Execute(object parameter)
		{
			var selection = SelectionService.OfType<Part>();
			selection.ForEach(x => SolidModelingManager.MakeOp<NegateOperation>(x));
		}
	}

	public class IntersectCommand : CsgCommand
	{
		public override string Name => "Intersect";
		public override string Text => "The common area between two objects.";
		public override KeyGesture KeyGesture => new KeyGesture(Key.None);

		public override bool CanExecute(object parameter)
		{
			return SelectionService.Any(x => x is Part);
		}

		public override void Execute(object parameter)
		{
			var selection = SelectionService.OfType<Part>();
			selection.ForEach(x => SolidModelingManager.MakeOp<IntersectOperation>(x));
		}
	}

	public class SeparateCommand : CsgCommand
	{
		public override string Name => "Seperate";
		public override string Text => "Seperate parts from a fused selection.";

		public override KeyGesture KeyGesture
			=> new KeyGesture(Key.DbeCodeInput, ModifierKeys.Control | ModifierKeys.Shift);

		public override bool CanExecute(object parameter)
		{
			return SelectionService.Any() && SelectionService.All(x => x is PartOperation);
		}

		public override void Execute(object parameter)
		{
			var selection = SelectionService.OfType<PartOperation>();
			SolidModelingManager.Seperate(selection.First());
		}
	}
}