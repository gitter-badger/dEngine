// Tool.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Concurrent;
using System.Diagnostics;
using Caliburn.Micro;
using dEngine;
using dEngine.Instances;
using dEngine.Services;
using SharpDX;

namespace dEditor.Tools
{
	public abstract class StudioTool : PropertyChangedBase
	{
		protected readonly SelectionBox _hoverBox;
		private readonly ConcurrentDictionary<PVInstance, SelectionBox> _selectionBoxes;
		private readonly Stopwatch _stopwatch;
		private Colour _hoverColourA = Colour.fromRGB(177, 229, 255);
		private Colour _hoverColourB = Colour.fromRGB(39, 160, 255);
		private float _index;
		private bool _isEquipped;
	    private readonly Colour _primaryHoverColour = Colour.fromRGB(255, 255, 0);
		protected PVInstance _target;

		protected StudioTool()
		{
			_stopwatch = Stopwatch.StartNew();
			_selectionBoxes = new ConcurrentDictionary<PVInstance, SelectionBox>();

			_hoverBox = new SelectionBox
			{
				Name = "SelectToolMouseOverBox",
				Parent = Game.CoreGui
			};

			InputService.Service.InputBegan.Event += i =>
			{
				if (IsEquipped) OnMouseInput(i);
			};
			InputService.Service.InputChanged.Event += i =>
			{
				if (IsEquipped) OnMouseInput(i);
			};
			InputService.Service.InputEnded.Event += i =>
			{
				if (IsEquipped) OnMouseInput(i);
			};

			Game.Selection.Selected.Event += i =>
			{
				if (IsEquipped) OnItemSelected(i);
			};
			Game.Selection.Deselected.Event += i =>
			{
				if (IsEquipped) OnItemDeselected(i);
			};

			RunService.Service.Heartbeat.Event += elapsed =>
			{
				if (_isEquipped)
				{
					OnHeartbeat(elapsed);
				}
			};

			ToolManager.PrimaryItemChanged += OnPrimaryItemChanged;
		}

		public bool IsEquipped
		{
			get { return _isEquipped; }
			set
			{
				if (value == _isEquipped) return;
				_isEquipped = value;

				if (value)
				{
					OnEquipped();
				    _hoverBox.Visible = true;
					SelectionService.ForEach(OnItemSelected);
				}
				else
				{
					OnUnequipped();
					_hoverBox.Visible = false;
					SelectionService.ForEach(OnItemDeselected);
				}

				var currentTool = ToolManager.SelectedTool;
				if (currentTool != null)
					currentTool.IsEquipped = false;
				ToolManager.SelectedTool = this;
				NotifyOfPropertyChange();
			}
		}

		private void OnPrimaryItemChanged(Instance primary, Instance lastPrimary)
		{
			if (!IsEquipped)
				return;

			var lastPV = lastPrimary as PVInstance;
			if (lastPV != null)
			{
				SelectionBox lastBox;
				if (_selectionBoxes.TryGetValue(lastPV, out lastBox))
					lastBox.Colour = SelectionBox.DefaultColour;
			}

			var primaryPV = primary as PVInstance;
			if (primaryPV != null)
			{
			    SelectionBox selBox;
			    if (_selectionBoxes.TryGetValue(primaryPV, out selBox))
                    selBox.Colour = _primaryHoverColour;
			}
		}

		protected abstract void OnEquipped();
		protected abstract void OnUnequipped();

		protected virtual void OnItemSelected(Instance instance)
		{
			var pv = instance as PVInstance;

			if (pv == null)
				return;

			var box = new SelectionBox
			{
				Adornee = pv,
				Parent = Game.CoreGui,
				LineThickness = pv is Model ? 0.1f : 0.05f
			};

            pv.Changed.Connect(PVInstanceOnChanged);

			_selectionBoxes.TryAdd(pv, box);

			ToolManager.PrimaryItem = pv;

            UpdateHandles();
        }

	    private void PVInstanceOnChanged(string prop)
	    {
	        switch (prop)
            {
                case nameof(PVInstance.CFrame):
                case nameof(PVInstance.Size):
                    UpdateHandles();
	                break;
	        }
	    }

	    protected virtual void OnItemDeselected(Instance instance)
		{
			var pv = instance as PVInstance;

			if (pv == null)
				return;

			SelectionBox box;

			if (_selectionBoxes.TryRemove(pv, out box))
				box.Destroy();

			if (ToolManager.PrimaryItem == instance)
			{
				ToolManager.PrimaryItem = SelectionService.Last();
			}

		    UpdateHandles();

		}

		private Model GetRootModel(Instance part)
		{
			Instance parent = part.Parent;
			Model model = null;

			while (parent != null)
			{
				model = parent as Model;

				if (model?.Parent is IWorld)
					break;

				parent = parent.Parent;
			}

			return model;
		}

		protected virtual void OnHeartbeat(double dt)
		{
			var camera = Game.FocusedCamera;

			if (camera == null)
				return;

		    return;

			var result = Game.Workspace.Physics.FindPartOnRay(camera.GetMouseRay());
			var hitObject = result.HitObject;

			if (hitObject != null && !BaseHandles.IsMouseOverHandle)
			{
				var model = GetRootModel(hitObject);

				if (model != null && !InputService.Service.IsKeyDown(Key.LeftAlt))
				{
					_target = model;
				}
				else
				{
					_target = hitObject;
				}
			}
			else
			{
				_target = null;
			}
            
			var deltaTime = (float)(_stopwatch.Elapsed.TotalSeconds / (1 / 60f));
			_stopwatch.Restart();

			_index += 0.015f * deltaTime;

			if (_index >= 1.0)
			{
				_index = 0;

				var a = _hoverColourA;
				var b = _hoverColourB;

				_hoverColourA = b;
				_hoverColourB = a;
			}

			_hoverBox.Colour = _hoverColourA.lerp(_hoverColourB, _index);

		    if (_target is Model)
			{
				_hoverBox.LineThickness = 0.15f;
			}
			else
			{
				_hoverBox.LineThickness = 0.08f;
			}

			_hoverBox.Adornee = _target;
		}

		protected virtual void OnMouseInput(InputObject input)
		{
			if (input.InputType != InputType.MouseButton1 || input.InputState != InputState.Begin ||
				BaseHandles.IsMouseOverHandle)
				return;

			var sel = SelectionService.First();

			if (_target == null)
			{
				Game.Selection.ClearSelection();
				return;
			}

			if (_target.IsSelected)
			{
				ToolManager.PrimaryItem = _target;
				return;
			}

			if (InputService.Service.IsKeyDown(Key.LeftControl))
			{
				if (_target != sel)
					Game.Selection.Select(_target);
				else
					Game.Selection.Deselect(_target);
			}
			else
			{
				Game.Selection.Select(_target, true);
			}
		}

		protected virtual void UpdateHandles()
		{
			foreach (var selectionBox in _selectionBoxes.Values)
			{
				selectionBox.Dirty();
			}
		}
	}
}