using ActiproSoftware.UI.Avalonia.Themes.Generation;

namespace AutoSpex.Client.Resources;

public class AppThemeDefinition : ThemeDefinition
{
    public AppThemeDefinition()
    {
        AccentColorRampName = nameof(Hue.Sky);
        SuccessColorRampName = nameof(Hue.Emerald);
        WarningColorRampName = nameof(Hue.Amber);
    }
}