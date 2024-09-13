using System.Collections.Specialized;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class OverridesPageModel : PageViewModel
{
    /// <inheritdoc/>
    public OverridesPageModel(EnvironmentObserver environment) : base("Overrides")
    {
        Environment = environment;
        Environment.Sources.CollectionChanged += SourcesOnCollectionChanged;
        _selectedSource = environment.Sources.FirstOrDefault();
    }

    public override string Route => $"{nameof(Environment)}/{Environment.Id}/{Title}";
    public EnvironmentObserver Environment { get; }

    [ObservableProperty] private SourceObserver? _selectedSource;

    /// <inheritdoc />
    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        Environment.Sources.CollectionChanged -= SourcesOnCollectionChanged;
    }

    /// <summary>
    /// Update the selected source to the provided observer.
    /// </summary>
    [RelayCommand]
    private void SelectSource(SourceObserver? source)
    {
        if (source is null) return;
        SelectedSource = source;
    }

    /// <summary>
    /// Command to configure a new override by letting the user select an existing variable and then adding it to
    /// the selected source override collection.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddOverride))]
    private async Task AddOverride()
    {
        if (SelectedSource is null) return;

        var variable = await Prompter.Show<VariableObserver?>(() => new SelectVariablePageModel());
        if (variable is null) return;

        if (SelectedSource.Overrides.Contains(variable)) return;
        SelectedSource.Overrides.Add(variable);
    }

    /// <summary>
    /// Can only add overrides when there is a source selected.
    /// </summary>
    private bool CanAddOverride() => SelectedSource is not null;

    /// <summary>
    /// When the sources change we need to update the selected source if null.
    /// </summary>
    private void SourcesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SelectedSource ??= Environment.Sources.FirstOrDefault();
    }
}