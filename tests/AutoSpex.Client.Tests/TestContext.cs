using System.Data.SQLite;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoSpex.Client.Tests;

public sealed class TestContext : IDisposable
{
    private const string ProjectName = "test.spex";
    private readonly IConfigurationRoot _configuration;
    private readonly IContainer _container;

    public TestContext()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        _configuration = builder.Build();
        _container = Bootstrapper.Build(_configuration);

        
        ProjectPath = new Uri(Path.Combine(Directory.GetCurrentDirectory(), ProjectName));
        ProjectConnection = new SQLiteConnectionStringBuilder {DataSource = ProjectPath.AbsolutePath}.ConnectionString;
        
        var settings = _container.GetInstance<ISettingsManager>();
        settings.Save(Setting.OpenProjectConnection, ProjectConnection);
        settings.Save(Setting.OpenProjectPath, ProjectPath.AbsolutePath);
    }

    public readonly Uri ProjectPath;

    public readonly string ProjectConnection;

    public const string TestL5X = @"C:\Users\admin\Documents\L5X\Example.L5X";

    public T Resolve<T>() => _container.GetInstance<T>();

    public void BuildProject(long version = 0)
    {
        var registry = new ServiceRegistry();

        registry.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(ProjectConnection)
                .ScanIn(typeof(Bootstrapper).Assembly).For.Migrations())
            .Configure<RunnerOptions>(opt => { opt.Tags = new[] {"Project"}; });

        var container = new Container(registry);
        var runner = container.GetInstance<IMigrationRunner>();

        if (version > 0)
        {
            runner.MigrateUp(version);
        }
        else
        {
            runner.MigrateUp();
        }
        
        container.Dispose();
    }

    public void RunMigration(string tag)
    {
        var registry = new ServiceRegistry();

        registry.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(ProjectConnection)
                .ScanIn(typeof(TestContext).Assembly).For.Migrations())
            .Configure<RunnerOptions>(opt => { opt.Tags = new[] {tag}; });

        var container = new Container(registry);
        var runner = container.GetInstance<IMigrationRunner>();
        runner.MigrateUp();
        container.Dispose();
    }

    public void Dispose()
    {
        _container.Dispose();
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        var path = Path.Combine(Directory.GetCurrentDirectory(), _configuration["AppDatabase"]!);
        
        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(ProjectPath.AbsolutePath)) File.Delete(ProjectPath.AbsolutePath);
    }
}