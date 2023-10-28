namespace L5Spex.Persistence.Tests;

[TestFixture]
public class MigratorTest
{
    private const string TestDatabase = @"C:\Projects\L5Spex\src\L5Spex\L5Spex.Persistence\Spex.db";

    [Test]
    public void RunAllMigrationsToCreateTestDatabaseForConnection()
    {
        File.Delete(TestDatabase);
        var provider = new SqliteMigrationProvider($"Data Source={TestDatabase};");
        provider.Migrate();
    }
}