namespace AutoSpex.Engine.Tests;

[TestFixture]
public class RepoTests
{
    [Test]
    public void Cache_WhenCalled_ShouldBeExpected()
    {
        var repo = Repo.Cache;

        repo.RepoId.Should().BeEmpty();
        repo.Location.Should().Be(@"..\cache");
        repo.Name.Should().Be("cache");
    }

    [Test]
    public void New_ValidLocation_ShouldBeExpected()
    {
        var repo = new Repo(@"C:\Users\tnunnink\Documents");

        repo.RepoId.Should().NotBeEmpty();
        repo.Location.Should().Be(@"C:\Users\tnunnink\Documents");
        repo.Name.Should().Be("Documents");
    }
    
    [Test]
    public void FindSources_FakeLocation_ShouldBeEmpty()
    {
        var repo = new Repo(@"C:\Users\tnunnink\Documents\Fake");

        var sources = repo.FindSources().ToList();

        sources.Should().BeEmpty();
    }

    [Test]
    public void FindSources_ValidLocationWithNoSource_ShouldBeEmpty()
    {
        var repo = new Repo(@"C:\Users\tnunnink\Documents\Empty");

        var sources = repo.FindSources().ToList();

        sources.Should().BeEmpty();
    }
    
    [Test]
    public void FindSources_ValidLocationWithSources_ShouldBeEmpty()
    {
        var repo = new Repo(@"C:\Users\tnunnink\Documents\Rockwell");

        var sources = repo.FindSources().ToList();

        sources.Should().NotBeEmpty();
    }
}