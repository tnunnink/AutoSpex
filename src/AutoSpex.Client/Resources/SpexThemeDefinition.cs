using ActiproSoftware.UI.Avalonia.Themes.Generation;

namespace AutoSpex.Client.Resources;

public class SpexThemeDefinition : ThemeDefinition
{
    public SpexThemeDefinition()
    {
        AccentColorRampName = Hue.Sky.ToString();
        SuccessColorRampName = Hue.Emerald.ToString();
        WarningColorRampName = Hue.Amber.ToString();
    }
}