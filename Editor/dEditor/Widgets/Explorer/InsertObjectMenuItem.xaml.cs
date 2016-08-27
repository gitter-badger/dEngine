using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using dEditor.Framework.Services;
using dEditor.Widgets.AdvancedObjects;
using dEngine;
using dEngine.Instances.Attributes;
using dEngine.Serializer.V1;
using dEngine.Services;
using MoreLinq;

namespace dEditor.Widgets.Explorer
{
    /// <summary>
    /// Interaction logic for InsertObjectMenuItem.xaml
    /// </summary>
    public partial class InsertObjectMenuItem
    {
        public InsertObjectMenuItem()
        {
            DataContext = this;

            ObjectItems = Inst.TypeDictionary.Values.Where(t =>
                !t.IsAbstract && t.IsPublic && t.GetCustomAttribute<UncreatableAttribute>() == null &&
                !typeof(Service).IsAssignableFrom(t.Type)).Select(t => new ObjectItem(t.Type));

            InitializeComponent();

            ObjectItems.ForEach(x => Items.Add(new MenuItem()
            {
                Header = x.Name,
                Icon = new Image {Source = new BitmapImage(x.Icon) },
            }));
        }

        public IEnumerable<ObjectItem> ObjectItems { get; }

        public class ObjectItem
        {
            public string Name { get; }
            public Uri Icon { get; }

            public ObjectItem(Type type)
            {
                Name = type.Name;
                Icon = IconProvider.GetIconUri(type);
            }
        }
    }
}
