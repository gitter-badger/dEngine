// MaterialEditorViewModel.cs - dEditor
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using dEditor.Framework;
using dEditor.Widgets.ContentBrowser;
using dEditor.Widgets.MaterialEditor.Nodes;
using dEngine;
using dEngine.Instances;
using dEngine.Instances.Materials;
using dEngine.Services;
using dEngine.Utility;
using Button = dEngine.Instances.Button;
using FlowDirection = dEngine.FlowDirection;
using Frame = dEngine.Instances.Frame;

namespace dEditor.Widgets.MaterialEditor
{
	public sealed class MaterialEditorViewModel : Document
	{
		private readonly Character _character;
		private readonly TextLabel _debugLabel;
		private readonly ScreenGui _viewportGui;
		private CanvasWin32 _canvas;

		private double _deltaTime;
		private bool _displayDebugText;
		private Material _material;
		private PreviewShape _preview;
		private Mesh _previewMesh;
		private Part _previewPart;

        public Material Material { get; }

        public ObservableCollection<NodeViewModel> Nodes { get; }

		public MaterialEditorViewModel(Material material)
		{
            Material = material;
			DisplayName = material.Name;
			DisplayDebugText = true;

		    Nodes = new ObservableCollection<NodeViewModel>();
            material.Nodes.ForEach(n => Nodes.Add(new NodeViewModel(n)));

            _viewportGui = new ScreenGui {Name = "MaterialViewportGui"};
			_character = new Character {Parent = _canvas};

			_debugLabel = new TextLabel
			{
				TextColour = Colour.DebugForeground,
				BorderThickness = 0,
				BackgroundColour = Colour.DebugBackground,
				TextAlignmentX = AlignmentX.Left,
				TextAlignmentY = AlignmentY.Top,
				FontSize = 14,
				Parent = _viewportGui
			};

			var previewStack = new Stack
			{
				Parent = _viewportGui,
				Position = new UDim2(0, 4, 0, 4),
				Size = new UDim2(0, 26 * 5, 0, 26),
				Offset = new Vector2(4, 4),
				Orientation = FlowDirection.Horizontal,
				BackgroundColour = Colour.Transparent,
				AlignmentY = AlignmentY.Bottom,
				AlignmentX = AlignmentX.Right
			};

			var previewStackButton = new Button
			{
				Size = new UDim2(0, 26, 0, 26),
				BackgroundColour = new Colour(0, 0, 0, 0.4f),
				BorderThickness = 0
			};

			using (previewStackButton)
			{
				var cylinderButton = MakePreviewButton(PreviewShape.Cylinder, "Toolbar/CylinderPreview");
				cylinderButton.FrameIndex = 0;
				cylinderButton.Parent = previewStack;

				var sphereButton = MakePreviewButton(PreviewShape.Sphere, "Toolbar/SpherePreview");
				sphereButton.FrameIndex = 1;
				sphereButton.Parent = previewStack;

				var planeButton = MakePreviewButton(PreviewShape.Plane, "Toolbar/PlanePreview");
				planeButton.FrameIndex = 2;
				planeButton.Parent = previewStack;

				var cubeButton = MakePreviewButton(PreviewShape.Cube, "Toolbar/CubePreview");
				cubeButton.FrameIndex = 3;
				cubeButton.Parent = previewStack;

				var meshButton = MakePreviewButton(PreviewShape.Mesh, "Toolbar/TeapotPreview");
				meshButton.FrameIndex = 4;
				meshButton.Parent = previewStack;
            }

            RunService.Service.Heartbeat.Event += UpdateDebugText;
        }

		public bool DisplayDebugText
		{
			get { return _displayDebugText; }
			set
			{
				if (value == _displayDebugText) return;
				_displayDebugText = value;
				NotifyOfPropertyChange();
			}
		}

		public PreviewShape Preview
		{
			get { return _preview; }
			set
			{
				if (value == _preview) return;

				_previewMesh?.Destroy();

				switch (value)
				{
					case PreviewShape.Cylinder:
						_previewPart.Shape = Shape.Cylinder;
						break;
					case PreviewShape.Sphere:
						_previewPart.Shape = Shape.Sphere;
						break;
					case PreviewShape.Plane:
						_previewMesh = new PlaneMesh {Parent = _previewPart};
						break;
					case PreviewShape.Cube:
						_previewPart.Shape = Shape.Cube;
						break;
					case PreviewShape.Mesh:
						var contentBrowser = IoC.Get<ContentBrowserViewModel>();
						var mesh =
							contentBrowser.SelectedContents?.FirstOrDefault(
								c => c.Type == ContentType.StaticMesh || c.Type == ContentType.SkeletalMesh);
						if (mesh == null)
						{
							MessageBox.Show("A mesh must be selected in the content browser preview it.");
							return;
						}

						_previewMesh?.Destroy();
						_previewMesh = (Mesh)InsertService.Service.LoadAsset(mesh.File.FullName);

						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				_preview = value;

				NotifyOfPropertyChange();
			}
		}

		public enum PreviewShape
		{
			Cylinder,
			Sphere,
			Plane,
			Cube,
			Mesh
		}

		public Canvas Canvas { get; set; }
        
		private void UpdateDebugText(double deltaTime)
		{
			_deltaTime += (deltaTime - _deltaTime) * 0.1;
			var msec = _deltaTime * 1000.0;
			var fps = (int)(1.0f / _deltaTime);

			if (!_displayDebugText)
				return;

			_debugLabel.Text = $@" FPS: {fps} ({msec.ToString("F")}ms)";
		}

		private GuiElement MakePreviewButton(PreviewShape shape, string icon)
		{
			var button = new Frame
			{
				Name = shape.ToString(),
				Size = new UDim2(0, 26, 0, 26),
				BackgroundColour = new Colour(0, 0, 0, 0.4f),
				BorderThickness = 0
			};

            button.MouseEnter.Connect((x, y) =>
            {
                button.BackgroundColour = new Colour(0, 0, 0, 0.8f); 
            });

            button.MouseLeave.Connect((x, y) =>
            {
                button.BackgroundColour = new Colour(0, 0, 0, 0.4f);
            });

            // ReSharper disable once ObjectCreationAsStatement
            new ImageLabel
			{
				BackgroundColour = Colour.Transparent,
				BorderThickness = 0,
				Size = new UDim2(0, 16, 0, 16),
				AlignmentX = AlignmentX.Center,
				AlignmentY = AlignmentY.Middle,
				ImageId = $"editor://Icons/{icon}.png",
				Parent = button
			};

			button.MouseButton1Up.Event += (x, y) => { Preview = shape; };

		    return button;
		}

		public void OnHandleSet(IntPtr handle)
		{
		    _canvas = new CanvasWin32(handle)
		    {
		        Parent = Game.CoreEnvironment,
		        CurrentCamera =
		        {
		            CFrame = new CFrame(0, 0, -10) * CFrame.Angles(0, Mathf.Pi, 0),
		            CameraSubject = _character,
		            CameraType = CameraType.Custom
		        }
		    };
		    _viewportGui.Parent = _canvas.CurrentCamera;
			_previewPart = new Part {Parent = _canvas, Size = new Vector3(5, 5, 5)};
            
            Preview = PreviewShape.Sphere;
        }

		protected override void OnDeactivate(bool close)
		{
			base.OnDeactivate(close);

			if (close)
			{
				_canvas?.Dispose();
				_previewPart.Destroy();
			}
		}
	}
}