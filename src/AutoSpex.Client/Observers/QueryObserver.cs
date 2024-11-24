using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Observers;

public partial class QueryObserver : StepObserver<Query>
{
    public QueryObserver(Query model) : base(model)
    {
        Track(nameof(Match));
    }

    public Element Element
    {
        get => Model.Element;
        set => SetProperty(Model.Element, value, Model, (s, v) => s.Element = v);
    }

    /// <summary>
    /// Updates the specification query element type and resets all the criteria. We may not do this in the future
    /// if I can get better error handling for the criterion observer.
    /// </summary>
    [RelayCommand]
    private void UpdateElement(Element? element)
    {
        if (element is null) return;
        Element = element;
    }
}