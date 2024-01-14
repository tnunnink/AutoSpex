namespace AutoSpex.Engine.Tests;

[TestFixture]
public class SourceTests
{
    [Test]
    public void New_ValidFile_ShouldBeExpected()
    {
        var file = L5X.Load(Known.Test);
        var source = new Source(file);

        source.Should().NotBeNull();
        source.SourceId.Should().NotBeEmpty();
        source.Name.Should().Be(file.Info.TargetName);
        source.Description.Should().Be(file.Controller.Description);
        source.IsSelected.Should().BeFalse();
        source.TargetName.Should().Be(file.Info.TargetName);
        source.TargetType.Should().Be(file.Info.TargetType);
        source.ExportedBy.Should().Be(file.Info.Owner);
        source.ExportedOn.Should().Be(file.Info.ExportDate);
        
        source.Content.Should().NotBeEmpty();
        source.L5X.Should().NotBeNull();
    }

    [Test]
    public void UpdateSource_WhenCalled_ShouldUpdateProperties()
    {
        var test = L5X.Load(Known.Test);
        var source = new Source(test);

        var example = L5X.Load(Known.Example);
        source.Update(example);

        source.TargetName.Should().Be(example.Info.TargetName);
        source.TargetType.Should().Be(example.Info.TargetType);
        source.ExportedBy.Should().Be(example.Info.Owner);
        source.ExportedOn.Should().Be(example.Info.ExportDate);
    }
}