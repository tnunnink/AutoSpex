﻿using AutoSpex.Client.Features.Nodes;
using FluentAssertions;

namespace AutoSpex.Client.Tests.Nodes;

[TestFixture]
public class NodeTests
{
    [Test]
    public void New_Default_ShouldNotBeNull()
    {
        var node = new Node();

        node.Should().NotBeNull();
    }

    [Test]
    public void SpecCollection_ValidParameter_ShouldHaveExpectedValues()
    {
        var node = Node.SpecCollection("Test");

        node.NodeId.Should().NotBeEmpty();
        node.ParentId.Should().BeEmpty();
        node.Parent.Should().BeNull();
        node.Feature.Should().Be(Feature.Specifications);
        node.NodeType.Should().Be(NodeType.Collection);
        node.Name.Should().Be("Test");
        node.Depth.Should().Be(0);
        node.Ordinal.Should().Be(0);
    }

    [Test]
    public void Folder_ValidParameters_ShouldHaveExpectedValues()
    {
        var parent = Node.SpecCollection("Test");
        
        var node = parent.NewFolder("Test");
        
        node.NodeId.Should().NotBeEmpty();
        node.ParentId.Should().NotBeEmpty();
        node.Parent.Should().NotBeNull();
        node.Parent?.Name.Should().Be("Test");
        node.Feature.Should().Be(Feature.Specifications);
        node.NodeType.Should().Be(NodeType.Folder);
        node.Name.Should().Be("Test");
        node.Depth.Should().Be(1);
        node.Ordinal.Should().Be(0);
    }

    [Test]
    public void AddSpec_ValidName_ShouldHaveExpectedValues()
    {
        var collection = Node.SpecCollection("Test");

        var node = collection.NewSpec("MySpec");
        
        node.NodeId.Should().NotBeEmpty();
        node.ParentId.Should().NotBeEmpty();
        node.Parent.Should().Be(collection);
        node.Feature.Should().Be(Feature.Specifications);
        node.NodeType.Should().Be(NodeType.Spec);
        node.Name.Should().Be("MySpec");
        node.Depth.Should().Be(1);
        node.Ordinal.Should().Be(0);
    }
}