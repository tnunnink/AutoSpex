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
        
        Container.Build();

        TestProjectUri = new Uri(Path.Combine(Directory.GetCurrentDirectory(), ProjectName));
    }

    public readonly Uri TestProjectUri;

    public const string TestL5X = @"C:\Users\admin\Documents\L5X\Example.L5X";
    public static T Resolve<T>() where T : class => Container.Resolve<T>();

    public void Dispose()
    {
        Container.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        /*var path = Path.Combine(Directory.GetCurrentDirectory(), _configuration["AppDatabase"]!);

        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(TestProjectUri.LocalPath)) File.Delete(TestProjectUri.LocalPath);*/
    }
}