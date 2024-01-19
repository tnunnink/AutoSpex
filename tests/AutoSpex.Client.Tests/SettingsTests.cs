using Avalonia.Styling;
using FluentAssertions;

namespace AutoSpex.Client.Tests;

[TestFixture]
public class SettingsTests
{
    [TearDown]
    public void TearDown()
    {
        File.Delete("settings.db");
    }
    
    [Test]
    public void Theme_Default_ShouldBeEmpty()
    {
        var setting = Settings.App.Theme;

        setting.Key.Should().Be(ThemeVariant.Light.Key);
    }

    [Test]
    public void ShellWidth_Default_ShouldBeExpected()
    {
        var setting = Settings.App.ShellWidth;
        
        setting.Should().Be(1400);
    }
}