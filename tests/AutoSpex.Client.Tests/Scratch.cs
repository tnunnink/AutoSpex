using ActiproSoftware.Extensions;
using FluentAssertions;
using L5Sharp.Core;

namespace AutoSpex.Client.Tests;

[TestFixture]
public class Scratch
{
    [Test]
    public void ScratchTest()
    {
    }

    [Test]
    public void CreateTypeFromQualifiedName()
    {
        var example = typeof(Tag);
        var qualified = example.AssemblyQualifiedName!;
        var assembly = example.Assembly.FullName;
        var version = example.Assembly.GetFileVersion();
        var nameSpace = example.Namespace;
        var fullName = example.FullName;
        var typeName = example.Name;
        
        var type = Type.GetType(qualified);
        
        type.Should().NotBeNull();
    }
}