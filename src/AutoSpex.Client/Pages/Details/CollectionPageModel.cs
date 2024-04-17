using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Observers;

namespace AutoSpex.Client.Pages;

public class CollectionPageModel(NodeObserver node) : NodePageModel(node)
{
    public IEnumerable<NodeObserver> Specs => Node.Model.Specs().Select(s => new NodeObserver(s));
}