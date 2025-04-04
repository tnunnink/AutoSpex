using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Components;

public partial class SaveChangesPageModel(string? name = null) : PageViewModel
{
    [ObservableProperty] private string? _name = name;

    [ObservableProperty] private bool _discardChanges;

    public override async Task Load()
    {
        DiscardChanges = await Settings.GetValue<bool>(SettingKey.AlwaysDiscardChanges);
    }

    partial void OnDiscardChangesChanged(bool value)
    {
        Settings
            .SaveValue(SettingKey.AlwaysDiscardChanges, value)
            .FireAndForget(e => Notifier.ShowError("Failed to save settings", $"{e.Message}"));
    }
}