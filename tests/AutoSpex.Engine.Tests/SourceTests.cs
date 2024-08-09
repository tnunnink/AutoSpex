namespace AutoSpex.Engine.Tests;

[TestFixture]
public class SourceTests
{
    [Test]
    public void New_ValidFile_ShouldBeExpected()
    {
        var file = new Uri(Known.Test);
        var source = new Source(file);

        source.Should().NotBeNull();
        source.SourceId.Should().NotBeEmpty();
        source.Uri.Should().Be(file);
        source.Name.Should().Be("Test");
        source.FileName.Should().Be("Test.xml");
        source.Directory.Should().NotBeEmpty();
        source.Exists.Should().BeTrue();
        source.Overrides.Should().BeEmpty();
    }

    [Test]
    public void LoadFile_WhenCalled_ShouldReturnNotNullL5X()
    {
        var file = new Uri(Known.Test);
        var source = new Source(file);

        var content = source.Load();

        content.Should().NotBeNull();
    }

    [Test]
    public void CreateWatcher_WhenCalled_ShouldReturnNotNull()
    {
        var file = new Uri(Known.Test);
        var source = new Source(file);

        var watcher = source.CreateWatcher();

        watcher.Should().NotBeNull();
    }
    
    [Test]
    public void AddOverride_ValidVariable_ShouldHaveExpectedCount()
    {
        var file = new Uri(Known.Test);
        var source = new Source(file);

        source.Add(new Variable("TestVar", 123));

        source.Overrides.Should().HaveCount(1);
    }

    [Test]
    public void AddOverride_Null_ShouldThrowException()
    {
        var file = new Uri(Known.Test);
        var source = new Source(file);

        FluentActions.Invoking(() => source.Add(null!)).Should().Throw<ArgumentNullException>();
    }
}