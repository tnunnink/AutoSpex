using System.Data.SQLite;
using Microsoft.Extensions.Configuration;

namespace AutoSpex.Client.Tests;

public sealed class TestContext : IDisposable
{
    private const string ProjectName = "test.spex";

    public TestContext()
    {
        Container.Build();
        ProjectPath = new Uri(Path.Combine(Directory.GetCurrentDirectory(), ProjectName));
        ProjectConnection = new SQLiteConnectionStringBuilder {DataSource = ProjectPath.AbsolutePath}.ConnectionString;
        
    }

    public readonly Uri ProjectPath;

    public readonly string ProjectConnection;

    public const string TestL5X = @"C:\Users\admin\Documents\L5X\Example.L5X";
    public static T Resolve<T>() => Container.Resolve<T>();

    public void Dispose()
    {
        Container.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        /*var path = Path.Combine(Directory.GetCurrentDirectory(), _configuration["AppDatabase"]!);
        
        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(ProjectPath.LocalPath)) File.Delete(ProjectPath.LocalPath);*/
    }
}