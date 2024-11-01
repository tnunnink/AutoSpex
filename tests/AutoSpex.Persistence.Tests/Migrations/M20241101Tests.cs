namespace AutoSpex.Persistence.Tests.Migrations;

[TestFixture]
public class M20241101Tests : MigrationTest
{
    [Test]
    public void MigrationShouldProperlyTransformSpecConfig()
    {
        Runner.MigrateUp(20241027);
        //seed data.
        
        Runner.MigrateUp(20241101);
        //validated transformation
    }
}