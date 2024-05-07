using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;

namespace AutoSpex.Client.Pages;

public class FolderPageModel(NodeObserver node) : NodePageModel(node)
{
    public ObservableCollection<NodeObserver> Specs => new(Node.Model.Specs().Select(s => new NodeObserver(s)));
}