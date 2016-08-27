// ToolManager.cs - dEditor
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
using dEditor.Tools.Building;
using dEngine;
using dEngine.Instances;
using dEngine.Services;

namespace dEditor.Tools
{
	public static class ToolManager
	{
		private static Instance _primaryItem;
		private static StudioTool _selectedTool;

		static ToolManager()
		{
			MoveTool = new MoveTool();
			SelectTool = new SelectTool();
			ScaleTool = new ScaleTool();
			RotateTool = new RotateTool();

		    SelectionService.Service.SelectionChanged.Event += () =>
		    {
                ContextActionService.SetState("selectionEmpty", SelectionService.SelectionCount == 0);
            };

            ContextActionService.Register("tools.selectTool", () => SelectTool.IsEquipped = true);
            ContextActionService.Register("tools.moveTool", () => MoveTool.IsEquipped = true);
            ContextActionService.Register("tools.scaleTool", () => ScaleTool.IsEquipped = true);
            ContextActionService.Register("tools.rotateTool", () => RotateTool.IsEquipped = true);
            ContextActionService.Register("pickerInsideModels", PickerInsideModels);
            ContextActionService.Register("groupSelection", GroupSelection);
            ContextActionService.Register("ungroupSelection", UngroupSelection);
        }

	    private static void GroupSelection()
        {
            var parent = SelectionService.Unanimous(i => i.Parent) ?? Game.Workspace;
            var model = new Model { Parent = parent };
            foreach (var item in SelectionService.Selection)
            {
                item.Parent = model;
            }
        }

        private static void UngroupSelection()
        {
            var parent = SelectionService.Unanimous(i => i.Parent) ?? Game.Workspace;

            foreach (var item in SelectionService.Selection)
            {
                var model = item as Model;

                if (model != null)
                {
                    var children = model.Children;
                    children.EnterUpgradeableReadLock();
                    for (var i = model.Children.Count - 1; i >= 0; i--)
                    {
                        children[i].Parent = parent;
                    }
                    children.ExitUpgradeableReadLock();
                }
            }
        }

        public static void PickerInsideModels(bool pressed)
	    {

        }

        public static void RotateSelectionY()
        {

        }

        public static void RotateSelectionX()
        {

        }

        public static void ToggleCoordinates()
        {
        }

        public static MoveTool MoveTool { get; }
		public static SelectTool SelectTool { get; }
		public static ScaleTool ScaleTool { get; }
		public static RotateTool RotateTool { get; }

		public static StudioTool SelectedTool
		{
			get { return _selectedTool; }
			internal set
			{
				_selectedTool = value;
				SelectedToolChanged?.Invoke(value);
			}
		}

		public static Instance PrimaryItem
		{
			get { return _primaryItem; }
			set
			{
				var last = _primaryItem;
				_primaryItem = value;
				PrimaryItemChanged?.Invoke(value, last);
			}
		}

		public static event Action<Instance, Instance> PrimaryItemChanged;
		public static event Action<StudioTool> SelectedToolChanged;
	}
}