using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages;

public partial class SettingsGeneralPageModel() : PageViewModel("General")
{
    public override string Route => "Settings/General";

    [ObservableProperty] private bool _alwaysDiscardChanges;

    public override Task Load()
    {
        AlwaysDiscardChanges = Settings.App.AlwaysDiscardChanges;

        return Task.CompletedTask;
    }

    partial void OnAlwaysDiscardChangesChanged(bool value)
    {
        Settings.App.Save(s => s.AlwaysDiscardChanges = value);
    }
}