using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class PackageTests
{
    [Test]
    public Task ShouldBeVerifiedWhenSerialized()
    {
        var collection = Node.NewCollection();
        collection.AddSpec("First", s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.Like, "someName");
            s.Validate("Value", Operation.EqualTo, 123);
        });
        collection.AddSpec("Second", s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.Containing, "anotherName");
            s.Validate("Value", Operation.GreaterThan, 456);
        });
        collection.AddSpec("Third", s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.EqualTo, "yetAnotherName");
            s.Validate("Value", Negation.Not, Operation.EqualTo, 678);
        });

        var package = new Package(collection, 10000);

        var json = JsonSerializer.Serialize(package);

        return VerifyJson(json);
    }

    [Test]
    public void ShouldBeEquivalentWhenDeserialized()
    {
        var collection = Node.NewCollection();
        collection.AddSpec("First", s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.Like, "someName");
            s.Validate("Value", Operation.EqualTo, 123);
        });
        collection.AddSpec("Second", s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.Containing, "anotherName");
            s.Validate("Value", Operation.GreaterThan, 456);
        });
        collection.AddSpec("Third", s =>
        {
            s.Get(Element.Tag);
            s.Where("Name", Operation.EqualTo, "yetAnotherName");
            s.Validate("Value", Negation.Not, Operation.EqualTo, 678);
        });

        var package = new Package(collection, 10000);
        var json = JsonSerializer.Serialize(package);

        var result = JsonSerializer.Deserialize<Package>(json, new JsonSerializerOptions { IncludeFields = true });

        result.Should().BeEquivalentTo(package);
    }
}