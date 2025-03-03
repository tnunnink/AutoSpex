namespace AutoSpex.Engine.Tests;

[TestFixture]
public class SourceTests
{
    [Test]
    public void New_DefaultOverload_ShouldBeExpected()
    {
        var source = new Source();

        source.Should().NotBeNull();
        source.SourceId.Should().NotBeEmpty();
        source.Name.Should().Be("New Source");
        source.IsTarget.Should().BeFalse();
        source.TargetName.Should().BeEmpty();
        source.TargetType.Should().BeEmpty();
        source.ExportedOn.Should().BeEmpty();
        source.ExportedBy.Should().BeEmpty();
        source.Content.Should().BeNull();
    }

    [Test]
    public void New_ValidFile_ShouldBeExpected()
    {
        var file = L5X.Load(Known.Test);
        var source = new Source(file);

        source.Should().NotBeNull();
        source.SourceId.Should().NotBeEmpty();
        source.Name.Should().Be("TestController");
        source.IsTarget.Should().BeFalse();
        source.TargetName.Should().Be("TestController");
        source.TargetType.Should().Be("Controller");
        source.ExportedOn.Should().NotBeEmpty();
        source.ExportedBy.Should().NotBeEmpty();
        source.Content.Should().NotBeNull();
    }

    [Test]
    public void Update_ValidContent_ShouldBeExpected()
    {
        var source = new Source();

        var file = L5X.Load(Known.Test);
        source.Update(file);

        source.Should().NotBeNull();
        source.SourceId.Should().NotBeEmpty();
        source.Name.Should().Be("New Source");
        source.IsTarget.Should().BeFalse();
        source.TargetName.Should().Be("TestController");
        source.TargetType.Should().Be("Controller");
        source.ExportedOn.Should().NotBeEmpty();
        source.ExportedBy.Should().NotBeEmpty();
        source.Content.Should().NotBeNull();
    }

    [Test]
    public void SourceIdShouldBeInjectedIntoContentFileUponCreation()
    {
        var source = new Source(L5X.Load(Known.Test));

        var content = source.Content;
        var sourceId = content?.Serialize().Attribute("SourceId")?.Value.Parse<Guid>();

        sourceId.Should().Be(source.SourceId);
    }

    [Test]
    public void SourceIdShouldBeInjectedIntoContentFileUponUpdate()
    {
        var source = new Source();
        source.Update(L5X.Load(Known.Test));

        var content = source.Content;
        var sourceId = content?.Serialize().Attribute("SourceId")?.Value.Parse<Guid>();

        sourceId.Should().Be(source.SourceId);
    }
}