using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages;

public partial class SettingsGeneralPageModel() : PageViewModel("General")
{
    public override string Route => "Settings/General";

    [ObservableProperty] private bool _alwaysDiscardChanges;

    public override async Task Load()
    {
        AlwaysDiscardChanges = await Settings.GetValue<bool>(SettingKey.AlwaysDiscardChanges);
    }

    partial void OnAlwaysDiscardChangesChanged(bool value)
    {
        Settings
            .SaveValue(SettingKey.AlwaysDiscardChanges, value)
            .FireAndForget(e => Notifier.ShowError("Failed to save settings", $"{e.Message}"));
    }
}