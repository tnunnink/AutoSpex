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
            s.Find(Element.Tag);
            s.Filter("Name", Operation.Like, "someName");
            s.Verify("Value", Operation.EqualTo, 123);
        });
        collection.AddSpec("Second", s =>
        {
            s.Find(Element.Tag);
            s.Filter("Name", Operation.Containing, "anotherName");
            s.Verify("Value", Operation.GreaterThan, 456);
        });
        collection.AddSpec("Third", s =>
        {
            s.Find(Element.Tag);
            s.Filter("Name", Operation.EqualTo, "yetAnotherName");
            s.Verify("Value", Negation.Not, Operation.EqualTo, 678);
            s.VerificationInclusion = Inclusion.Any;
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
            s.Find(Element.Tag);
            s.Filter("Name", Operation.Like, "someName");
            s.Verify("Value", Operation.EqualTo, 123);
        });
        collection.AddSpec("Second", s =>
        {
            s.Find(Element.Tag);
            s.Filter("Name", Operation.Containing, "anotherName");
            s.Verify("Value", Operation.GreaterThan, 456);
        });
        collection.AddSpec("Third", s =>
        {
            s.Find(Element.Tag);
            s.Filter("Name", Operation.EqualTo, "yetAnotherName");
            s.Verify("Value", Negation.Not, Operation.EqualTo, 678);
            s.VerificationInclusion = Inclusion.Any;
        });
        
        var package = new Package(collection, 10000);
        var json = JsonSerializer.Serialize(package);

        var result = JsonSerializer.Deserialize<Package>(json, new JsonSerializerOptions {IncludeFields = true});

        result.Should().BeEquivalentTo(package);
    }
}