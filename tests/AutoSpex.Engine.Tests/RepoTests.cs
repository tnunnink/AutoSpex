using System.Diagnostics;

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

    [Test]
    public void AddTarget_ValidPath_ShouldContainExpectedCount()
    {
        var repo = Repo.Configure(@"C:\Path\To\TestRepo\");
        var source = Source.Create(@"C:\Path\To\TestRepo\SourceFile.L5X");
        
        repo.AddTarget(source);

        repo.Targets.Should().HaveCount(1);
    }
    
    [Test]
    public void AddTarget_AlreadyContainsTarget_ShouldContainExpectedCount()
    {
        var repo = Repo.Configure(@"C:\Path\To\TestRepo\");
        var source = Source.Create(@"C:\Path\To\TestRepo\SourceFile.L5X");
        
        repo.AddTarget(source);
        repo.AddTarget(source);
        repo.AddTarget(source);

        repo.Targets.Should().HaveCount(1);
    }

    [Test]
    public void AddTarget_InvalidPath_ShouldThrowException()
    {
        var repo = Repo.Configure(@"C:\Path\To\TestRepo\");
        var source = Source.Create(@"C:\Path\To\AnotherRepo\SourceFile.L5X");
        
        var action = () => repo.AddTarget(source);

        action.Should().Throw<ArgumentException>();
    }
    
    [Test]
    public void AddTarget_Null_ShouldThrowException()
    {
        var repo = Repo.Configure(@"C:\Path\To\TestRepo\");
        
        var action = () => repo.AddTarget((Source)null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void RemoveTarget_ValidTarget_ShouldHaveExpectedCount()
    {
        var repo = Repo.Configure(@"C:\Path\To\TestRepo\");
        repo.AddTarget(Source.Create(@"C:\Path\To\TestRepo\File1.L5X"));
        repo.AddTarget(Source.Create(@"C:\Path\To\TestRepo\File2.L5X"));
        repo.AddTarget(Source.Create(@"C:\Path\To\TestRepo\File3.L5X"));
        
        repo.RemoveTarget(Source.Create(@"C:\Path\To\TestRepo\File2.L5X"));

        repo.Targets.Should().HaveCount(2);
    }

    [Test]
    public void ClearTargets_WhenCalled_ShouldHaveExpectedCount()
    {
        var repo = Repo.Configure(@"C:\Path\To\TestRepo\");
        repo.AddTarget(Source.Create(@"C:\Path\To\TestRepo\File1.L5X"));
        repo.AddTarget(Source.Create(@"C:\Path\To\TestRepo\File2.L5X"));
        repo.AddTarget(Source.Create(@"C:\Path\To\TestRepo\File3.L5X"));
        
        repo.ClearTargets();
        
        repo.Targets.Should().BeEmpty();
    }
}