using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Pages;

public class PropertyTreePageModel : PageViewModel
{
    public PropertyTreePageModel(ElementObserver element)
    {
        Properties.BindReadOnly(element.GetProperties().ToList());
        RegisterDisposable(Properties);
    }

    public ObserverCollection<Property, PropertyObserver> Properties { get; } = [];

    protected override void FilterChanged(string? filter)
    {
        Properties.Filter(filter);
    }
}