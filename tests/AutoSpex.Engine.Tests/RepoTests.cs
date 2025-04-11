using System.Diagnostics;
using Task = System.Threading.Tasks.Task;

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
    public void Configure_EndsInSlash_ShouldHaveExpectedValues()
    {
        var repo = Repo.Configure(@"C:\Path\To\TestRepo\");

        repo.RepoId.Should().NotBeEmpty();
        repo.Location.Should().Be(@"C:\Path\To\TestRepo");
        repo.Name.Should().Be("TestRepo");
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
    
    [Test]
    public void FindSources_WhenCalled_ShouldYieldReturn()
    {
        var repo = Repo.Configure(@"C:\Users\Public");

        var stopWatch = Stopwatch.StartNew();
        var sources = repo.FindSources().ToList();
        stopWatch.Stop();

        sources.Should().NotBeEmpty();
        Console.WriteLine(stopWatch.ElapsedMilliseconds);
    }

    /*[Test]
    public async Task FindSourcesAsync_WhenCalled_ShouldYieldReturn()
    {
        var repo = Repo.Configure(@"C:\Users\Public");

        var stopWatch = Stopwatch.StartNew();
        
        await foreach (var source in repo.FindSourcesAsync())
        {
            Console.WriteLine(source.Location);
            source.Should().NotBeNull();
        }
        
        stopWatch.Stop();
        Console.WriteLine(stopWatch.ElapsedMilliseconds);
    }*/
}