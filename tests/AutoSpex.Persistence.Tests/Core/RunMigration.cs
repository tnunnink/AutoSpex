namespace AutoSpex.Persistence.Tests.Core;

[TestFixture]
public class RunMigration
{
    [Test]
    public void RunMigrationTest()
    {
        var manager = ConnectionManager.Default;
        manager.Register(Database.Project, "persistence.db");

        var result = manager.Migrate(Database.Project);

        result.IsSuccess.Should().BeTrue();
    }
}