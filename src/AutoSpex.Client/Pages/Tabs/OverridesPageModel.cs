using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class OverridesPageModel(EnvironmentObserver environment) : PageViewModel
{
    public override string Route => $"{nameof(Environment)}/{Environment.Id}/{Title}";
    public override string Title => "Overrides";
    public EnvironmentObserver Environment { get; } = environment;

    [ObservableProperty] private SourceObserver? _selectedSource = environment.Sources.FirstOrDefault();

    [RelayCommand]
    private void SelectSource(SourceObserver? source)
    {
        if (source is null) return;
        SelectedSource = source;
    }

    [RelayCommand(CanExecute = nameof(CanAddOverride))]
    private async Task AddOverride()
    {
        if (SelectedSource is null) return;

        var variable = await Prompter.Show<VariableObserver?>(() => new SelectVariablePageModel());
        if (variable is null) return;

        if (SelectedSource.Overrides.Contains(variable)) return;
        SelectedSource.Overrides.Add(variable);
    }

    private bool CanAddOverride() => SelectedSource is not null;
}