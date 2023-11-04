using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using JetBrains.Annotations;
using L5Spex.Client.Common;
using Lamar;
using Microsoft.Extensions.Configuration;

namespace L5Spex.Client;

[UsedImplicitly]
public class App : Application
{
    public readonly IContainer Container;

    public  AppSettings Settings;

    public App()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        var configuration = builder.Build();
        Settings = new AppSettings();
        /*var section = configuration.GetSection("AppSettings");*/
        Container = Bootstrapper.Build(configuration);
    }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        RequestedThemeVariant = ThemeVariant.Light;
    }
    
    public static App Instance => (App)Current!;

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            
            desktop.MainWindow = Container.GetInstance<Window>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}