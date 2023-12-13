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

        Parser.Parse(typeof(ExternalAccess), "Read/Write");
    }

    [Test]
    public void METHOD()
    {
        var task = new Task();
    }
}