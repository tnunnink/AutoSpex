using System.Diagnostics;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class SourceTests
{
    [Test]
    public void New_ValidSourcePath_ShouldBeExpected()
    {
        var source = Source.Create(Known.Test);
        
        source.Location.Should().NotBeEmpty();
        source.Name.Should().Be("Test");
        source.Type.Should().Be(SourceType.Markup);
        source.UpdatedOn.Should().BeWithin(TimeSpan.FromDays(1000));
        source.Size.Should().BeGreaterThan(0);
    }

    [Test]
    public void New_FakeSourcePath_ShouldThrowException()
    {
        FluentActions.Invoking(() => Source.Create(Known.Fake)).Should().Throw<ArgumentException>();
    }

    [Test]
    public async Task OpenAsync_MarkupFile_ShouldNotBeNull()
    {
        var source = Source.Create(Known.Test);

        var content = await source.OpenAsync();

        content.Should().NotBeNull();
    }
    
    [Test]
    public async Task OpenAsync_ArchiveFile_ShouldNotBeNull()
    {
        var source = Source.Create(Known.Archive);

        var content = await source.OpenAsync();

        content.Should().NotBeNull();
    }

    [Test]
    public async Task OpenAsync_CompressedFile_ShouldNotBeNull()
    {
        var source = Source.Create(Known.Test);

        var content = await source.OpenAsync();

        content.Should().NotBeNull();
    }
    
    [Test]
    public void HashContent_ArchiveFile_ShouldReturnValueQuicklyAndNotContainInvalidPathChars()
    {
        var source = Source.Create(Known.Archive);
        
        var stopwatch = Stopwatch.StartNew();
        var hash = source.HashContent();
        stopwatch.Stop();

        Console.WriteLine($"Hash: {hash}");
        Console.WriteLine($"Time Taken: {stopwatch.ElapsedMilliseconds}ms");
        hash.Select(c => c).Should().AllSatisfy(x => Path.GetInvalidFileNameChars().Should().NotContain(x));
    }
    
    [Test]
    public void HashContent_MarkupFile_ShouldReturnValueQuicklyAndNotContainInvalidPathChars()
    {
        var source = Source.Create(Known.Test);
        
        var stopwatch = Stopwatch.StartNew();
        var hash = source.HashContent();
        stopwatch.Stop();

        Console.WriteLine($"Hash: {hash}");
        Console.WriteLine($"Time Taken: {stopwatch.ElapsedMilliseconds}ms");
        hash.Select(c => c).Should().AllSatisfy(x => Path.GetInvalidFileNameChars().Should().NotContain(x));
    }
    
    [Test]
    public void HashContent_CompressedFile_ShouldReturnValueQuicklyAndNotContainInvalidPathChars()
    {
        var source = Source.Create(Known.Compressed);
        
        var stopwatch = Stopwatch.StartNew();
        var hash = source.HashContent();
        stopwatch.Stop();

        Console.WriteLine($"Hash: {hash}");
        Console.WriteLine($"Time Taken: {stopwatch.ElapsedMilliseconds}ms");
        hash.Select(c => c).Should().AllSatisfy(x => Path.GetInvalidFileNameChars().Should().NotContain(x));
    }
}