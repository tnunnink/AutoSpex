namespace AutoSpex.Persistence.Tests;

[TestFixture]
public class ConnectionManagerTests
{
    [Test]
    public async Task Connect_AppDatabase_ShouldReturnConnection()
    {
        using var context = new TestContext();
        var manager = context.Resolve<IConnectionManager>();

        using var connection = await manager.Connect(CancellationToken.None);

        connection.Should().NotBeNull();
    }
}