namespace AutoSpex.Engine.Tests.Elements;

[TestFixture]
public class TagElementTests
{
    [Test]
    public void Properties_WhenCalled_ShouldNotBeEmpty()
    {
        var element = Element.Tag;

        var properties = element.Properties.ToList();

        properties.Should().NotBeEmpty();
    }

    [Test]
    public void This_WhenCalled_ShouldNotBeNull()
    {
        var element = Element.Tag;

        var result = element.This;

        result.Should().NotBeNull();
    }

    [Test]
    public void This_WhenCalled_ShouldExpectedValues()
    {
        var element = Element.Tag;

        var result = element.This;

        result.Origin.Should().Be(typeof(Tag));
        result.Type.Should().Be(typeof(Tag));
        result.Name.Should().Be("This");
        result.Path.Should().Be("This");
        result.Group.Should().Be(TypeGroup.Element);
        result.DisplayName.Should().Be("Tag");
        result.Properties.Should().NotBeEmpty();
    }

    [Test]
    public void IsComponent_WhenCalled_ShouldBeTrue()
    {
        var element = Element.Tag;

        var result = element.IsComponent;

        result.Should().BeTrue();
    }

    [Test]
    public void Property_SimpleProperty_ShouldBeExpected()
    {
        var element = Element.Tag;

        var property = element.Property("Name");

        property.Origin.Should().Be(typeof(Tag));
        property.Type.Should().Be(typeof(string));
        property.Name.Should().Be("Name");
        property.Path.Should().Be("Name");
        property.Group.Should().Be(TypeGroup.Text);
        property.DisplayName.Should().Be("string");
        property.Properties.Should().BeEmpty();
    }

    [Test]
    public void Property_NestedEnumProperty_ShouldBeExpected()
    {
        var element = Element.Tag;

        var property = element.Property("Radix.Name");

        property.Origin.Should().Be(typeof(Tag));
        property.Type.Should().Be(typeof(string));
        property.Name.Should().Be("Name");
        property.Path.Should().Be("Radix.Name");
        property.Group.Should().Be(TypeGroup.Text);
        property.DisplayName.Should().Be("string");
        property.Properties.Should().BeEmpty();
    }

    [Test]
    public void Property_NestedElementProperty_ShouldBeExpected()
    {
        var element = Element.Tag;

        var property = element.Property("Root.Root.Parent");

        property.Origin.Should().Be(typeof(Tag));
        property.Type.Should().Be(typeof(Tag));
        property.Name.Should().Be("Parent");
        property.Path.Should().Be("Root.Root.Parent");
        property.Group.Should().Be(TypeGroup.Element);
        property.DisplayName.Should().Be("Tag");
        property.Properties.Should().NotBeEmpty();
    }

    [Test]
    public void Property_References_ShouldBeExpected()
    {
        var element = Element.Tag;

        var property = element.Property("References");

        property.Origin.Should().Be(typeof(Tag));
        property.Type.Should().Be(typeof(List<CrossReference>));
        property.Name.Should().Be("References");
        property.Path.Should().Be("References");
        property.Group.Should().Be(TypeGroup.Collection);
        property.DisplayName.Should().Be("CrossReference[]");
    }

    [Test]
    public void GetValue_References_ShouldBeExpected()
    {
        var content = L5X.Load(Known.Example, L5XOptions.Index);
        var tag = content.Get<Tag>("/Air_Supply_01/Tag/AirCompressor");
        var element = Element.Tag;
        var property = element.Property("References.Count");

        var references = tag.References().ToList();
        references.Should().NotBeEmpty();

        var count = property.GetValue(tag);
        count.Should().Be(references.Count);
    }

    [Test]
    public void GetValue_ReferencesOfManyTags_ShouldBeExpected()
    {
        var content = L5X.Load(Known.Example, L5XOptions.Index);
        var tags = content.Query<Tag>().Where(t => t.Scope.Program == "Air_Supply_01");
        var element = Element.Tag;
        var property = element.Property("References.Count");

        var counts = tags.Select(t => new { t.TagName, References = property.GetValue(t) });
        counts.Should().AllSatisfy(x => x.References.As<int>().Should().BeGreaterThan(0));
    }
}