using FluentAssertions;
using L5Sharp.Core;

namespace AutoSpex.Client.Tests;

[TestFixture]
public class Scratch
{
    [Test]
    public void ScratchTest()
    {
        var uri = new Uri($"//MyRootName/SubPathPerhaps/{Guid.NewGuid()}");

        uri.Should().NotBeNull();
    }

    [Test]
    public void METHOD()
    {
        var test = L5X.Load(@"C:\Users\admin\Documents\L5X\Test.L5X");
        test.Should().NotBeNull();
    }
}