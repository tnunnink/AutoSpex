using System;
using Avalonia;
using Avalonia.Logging;
using Avalonia.Svg.Skia;
using JetBrains.Annotations;

namespace AutoSpex.Client;

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
    private static AppBuilder BuildAvaloniaApp()
    {
        GC.KeepAlive(typeof(SvgImageExtension).Assembly);
        GC.KeepAlive(typeof(Avalonia.Svg.Skia.Svg).Assembly);
        
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace(LogEventLevel.Verbose);
    }
}