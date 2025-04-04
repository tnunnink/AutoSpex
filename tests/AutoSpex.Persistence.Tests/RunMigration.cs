namespace AutoSpex.Persistence.Tests;

[TestFixture]
public class RunMigration
{
    [Test]
    public async Task RunSpexDatabaseMigration()
    {
        var manager = new ConnectionManager("app.db");

        var connection = await manager.Connect(CancellationToken.None);

        connection.Should().NotBeNull();
    }
}