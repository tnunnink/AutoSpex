using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using FluentAssertions;

namespace AutoSpex.Client.Tests.Projects;

[TestFixture]
public class ProjectMigratorTests
{
    [Test]
    public async Task Migrate_ValidPath_ShouldReturnSuccess()
    {
        using var context = new TestContext();
        var migrator = context.Resolve<IProjectMigrator>();

        var result = await migrator.Migrate(context.ProjectPath);

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public void Evaluate_AgainstCurrentVersion_ShouldReturnNoActionRequired()
    {
        using var context = new TestContext();
        context.BuildProject();
        var migrator = context.Resolve<IProjectMigrator>();

        var result = migrator.Evaluate(context.ProjectPath);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(ProjectAction.NoActionRequired);
    }
    
    [Test]
    public void Evaluate_AgainstLowerDatabaseVersion_ShouldReturnMigrationRequired()
    {
        using var context = new TestContext();
        context.BuildProject(10000);
        var migrator = context.Resolve<IProjectMigrator>();

        var result = migrator.Evaluate(context.ProjectPath);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(ProjectAction.MigrationRequired);
    }
    
    [Test]
    public void Evaluate_AgainstHigherDatabaseMajorVersion_ShouldReturnUpdateRequired()
    {
        using var context = new TestContext();
        context.BuildProject();
        context.RunMigration("MockMajorVersionUpgrade");
        var migrator = context.Resolve<IProjectMigrator>();

        var result = migrator.Evaluate(context.ProjectPath);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(ProjectAction.UpdateRequired);
    }
}