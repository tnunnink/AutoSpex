using System.Reflection;
using ActiproSoftware.Extensions;
using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Features.Sources;
using FluentAssertions;
using L5Sharp.Core;

namespace AutoSpex.Client.Tests;

[TestFixture]
public class Scratch
{
    [Test]
    public void ScratchTest()
    {
        var test = new SourceViewModel(new Node(new
        {
            NodeId = Guid.NewGuid(),
            ParentId = (Guid) default,
            NodeType = NodeType.Source,
            Name = "MySource",
            Depth = 0, 
            Ordinal = 0,
            Description = "this is a test of course."
        }));


        test.Should().NotBeNull();
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