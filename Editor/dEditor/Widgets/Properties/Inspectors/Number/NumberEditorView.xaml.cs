// NumberEditorView.xaml.cs - dEditor
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Windows;
using System.Windows.Data;
using dEditor.Framework.Converters;

namespace dEditor.Widgets.Properties.Inspectors.Number
{
    /// <summary>
    /// Interaction logic for NumberEditorView.xaml
    /// </summary>
    public partial class NumberEditorView
    {
        public static DependencyProperty CurrentConverterProperty = DependencyProperty.Register(
            nameof(CurrentConverter), typeof(NumberTextConverter), typeof(NumberEditorView), null);

        public NumberEditorView()
        {
            DataContextChanged += OnDataContextChanged;
            InitializeComponent();
            var textBinding = new Binding("DoubleValue") { Converter = CurrentConverter = new NumberTextConverter() };
            NumberTextBox.SetBinding(System.Windows.Controls.TextBox.TextProperty, textBinding);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            UIElement_OnLostFocus(null, null);
        }

        public NumberTextConverter CurrentConverter
        {
            get { return (NumberTextConverter)GetValue(CurrentConverterProperty); }
            set { SetValue(CurrentConverterProperty, value); }
        }

        private void UIElement_OnGotFocus(object sender, RoutedEventArgs e)
        {
            dynamic vm = DataContext;
            CurrentConverter.DecimalCount = vm.IsRanged ? 4 : 18;
            vm.NotifyOfPropertyChange("DoubleValue");
        }

        private void UIElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            CurrentConverter.DecimalCount = 4;
            dynamic vm = DataContext;
            vm.NotifyOfPropertyChange("DoubleValue");
        }
    }
}