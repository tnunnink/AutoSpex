using AutoSpex.Client.Shared;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class SettingsPageModel() : DetailPageModel("Settings")
{
    public override string Route => "Settings";
    public override string Icon => Title;

    /// <inheritdoc />
    protected override async Task NavigateTabs()
    {
        await Navigator.Navigate(() => new SettingsGeneralPageModel());
        await Navigator.Navigate(() => new SettingsAboutPageModel());
    }
}