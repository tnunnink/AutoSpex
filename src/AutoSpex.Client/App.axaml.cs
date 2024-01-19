using System;
using System.Threading.Tasks;
using ActiproSoftware.UI.Avalonia.Controls;
using ActiproSoftware.UI.Avalonia.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using JetBrains.Annotations;
using Serilog;

namespace AutoSpex.Client;

[UsedImplicitly]
public sealed class App : Application, IDisposable, IAsyncDisposable
{
    public App()
    {
        /*var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        var config = builder.Build();*/
        Container.Build();

        PropertyChanged += (_, e) =>
        {
            if (e.Property != RequestedThemeVariantProperty) return;
            var theme = e.NewValue as ThemeVariant ?? throw new InvalidOperationException("Not a ThemeVariant");
            Settings.App.Save(s => s.Theme = theme);
        };
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ImageProvider.Default.ChromaticAdaptationMode = ImageChromaticAdaptationMode.DarkThemes;
        RequestedThemeVariant = Settings.App.Theme;
    }

    public static Window MainWindow =>
        ((IClassicDesktopStyleApplicationLifetime) Current!.ApplicationLifetime!).MainWindow!;

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            //Tells Actipro to show all prompts as an overlay by default
            UserPromptBuilder.DefaultDisplayMode = UserPromptDisplayMode.Overlay;

            desktop.MainWindow = Container.Resolve<Shell>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void Dispose()
    {
        Container.Dispose();
        Log.CloseAndFlush();
    }

    public async ValueTask DisposeAsync()
    {
        await Container.DisposeAsync();
        await Log.CloseAndFlushAsync();
    }
}