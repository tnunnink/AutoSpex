using Avalonia;
using System;
using JetBrains.Annotations;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Projektanker.Icons.Avalonia.MaterialDesign;

namespace L5Spex.Client;

[UsedImplicitly]
public static class Program
{
    /// <summary>
    /// Provides the main entry point of the application.
    /// </summary>
    /// <param name="args">The string arguments.</param>
    [STAThread]
    public static void Main(string[] args)
        => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    /// <summary>
    /// Creates an Avalonia application builder.
    /// </summary>
    /// <returns>The <see cref="AppBuilder"/> object that was created.</returns>
    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register<FontAwesomeIconProvider>()
            .Register<MaterialDesignIconProvider>();
        
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    }
}