namespace AutoSpex.Engine.Tests;

[TestFixture]
public class NodeTests
{
    [Test]
    public void NewContainer_Default_ShouldHaveExpectedValues()
    {
        var node = Node.NewContainer();

        node.NodeId.Should().NotBeEmpty();
        node.ParentId.Should().BeEmpty();
        node.Parent.Should().BeNull();
        node.Type.Should().Be(NodeType.Container);
        node.Name.Should().Be("New Container");
        node.Depth.Should().Be(0);
        node.Path.Should().Be(string.Empty);
        node.Base.Should().BeSameAs(node);
        node.Nodes.Should().BeEmpty();
    }

    [Test]
    public void NewContainer_Name_ShouldHaveExpectedValues()
    {
        var node = Node.NewContainer("Test");

        node.Name.Should().Be("Test");
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
    }
    
    [Test]
    public void NewSource_Default_ShouldHaveExpectedValues()
    {
        var node = Node.NewSource("MySource");

        node.NodeId.Should().NotBeEmpty();
        node.ParentId.Should().BeEmpty();
        node.Parent.Should().BeNull();
        node.Type.Should().Be(NodeType.Source);
        node.Name.Should().Be("MySource");
    }
    
    [Test]
    public void NewRun_Default_ShouldHaveExpectedValues()
    {
        var node = Node.NewRun("MyRun");

        node.NodeId.Should().NotBeEmpty();
        node.ParentId.Should().BeEmpty();
        node.Parent.Should().BeNull();
        node.Type.Should().Be(NodeType.Run);
        node.Name.Should().Be("MyRun");
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
        var root = Node.NewContainer("Root"); //this would represent the root feature node
        var first = root.AddContainer("First");
        var second = first.AddContainer("Second");
        var spec = second.AddSpec();

        var path = spec.Path;

        path.Should().Be("First/Second");
    }

    [Test]
    public void AddNode_ValidNode_ShouldHaveExpectedCount()
    {
        var collection = Node.NewContainer();
        var folder = Node.NewContainer();

        collection.AddNode(folder);

        collection.Nodes.Should().HaveCount(1);
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
    public void Ancestors_NestedHierarchy_ShouldHaveExpectedCount()
    {
        var root = Node.NewContainer(); //this would represent the root feature node
        var first = root.AddContainer();
        var second = first.AddContainer();
        var spec = second.AddSpec();

        var ancestors = spec.Ancestors();

        ancestors.Should().HaveCount(2);
    }

    [Test]
    public void Ancestors_NestedHierarchy_ShouldHaveExpectedOrder()
    {
        var root = Node.NewContainer(); //this would represent the root feature node
        var first = root.AddContainer();
        var second = first.AddContainer();
        var spec = second.AddSpec();

        var nodes = spec.Ancestors().ToList();

        nodes[0].Should().BeSameAs(first);
        nodes[1].Should().BeSameAs(second);
    }

    [Test]
    public void AncestorsAndSelf_NestedHierarchy_ShouldHaveExpectedCount()
    {
        var root = Node.NewContainer(); //this would represent the root feature node
        var first = root.AddContainer();
        var second = first.AddContainer();
        var spec = second.AddSpec();

        var path = spec.AncestorsAndSelf();

        path.Should().HaveCount(3);
    }

    [Test]
    public void AncestorsAndSelf_NestedHierarchy_ShouldHaveExpectedOrder()
    {
        var root = Node.NewContainer(); //this would represent the root feature node
        var first = root.AddContainer();
        var second = first.AddContainer();
        var spec = second.AddSpec();

        var path = spec.AncestorsAndSelf().ToList();

        path[0].Should().BeSameAs(first);
        path[1].Should().BeSameAs(second);
        path[2].Should().BeSameAs(spec);
    }

    [Test]
    public void Descendents_WhenCalled_ShouldHAveExpectedCount()
    {
        var collection = Node.NewContainer();
        var folder = collection.AddContainer();
        folder.AddSpec();

        var descendents = collection.Descendents().ToList();

        descendents.Should().HaveCount(2);
    }

    [Test]
    public void Base_GetFromNestedSpec_ShouldNotBeNullAndExpectedInstance()
    {
        var collection = Node.NewContainer();
        var folder = collection.AddContainer();
        var spec = folder.AddSpec();

        var result = spec.Base;

        result.Should().NotBeNull();
        result.Should().BeSameAs(collection);
    }

    [Test]
    public void Descendants_EmptyCollection_ShouldBeEmpty()
    {
        var node = Node.NewContainer();

        var specs = node.Descendents(NodeType.Spec);

        specs.Should().BeEmpty();
    }

    [Test]
    public void Descendants_CollectionWithSpecs_ShouldHaveExpectedCount()
    {
        var collection = Node.NewContainer();
        collection.AddSpec();
        collection.AddSpec();
        collection.AddSpec();

        var specs = collection.Descendents(NodeType.Spec);

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

        var specs = collection.Descendents(NodeType.Spec);

        specs.Should().HaveCount(3);
    }

    [Test]
    public void SDescendants_SpecNode_ShouldHaveExpectedCount()
    {
        var spec = Node.NewSpec();

        var specs = spec.Descendents(NodeType.Spec);

        specs.Should().HaveCount(1);
    }
}