using AutoSpex.Client.Features.Nodes;
using FluentAssertions;

namespace AutoSpex.Client.Tests.Models;

[TestFixture]
public class NodeTests
{
    [Test]
    public void New_ValidParameters_ShouldNotBeNull()
    {
        var node = new Node("Test", NodeType.Source);

        node.Should().NotBeNull();
    }
    
    [Test]
    public void New_ValidRecord_ShouldNotBeNull()
    {
        var node = new Node((dynamic)new
        {
            NodeId = Guid.NewGuid(),
            ParentId = (Guid) default,
            NodeType = NodeType.Source,
            Name = "MySource",
            Depth = 0,
            Ordinal = 0,
            Description = "this is a test of course."
        });

        node.Should().NotBeNull();
    }
}