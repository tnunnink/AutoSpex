using System;
using System.Threading.Tasks;
using ActiproSoftware.UI.Avalonia.Media;
using AutoSpex.Client.Services;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using JetBrains.Annotations;

namespace AutoSpex.Client;

[UsedImplicitly]
public sealed class App : Application, IDisposable, IAsyncDisposable
{
    private readonly Settings _settings;

    public App()
    {
        Registrar.Build();
        _settings = Registrar.Resolve<Settings>();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != RequestedThemeVariantProperty) return;
        var theme = (ThemeVariant)change.NewValue!;
        _settings.SaveValue(SettingKey.Theme, theme.ToString()).FireAndForget();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ImageProvider.Default.ChromaticAdaptationMode = ImageChromaticAdaptationMode.DarkThemes;

        _settings.GetTheme()
            .ContinueWith(t => Dispatcher.UIThread.Invoke(() => RequestedThemeVariant = t.Result))
            .FireAndForget(e => { Console.WriteLine(e.Message); });
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            desktop.MainWindow = Registrar.Resolve<Shell>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void Dispose()
    {
        Registrar.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await Registrar.DisposeAsync();
    }
}