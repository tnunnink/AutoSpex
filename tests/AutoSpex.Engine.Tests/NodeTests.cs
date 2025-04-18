﻿using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class NodeTests
{
    [Test]
    public void NewCollection_Default_ShouldHaveExpectedValues()
    {
        var node = Node.NewCollection();

        node.NodeId.Should().NotBeEmpty();
        node.ParentId.Should().BeEmpty();
        node.Parent.Should().BeNull();
        node.Type.Should().Be(NodeType.Collection);
        node.Name.Should().Be("New Collection");
        node.Description.Should().BeNull();
        node.Depth.Should().Be(0);
        node.Path.Should().Be(string.Empty);
        node.Nodes.Should().BeEmpty();
    }

    [Test]
    public void NewContainer_Default_ShouldHaveExpectedValues()
    {
        var node = Node.NewContainer();

        node.NodeId.Should().NotBeEmpty();
        node.ParentId.Should().BeEmpty();
        node.Parent.Should().BeNull();
        node.Type.Should().Be(NodeType.Container);
        node.Name.Should().Be("New Container");
        node.Description.Should().BeNull();
        node.Depth.Should().Be(0);
        node.Path.Should().Be(string.Empty);
        node.Nodes.Should().BeEmpty();
    }

    [Test]
    public void NewContainer_Name_ShouldHaveExpectedValues()
    {
        var node = Node.NewContainer("Test");

        node.Name.Should().Be("Test");
    }

    [Test]
    public void SetComment_AnyValue_ShouldUpdate()
    {
        var node = Node.NewCollection();

        node.Description = "This is the root collection";

        node.Description.Should().NotBeEmpty();
    }

    [Test]
    public void NewSpec_Default_ShouldHaveExpectedValues()
    {
        var node = Node.NewSpec("MySpec");

        node.NodeId.Should().NotBeEmpty();
        node.ParentId.Should().BeEmpty();
        node.Parent.Should().BeNull();
        node.Type.Should().Be(NodeType.Spec);
        node.Name.Should().Be("MySpec");
        node.Description.Should().BeNull();
    }

    [Test]
    public void AddContainer_ValidParameters_ShouldHaveExpectedValues()
    {
        var parent = Node.NewContainer("Test");

        var node = parent.AddContainer("Folder");

        node.NodeId.Should().NotBeEmpty();
        node.ParentId.Should().NotBeEmpty();
        node.Parent.Should().NotBeNull();
        node.Parent?.Name.Should().Be("Test");
        node.Type.Should().Be(NodeType.Container);
        node.Name.Should().Be("Folder");
        node.Depth.Should().Be(1);
    }

    [Test]
    public void AddContainer_ToSpec_ShouldFail()
    {
        var spec = Node.NewSpec();

        FluentActions.Invoking(() => spec.AddContainer("Folder")).Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void AddSpec_ToSpec_ShouldFail()
    {
        var spec = Node.NewSpec();

        FluentActions.Invoking(() => spec.AddSpec("Spec")).Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void AddSpec_ValidName_ShouldHaveExpectedValues()
    {
        var collection = Node.NewContainer("Test");

        var node = collection.AddSpec("MySpec");

        node.NodeId.Should().NotBeEmpty();
        node.ParentId.Should().NotBeEmpty();
        node.Parent.Should().Be(collection);
        node.Type.Should().Be(NodeType.Spec);
        node.Name.Should().Be("MySpec");
        node.Depth.Should().Be(1);
    }

    [Test]
    public void Path_NestedSpec_ShouldBeExpected()
    {
        var root = Node.NewCollection("Collection");
        var first = root.AddContainer("First");
        var second = first.AddContainer("Second");
        var spec = second.AddSpec();

        var path = spec.Path;

        path.Should().Be("Collection/First/Second");
    }

    [Test]
    public void AddNode_ValidNode_ShouldHaveExpectedCount()
    {
        var collection = Node.NewCollection();
        var folder = Node.NewContainer();

        collection.AddNode(folder);

        collection.Nodes.Should().HaveCount(1);
        folder.Parent.Should().Be(collection);
    }

    [Test]
    public void AddNode_NullNode_ShouldHaveExpectedCount()
    {
        var collection = Node.NewContainer();

        FluentActions.Invoking(() => collection.AddNode(null!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void RemoveNode_ValidNode_ShouldHaveExpectedCount()
    {
        var collection = Node.NewContainer();
        var folder1 = collection.AddContainer("Folder1");
        folder1.AddSpec("MySpec");
        var folder2 = collection.AddContainer("Folder2");

        collection.RemoveNode(folder1);
        collection.Nodes.Should().HaveCount(1);
        collection.Nodes.First().Should().BeEquivalentTo(folder2);

        collection.RemoveNode(folder2);
        collection.Nodes.Should().BeEmpty();
    }

    [Test]
    public void RemoveNode_NullNode_ShouldHaveExpectedCount()
    {
        var collection = Node.NewContainer();

        FluentActions.Invoking(() => collection.RemoveNode(null!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ClearNodes_WhenCalled_ShouldClearNodes()
    {
        var collection = Node.NewCollection();
        collection.AddContainer();
        collection.AddSpec();

        collection.ClearNodes();

        collection.Nodes.Should().BeEmpty();
    }

    [Test]
    public void Ancestors_NestedHierarchy_ShouldHaveExpectedCount()
    {
        var collection = Node.NewCollection();
        var first = collection.AddContainer();
        var second = first.AddContainer();
        var spec = second.AddSpec();

        var ancestors = spec.Ancestors();

        ancestors.Should().HaveCount(3);
    }

    [Test]
    public void Ancestors_NestedHierarchy_ShouldHaveExpectedOrder()
    {
        var collection = Node.NewCollection();
        var first = collection.AddContainer();
        var second = first.AddContainer();
        var spec = second.AddSpec();

        var nodes = spec.Ancestors().ToList();

        nodes[0].Should().BeSameAs(collection);
        nodes[1].Should().BeSameAs(first);
        nodes[2].Should().BeSameAs(second);
    }

    [Test]
    public void AncestorsAndSelf_NestedHierarchy_ShouldHaveExpectedCount()
    {
        var collection = Node.NewCollection();
        var first = collection.AddContainer();
        var second = first.AddContainer();
        var spec = second.AddSpec();

        var path = spec.AncestorsAndSelf();

        path.Should().HaveCount(4);
    }

    [Test]
    public void AncestorsAndSelf_NestedHierarchy_ShouldHaveExpectedOrder()
    {
        var collection = Node.NewCollection();
        var first = collection.AddContainer();
        var second = first.AddContainer();
        var spec = second.AddSpec();

        var path = spec.AncestorsAndSelf().ToList();

        path[0].Should().BeSameAs(collection);
        path[1].Should().BeSameAs(first);
        path[2].Should().BeSameAs(second);
        path[3].Should().BeSameAs(spec);
    }

    [Test]
    public void Descendents_WhenCalled_ShouldHAveExpectedCount()
    {
        var collection = Node.NewContainer();
        var folder = collection.AddContainer();
        folder.AddSpec();

        var descendents = collection.Descendants().ToList();

        descendents.Should().HaveCount(2);
    }

    [Test]
    public void Descendants_EmptyCollection_ShouldBeEmpty()
    {
        var node = Node.NewContainer();

        var specs = node.Descendants(NodeType.Spec);

        specs.Should().BeEmpty();
    }

    [Test]
    public void Descendants_CollectionWithSpecs_ShouldHaveExpectedCount()
    {
        var collection = Node.NewContainer();
        collection.AddSpec();
        collection.AddSpec();
        collection.AddSpec();

        var specs = collection.Descendants(NodeType.Spec);

        specs.Should().HaveCount(3);
    }

    [Test]
    public void Descendants_NestedSpecs_ShouldHaveExpectedCount()
    {
        var collection = Node.NewContainer();
        var folder = collection.AddContainer();
        collection.AddSpec();
        folder.AddSpec();
        folder.AddSpec();

        var specs = collection.Descendants(NodeType.Spec);

        specs.Should().HaveCount(3);
    }

    [Test]
    public void Descendants_SpecNode_ShouldHaveExpectedCount()
    {
        var spec = Node.NewSpec();

        var specs = spec.Descendants(NodeType.Spec);

        specs.Should().HaveCount(1);
    }

    [Test]
    public void Contains_NodeExists_ReturnsTrue()
    {
        var collection = Node.NewCollection();
        var first = collection.AddContainer();
        var second = first.AddContainer();

        var result = collection.Contains(second.NodeId);

        result.Should().BeTrue();
    }

    [Test]
    public void Contains_NodeDoesNotExist_ReturnsFalse()
    {
        var collection = Node.NewCollection();
        collection.AddContainer();
        collection.AddContainer();

        var result = collection.Contains(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Test]
    public void IsDescendantOf_Null_ShouldThrowException()
    {
        var spec = Node.NewSpec();

        FluentActions.Invoking(() => spec.IsDescendantOf(null!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void IsDescendantOf_SeparateNodes_ShouldBeFalse()
    {
        var collection = Node.NewCollection();
        var separate = Node.NewSpec();

        var result = separate.IsDescendantOf(collection);

        result.Should().BeFalse();
    }

    [Test]
    public void IsDescendantOf_DirectChild_ShouldBeTrue()
    {
        var collection = Node.NewCollection();
        var spec = collection.AddSpec();

        var result = spec.IsDescendantOf(collection);

        result.Should().BeTrue();
    }

    [Test]
    public void IsDescendantOf_NestedSpecNode_ShouldBeTrue()
    {
        var collection = Node.NewCollection();
        var first = collection.AddContainer();
        var second = first.AddContainer();
        var spec = second.AddSpec();

        var result = spec.IsDescendantOf(collection);

        result.Should().BeTrue();
    }

    [Test]
    public void IsDescendantOf_Itself_ShouldBeFalse()
    {
        var collection = Node.NewCollection();
        var spec = collection.AddSpec();

        var result = spec.IsDescendantOf(spec);

        result.Should().BeFalse();
    }

    [Test]
    public void Configure_ValidConfig_ShouldNotBeNull()
    {
        var node = Node.NewSpec();

        node.Configure(c =>
        {
            c.Get(Element.Program);
            c.Where("Name", Operation.Like, "%Test");
            c.Validate("Disabled", Operation.EqualTo, true);
        });

        node.Spec.Should().NotBeNull();
    }

    [Test]
    public void Configure_NullConfig_ShouldThrowException()
    {
        var node = Node.NewSpec();

        FluentActions.Invoking(() => node.Configure((Action<Spec>)null!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Configure_ValidSpec_ShouldHaveExpectedCount()
    {
        var node = Node.NewSpec();
        var expected = new Spec(Element.Tag);

        node.Configure(expected);

        node.Spec.Should().BeEquivalentTo(expected);
    }

    [Test]
    public async Task RunAll_SingleConfiguredSpec_ShouldReturnExpectedResult()
    {
        var content = L5X.Load(Known.Test);

        var node = Node.NewSpec("Test", s =>
        {
            s.Get(Element.Tag);
            s.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            s.Validate("DataType", Operation.EqualTo, "SimpleType");
        });

        var verification = await node.Run(content);

        verification.Result.Should().Be(ResultState.Passed);
        verification.Evaluations.Should().NotBeEmpty();
        verification.Duration.Should().BeGreaterThan(0);
    }
}