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
        source.TargetName.Should().Be(file.Info.TargetName);
        source.TargetType.Should().Be(file.Info.TargetType);
        source.ExportedBy.Should().Be(file.Info.Owner);
        source.ExportedOn.Should().Be(file.Info.ExportDate);
        source.Content.Should().NotBeEmpty();
        source.L5X.Should().NotBeNull();
        source.L5X.Serialize().Attribute("SourceId")?.Value.Should().Be(source.SourceId.ToString());
        source.L5X.Serialize().Descendants(L5XName.Data)
            .Where(e => e.Attribute(L5XName.Format)?.Value == DataFormat.L5K).Should().BeEmpty();
    }

    [Test]
    public void UpdateSource_WhenCalled_ShouldUpdateProperties()
    {
        var test = L5X.Load(Known.Test);
        var source = new Source(test);

        var example = L5X.Load(Known.Example);
        source.Update(example, true);

        source.TargetName.Should().Be(example.Info.TargetName);
        source.TargetType.Should().Be(example.Info.TargetType);
        source.ExportedBy.Should().Be(example.Info.Owner);
        source.ExportedOn.Should().Be(example.Info.ExportDate);
        source.Content.Should().NotBeEmpty();
        source.L5X.Should().NotBeNull();
        source.L5X.Serialize().Attribute("SourceId")?.Value.Should().Be(source.SourceId.ToString());
        source.L5X.Serialize().Descendants(L5XName.Data)
            .Where(e => e.Attribute(L5XName.Format)?.Value == DataFormat.L5K).Should().BeEmpty();
    }

    [Test]
    public void SaveScrubbedSourceToSeeSizeDifferenceOnDisc()
    {
        var test = L5X.Load(Known.Test);
        
        var source = new Source(test);

        source.L5X.Save(@"C:\Users\tnunn\Documents\L5X\Test_Scrubbed.L5X");
    }
}