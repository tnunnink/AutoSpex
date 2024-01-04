using Serialize.Linq.Factories;
using Serialize.Linq.Serializers;
using Task = L5Sharp.Core.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class Scratch
{
    [Test]
    public void Testing()
    {
        var uri = new Uri("Project Name/Some Folder/Sub folder/My Specification Name");

        uri.Should().NotBeNull();
    }

    [Test]
    public void TypeTests()
    {
        var type = Type.GetType("System.String");

        type.Should().Be(typeof(string));
    }
}