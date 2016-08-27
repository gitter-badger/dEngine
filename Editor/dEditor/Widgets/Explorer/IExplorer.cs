using dEngine.Instances;

namespace dEditor.Widgets.Explorer
{
	public interface IExplorer
	{
	    Instance LastClickedInstance { get; set; }
	    ExplorerItem RootItem { get; set; }
	}
}