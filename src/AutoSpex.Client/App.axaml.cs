using System;
using System.Threading.Tasks;
using ActiproSoftware.UI.Avalonia.Media;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using JetBrains.Annotations;

namespace AutoSpex.Client;

[UsedImplicitly]
public sealed class App : Application, IDisposable, IAsyncDisposable
{
    public App()
    {
        Container.Build();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != RequestedThemeVariantProperty) return;
        var theme = (ThemeVariant)change.NewValue!;
        Settings.App.Save(s => s.Theme = theme);
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ImageProvider.Default.ChromaticAdaptationMode = ImageChromaticAdaptationMode.DarkThemes;
        RequestedThemeVariant = Settings.App.Theme;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            desktop.MainWindow = Container.Resolve<Shell>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void Dispose()
    {
        Container.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}