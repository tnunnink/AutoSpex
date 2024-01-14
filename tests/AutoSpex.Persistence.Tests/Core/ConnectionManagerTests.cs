namespace AutoSpex.Persistence.Tests.Core;

[TestFixture]
public class ConnectionManagerTests
{
    [Test]
    public void AppDatabaseShouldBeRegisteredByDefault()
    {
        var manager = ConnectionManager.Default;

        var result = manager.IsRegistered(Database.App);

        result.Should().BeTrue();
    }
    
    [Test]
    public void Register_AppDatabase_ShouldReturnSuccess()
    {
        var manager = ConnectionManager.Default;
        
        manager.Register(Database.App, "test.db");

        var registered = manager.IsRegistered(Database.App);
        registered.Should().BeTrue();
    }
    
    [Test]
    public void Register_ProjectDatabase_ShouldReturnSuccess()
    {
        var manager = ConnectionManager.Default;
        
        manager.Register(Database.Project, "test.db");

        var registered = manager.IsRegistered(Database.Project);
        registered.Should().BeTrue();
    }

    [Test]
    public void Migrate_AppDatabase_ShouldReturnSuccess()
    {
        var manager = ConnectionManager.Default;

        var result = manager.Migrate(Database.App);

        result.IsSuccess.Should().BeTrue();

        var source = manager.GetSource(Database.App);
        File.Delete(source);
    }
    
    [Test]
    public void Migrate_ProjectDatabase_ShouldReturnSuccess()
    {
        var manager = ConnectionManager.Default;
        manager.Register(Database.Project, "test.db");

        var result = manager.Migrate(Database.Project);

        result.IsSuccess.Should().BeTrue();
        
        var source = manager.GetSource(Database.Project);
        File.Delete(source);
    }
    
    [Test]
    public void Migrate_ProjectDatabaseAsSpexExtension_ShouldReturnSuccess()
    {
        var manager = ConnectionManager.Default;
        manager.Register(Database.Project, "test.spex");

        var result = manager.Migrate(Database.Project);

        result.IsSuccess.Should().BeTrue();
        
        var source = manager.GetSource(Database.Project);
        File.Delete(source);
    }

    [Test]
    public async Task Connect_AppDatabase_ShouldReturnConnection()
    {
        var manager = ConnectionManager.Default;

        using var connection = await manager.Connect(Database.App, CancellationToken.None);

        connection.Should().NotBeNull();
        
        connection.Dispose();
        
        var source = manager.GetSource(Database.App);
        File.Delete(source);
    }
    
    [Test]
    public async Task Connect_ProjectDatabase_ShouldReturnConnection()
    {
        var manager = ConnectionManager.Default;
        manager.Register(Database.Project, "test.db");

        using var connection = await manager.Connect(Database.Project, CancellationToken.None);

        connection.Should().NotBeNull();
        
        connection.Dispose();
        
        var source = manager.GetSource(Database.Project);
        File.Delete(source);
    }
}