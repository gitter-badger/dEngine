// ShellViewModel.cs - dEditor
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using dEditor.Dialogs.ProjectProperties;
using dEditor.Dialogs.Settings;
using dEditor.Framework;
using dEditor.Framework.Commands;
using dEditor.Framework.Services;
using dEditor.Shell.CommandBar;
using dEditor.Shell.Commands;
using dEditor.Shell.StatusBar;
using dEditor.Tools;
using dEditor.Tools.Building;
using dEditor.Widgets.Viewport;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace dEditor.Shell
{
	public class ShellViewModel : Conductor<Document>.Collection.OneActive
	{
		private LayoutItem _activeLayoutItem;

	    public ShellViewModel()
		{
			Widgets = new ObservableCollection<Widget>();
			RecentCommands = new ObservableCollection<string>();
			LayourItemStatePersister = new LayourItemStatePersister();

			ToolManager.SelectedToolChanged += tool =>
			{
				NotifyOfPropertyChange(nameof(ToolIncrement));
				NotifyOfPropertyChange(nameof(IsBuildTool));
				IncrementOptions = (tool as StudioBuildTool)?.IncrementOptions;
				NotifyOfPropertyChange(nameof(IncrementOptions));
			};

			SaveProjectCommand = new SaveProjectCommand(this);

			Editor.Current.ProjectChanged += project => NotifyOfPropertyChange(() => DisplayName);

			Editor.Current.Shell = this;
		}

	    public bool IsBuildTool => ToolManager.SelectedTool is StudioBuildTool;

		public object ToolIncrement
		{
			get { return (ToolManager.SelectedTool as StudioBuildTool)?.Increment ?? 1; }
			set
			{
				var btool = ToolManager.SelectedTool as StudioBuildTool;
				var dbl = value as double? ?? -1;
				if (btool != null && dbl >= 0)
					btool.Increment = dbl;
			}
		}

		public IEnumerable<double> IncrementOptions { get; private set; }

		public Document ActiveDocument
		{
			get { return ActiveItem; }
			set { ActiveItem = value; }
		}

		public ObservableCollection<Widget> Widgets { get; }
		public ObservableCollection<string> RecentCommands { get; }
		public bool ShowFloatingWindowsInTaskbar { get; set; }
		public IEnumerable<object> FloatingWindows { get; internal set; }
		public LayourItemStatePersister LayourItemStatePersister { get; }
		public ShellView View { get; private set; }
		public ViewportViewModel Viewport { get; set; }

		public override string DisplayName
		{
			get
			{
				return Editor.Current.Project != null
					? $"{Editor.Current.Project.Name} - dEditor"
					: "dEditor";
			}
			set { }
		}

		public LayoutItem ActiveLayoutItem
		{
			get { return _activeLayoutItem; }
			set
			{
				if (ReferenceEquals(_activeLayoutItem, value))
					return;

				_activeLayoutItem = value;

				var doc = value as Document;
				if (doc != null)
					ActivateItem(doc);

				NotifyOfPropertyChange(() => ActiveLayoutItem);
			}
		}

		public event Action<Document> ActiveDocumentChanging;
		public event Action<Document> ActiveDocumentChanged;

		public override void ActivateItem(Document item)
		{
			ActiveDocumentChanging?.Invoke(item);

			var lastItem = ActiveItem;

			base.ActivateItem(item);

			if (!ReferenceEquals(item, lastItem))
				ActiveDocumentChanged?.Invoke(item);
		}

		protected override void OnViewLoaded(object view)
		{
			View = (ShellView)view;
		}

		internal void SaveLayout(Stream stream)
		{
			var serializer = new XmlLayoutSerializer(View.DockingManager);
			serializer.Serialize(stream);
		}

		internal void LoadLayout(Stream stream, Action<Widget> addToolCallback, Action<Document> addDocumentCallback,
			Dictionary<string, LayoutItem> itemsState)
		{
			var serializer = new XmlLayoutSerializer(View.DockingManager);

			serializer.LayoutSerializationCallback += (s, e) =>
			{
				LayoutItem item;

				if (itemsState.TryGetValue(e.Model.ContentId, out item))
				{
					e.Content = item;

					var tool = item as Widget;
					var anchorable = e.Model as LayoutAnchorable;

					var document = item as Document;
					var layoutDocument = e.Model as LayoutDocument;

					if (tool != null && anchorable != null)
					{
						addToolCallback(tool);
						tool.IsVisible = anchorable.IsVisible;

						if (anchorable.IsActive)
							((IActivate)tool).Activate();

						tool.IsSelected = e.Model.IsSelected;

						return;
					}

					if (document != null && layoutDocument != null)
					{
						addDocumentCallback(document);

						// Nasty hack to get around issue that occurs if documents are loaded from state,
						// and more documents are opened programmatically.
						layoutDocument.GetType()
							.GetProperty("IsLastFocusedDocument")
							.SetValue(layoutDocument, false, null);

						document.IsSelected = layoutDocument.IsSelected;
						return;
					}
				}

				e.Cancel = true;
			};

			serializer.Deserialize(stream);
		}

		public void ShowProjectProperties()
		{
			if (Project.Current != null)
			{
				var projectVm = new ProjectViewModel();
				Editor.Current.WindowManager.ShowDialog(projectVm, null, projectVm.GetDialogSettings());
			}
		}

		/// <summary>
		/// Shows an instance of a dialog.
		/// </summary>
		public void ShowDialog(Dialog dialog)
		{
		    Editor.Current.WindowManager.ShowDialog(dialog, null, new Dictionary<string, object>
		    {
                {"MinWidth", dialog.MinWidth},
                {"MinHeight", dialog.MinHeight},
                {"MaxWidth", dialog.MaxWidth},
                {"MaxHeight", dialog.MaxHeight},
                {"Width", dialog.Width},
                {"Height", dialog.Height},
                {"ShowInTaskbar", false},
                {"WindowStyle", WindowStyle.ToolWindow},
            });

			dialog.IsVisible = true;
		}

		/// <summary>
		/// Shows an instance of a tool.
		/// </summary>
		public void ShowTool(Widget tool)
		{
			if (tool.IsVisible)
				throw new InvalidOperationException("Tool already shown.");

			Widgets.Add(tool);
			tool.IsVisible = true;
		}

		/// <summary>
		/// Hides an instance of a tool.
		/// </summary>
		public void HideTool(Widget tool)
		{
			Widgets.Remove(tool);
			tool.IsVisible = false;
		}

		/// <summary>
		/// Opens an instance of a document.
		/// </summary>
		public void OpenDocument(Document document)
		{
			if (document.IsVisible)
				throw new InvalidOperationException("Document already open.");

			Items.Add(document);

			ActiveLayoutItem = document;

			document.IsVisible = true;
		}

		/// <summary>
		/// Closes an instance of a document.
		/// </summary>
		public void CloseDocument(Document document)
		{
			if (document == Viewport)
			{
				Viewport = null;
			}

			DeactivateItem(document, true);
		}

		public void ClosePlace()
		{
			CloseDocument(Viewport);
		}

		public override void DeactivateItem(Document item, bool close)
		{
			ActiveDocumentChanging?.Invoke(item);

			base.DeactivateItem(item, close);

			ActiveDocumentChanged?.Invoke(item);
		}

		public void OpenSettingsView()
		{
			var w = 860;
			var h = 400;

			var settings = new SettingsViewModel();
			Editor.Current.WindowManager.ShowDialog(settings, null,
				new Dictionary<string, object>
				{
					{"MinWidth", w},
					{"MinHeight", h},
					{"MaxWidth", w},
					{"MaxHeight", h},
					{"Title", "Settings"},
					{"WindowStyle", WindowStyle.ToolWindow}
				});
		}

		public void OpenView(Type viewModelType)
		{
			if (viewModelType == null) return;

			Widget tool;
			Document doc;

			// if the document/tool is already open, highlight it.
			if (typeof(Document).IsAssignableFrom(viewModelType))
			{
				if ((doc = Items.FirstOrDefault(x => x.GetType() == viewModelType)) != null)
				{
					ActiveLayoutItem = doc;
					return;
				}
			}
			else if (typeof(Widget).IsAssignableFrom(viewModelType))
			{
				if ((tool = Widgets.FirstOrDefault(x => x.GetType() == viewModelType)) != null)
				{
					tool.IsVisible = true;
					return;
				}
			}

			var item = (LayoutItem)Activator.CreateInstance(viewModelType);
			tool = item as Widget;
			doc = item as Document;
			var dialog = item as Dialog;

			if (tool != null)
				ShowTool(tool);
			else if (doc != null)
				OpenDocument(doc);
			else if (dialog != null)
				ShowDialog(dialog);
			else
				throw new ArgumentOutOfRangeException();
		}

		#region Commands

		public UnionCommand UnionCommand { get; set; } = new UnionCommand();
		public NegateCommand NegateCommand { get; set; } = new NegateCommand();
		public SeparateCommand SeparateCommand { get; set; } = new SeparateCommand();

		public CutCommand CutCommand { get; set; } = new CutCommand();
		public CopyCommand CopyCommand { get; set; } = new CopyCommand();
		public PasteCommand PasteCommand { get; set; } = new PasteCommand();
		public PasteIntoCommand PasteIntoCommand { get; } = new PasteIntoCommand();
		public DuplicateCommand DuplicateCommand { get; } = new DuplicateCommand();
		public DeleteCommand DeleteCommand { get; } = new DeleteCommand();

		public SaveProjectCommand SaveProjectCommand { get; }
		public OpenProjectCommand OpenProjectCommand { get; } = new OpenProjectCommand();
		public NewProjectCommand NewProjectCommand { get; } = new NewProjectCommand();
		public NewPlaceCommand NewPlaceCommand { get; } = new NewPlaceCommand();
		public GroupCommand GroupCommand { get; } = new GroupCommand();
		public UngroupCommand UngroupCommand { get; } = new UngroupCommand();

		public PlayCommand PlayCommand { get; } = new PlayCommand();
		public RunCommand RunCommand { get; } = new RunCommand();
		public PauseCommand PauseCommand { get; } = new PauseCommand();
		public StopCommand StopCommand { get; } = new StopCommand();

		public UndoCommand UndoCommand { get; } = new UndoCommand();
		public RedoCommand RedoCommand { get; } = new RedoCommand();

        public ExecuteScriptCommand ExecuteScriptCommand { get; } = new ExecuteScriptCommand();

	    public CommandBarViewModel CommandBar { get; set; } = new CommandBarViewModel();
        public StatusBarViewModel StatusBar { get; set; } = new StatusBarViewModel();

        #endregion
    }
}