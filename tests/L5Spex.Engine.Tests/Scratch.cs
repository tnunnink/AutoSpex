namespace L5Spex.Engine.Tests;

[TestFixture]
public class Scratch
{
    [Test]
    public void Testing()
    {
        var uri = new Uri("Project Name/Some Folder/Sub folder/My Specification Name");

        uri.Should().NotBeNull();
    }
}