using System.Data.SQLite;
using Avalonia;

namespace AutoSpex.Client.Tests;

public sealed class TestContext : IDisposable
{
    private const string ProjectName = "test.spex";

    public TestContext()
    {
        if (Application.Current is null)
        {
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .SetupWithoutStarting();
        }
        
        Registrar.Build();

        TestProjectUri = new Uri(Path.Combine(Directory.GetCurrentDirectory(), ProjectName));
    }

    public readonly Uri TestProjectUri;

    public const string TestL5X = @"C:\Users\tnunn\Documents\L5X\Example.L5X";
    public static T Resolve<T>() where T : class => Registrar.Resolve<T>();

    public void Dispose()
    {
        Registrar.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        /*var path = Path.Combine(Directory.GetCurrentDirectory(), _configuration["AppDatabase"]!);

        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(TestProjectUri.LocalPath)) File.Delete(TestProjectUri.LocalPath);*/
    }
}