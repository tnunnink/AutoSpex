using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class OverridesPageModel : PageViewModel
{
    private readonly SourceObserver _source;

    /// <inheritdoc/>
    public OverridesPageModel(SourceObserver source) : base("Overrides")
    {
        _source = source;

        Overrides = new ObserverCollection<Variable, VariableObserver>(
            refresh: () => _source.Model.Overrides.Select(v => new VariableObserver(v)).ToList(),
            add: (_, m) => _source.Model.AddOverride(m)
        );

        Track(Overrides);
    }

    public override string Route => $"{nameof(Source)}/{_source.Id}/{Title}";

    public ObserverCollection<Variable, VariableObserver> Overrides { get; }

    /// <summary>
    /// Command to configure a new override by letting the user select an existing variable and then adding it to
    /// the selected source override collection.
    /// </summary>
    [RelayCommand]
    private async Task AddOverride()
    {
        var variable = await Prompter.Show<VariableObserver?>(() => new SelectVariablePageModel());
        if (variable is null) return;

        if (Overrides.Contains(variable)) return;
        Overrides.Add(variable);
    }
}