using ActiproSoftware.UI.Avalonia.Themes.Generation;

namespace AutoSpex.Client.Resources;

public class AppThemeDefinition : ThemeDefinition
{
    public AppThemeDefinition()
    {
        AccentColorRampName = Hue.Sky.ToString();
        SuccessColorRampName = Hue.Emerald.ToString();
        WarningColorRampName = Hue.Amber.ToString();
    }
}