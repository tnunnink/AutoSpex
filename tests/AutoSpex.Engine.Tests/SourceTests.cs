using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class SourceTests
{
    [Test]
    public void New_ValidSourcePath_ShouldBeExpected()
    {
        var source = Source.Create(Known.Test);

        source.Hash.Should().NotBeEmpty();
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
}