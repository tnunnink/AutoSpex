using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SelectVariablePageModel : PageViewModel
{
    public ObserverCollection<Variable, VariableObserver> Variables { get; private set; } = [];

    [ObservableProperty] private string? _filter;

    public override async Task Load()
    {
        var variables = await Mediator.Send(new ListVariables());

        Variables = new ObserverCollection<Variable, VariableObserver>(
            variables.ToList(),
            v => new VariableObserver(v));
    }

    partial void OnFilterChanged(string? value)
    {
        Variables.Filter(v => v.Filter(value) || v.Node?.Filter(value) is true);
    }
}