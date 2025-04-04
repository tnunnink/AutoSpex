namespace AutoSpex.Engine.Tests;

[TestFixture]
public class RepoTests
{
    [Test]
    public void Configure_SomeDirectory_ShouldHaveExpectedValues()
    {
        var repo = Repo.Configure(@"C:\Path\To\TestRepo");

        repo.RepoId.Should().NotBeEmpty();
        repo.Location.Should().Be(@"C:\Path\To\TestRepo");
        repo.Name.Should().Be("TestRepo");
    }

    [Test]
    public void Build_TempDirectory_ShouldExists()
    {
        var directory = Directory.CreateTempSubdirectory("AutoSpexTest");
        var repo = Repo.Configure(directory.FullName);

        repo.Build();

        repo.Exists.Should().BeTrue();
        directory.Delete(true);
    }

    [Test]
    public void FindSources_FakeLocation_ShouldBeEmpty()
    {
        var repo = Repo.Configure(@"C:\Users\tnunnink\Documents\Fake");

        var sources = repo.FindSources().ToList();

        sources.Should().BeEmpty();
    }

    [Test]
    public void FindSources_ValidLocationWithNoSource_ShouldBeEmpty()
    {
        var repo = Repo.Configure(@"C:\Users\tnunnink\Documents\Empty");

        var sources = repo.FindSources().ToList();

        sources.Should().BeEmpty();
    }

    [Test]
    public void FindSources_ValidLocationWithSources_ShouldBeEmpty()
    {
        var repo = Repo.Configure(@"C:\Users\tnunnink\Documents\Rockwell");

        var sources = repo.FindSources().ToList();

        sources.Should().NotBeEmpty();
    }
}