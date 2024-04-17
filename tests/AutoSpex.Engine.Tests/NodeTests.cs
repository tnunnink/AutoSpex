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
        node.NodeType.Should().Be(NodeType.Collection);
        node.Name.Should().Be("New Collection");
        node.Documentation.Should().BeEmpty();
        node.Depth.Should().Be(0);
        node.Path.Should().Be(string.Empty);
        node.Collection.Should().BeSameAs(node);
        node.Nodes.Should().BeEmpty();
    }

    [Test]
    public void NewCollection_Name_ShouldHaveExpectedValues()
    {
        var node = Node.NewCollection("Test");
        
        node.Name.Should().Be("Test");
    }

    [Test]
    public void NewFolder_Default_ShouldHaveExpectedValues()
    {
        var node = Node.NewFolder();

        node.NodeId.Should().NotBeEmpty();
        node.ParentId.Should().BeEmpty();
        node.Parent.Should().BeNull();
        node.NodeType.Should().Be(NodeType.Folder);
        node.Name.Should().Be("New Folder");
        node.Depth.Should().Be(0);
        node.Collection.Should().BeNull();
        node.Nodes.Should().BeEmpty();
    }

    [Test]
    public void NewFolder_Name_ShouldHaveExpectedValues()
    {
        var node = Node.NewFolder("Test");
        
        node.Name.Should().Be("Test");
    }

    [Test]
    public void NewSpec_Default_ShouldHaveExpectedValues()
    {
        var node = Node.NewSpec();

        node.NodeId.Should().NotBeEmpty();
        node.ParentId.Should().BeEmpty();
        node.Parent.Should().BeNull();
        node.NodeType.Should().Be(NodeType.Spec);
        node.Name.Should().Be("New Spec");
        node.Depth.Should().Be(0);
        node.Collection.Should().BeNull();
        node.Nodes.Should().BeEmpty();
    }

    [Test]
    public void Spec_Name_ShouldHaveExpectedValues()
    {
        var node = Node.NewSpec("Test");
        
        node.Name.Should().Be("Test");
    }

    [Test]
    public void AddFolder_ValidParameters_ShouldHaveExpectedValues()
    {
        var parent = Node.NewCollection("Test");

        var node = parent.AddFolder("Test");

        node.NodeId.Should().NotBeEmpty();
        node.ParentId.Should().NotBeEmpty();
        node.Parent.Should().NotBeNull();
        node.Parent?.Name.Should().Be("Test");
        node.NodeType.Should().Be(NodeType.Folder);
        node.Name.Should().Be("Test");
        node.Depth.Should().Be(1);
    }

    [Test]
    public void AddSpec_ValidName_ShouldHaveExpectedValues()
    {
        var collection = Node.NewCollection("Test");

        var node = collection.AddSpec("MySpec");

        node.NodeId.Should().NotBeEmpty();
        node.ParentId.Should().NotBeEmpty();
        node.Parent.Should().Be(collection);
        node.NodeType.Should().Be(NodeType.Spec);
        node.Name.Should().Be("MySpec");
        node.Depth.Should().Be(1);
    }

    [Test]
    public void Path_NestedSpec_ShouldBeExpected()
    {
        var collection = Node.NewCollection("Collection");
        var folder = collection.AddFolder("Folder");
        var spec = folder.AddSpec();

        var path = spec.Path;

        path.Should().Be("Collection/Folder");
    }

    [Test]
    public void AddNode_ValidNode_ShouldHaveExpectedCount()
    {
        var collection = Node.NewCollection();
        var folder = Node.NewFolder();

        collection.AddNode(folder);

        collection.Nodes.Should().HaveCount(1);
    }

    [Test]
    public void AddNode_NullNode_ShouldHaveExpectedCount()
    {
        var collection = Node.NewCollection();

        FluentActions.Invoking(() => collection.AddNode(null!)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddNode_InvalidNode_ShouldThrowArgumentException()
    {
        var collection = Node.NewCollection();
        var folder = Node.NewFolder();

        FluentActions.Invoking(() => folder.AddNode(collection)).Should().Throw<ArgumentException>();
    }

    [Test]
    public void RemoveNode_ValidNode_ShouldHaveExpectedCount()
    {
        var collection = Node.NewCollection();
        var folder1 = collection.AddFolder("Folder1");
        folder1.AddSpec("MySpec");
        var folder2 = collection.AddFolder("Folder2");

        collection.RemoveNode(folder1);
        collection.Nodes.Should().HaveCount(1);
        collection.Nodes.First().Should().BeEquivalentTo(folder2);

        collection.RemoveNode(folder2);
        collection.Nodes.Should().BeEmpty();
    }

    [Test]
    public void RemoveNode_NullNode_ShouldHaveExpectedCount()
    {
        var collection = Node.NewCollection();

        FluentActions.Invoking(() => collection.RemoveNode(null!)).Should().Throw<ArgumentNullException>();
    }
    
    
    [Test]
    public void Ancestors_NestedHierarchy_ShouldHaveExpectedCount()
    {
        var collection = Node.NewCollection();
        var folder = collection.AddFolder();
        var spec = folder.AddSpec();

        var ancestors = spec.Ancestors();

        ancestors.Should().HaveCount(2);
    }

    [Test]
    public void Ancestors_NestedHierarchy_ShouldHaveExpectedOrder()
    {
        var collection = Node.NewCollection();
        var folder = collection.AddFolder();
        var spec = folder.AddSpec();

        var nodes = spec.Ancestors().ToList();

        nodes[0].Should().BeSameAs(folder);
        nodes[1].Should().BeSameAs(collection);
    }

    [Test]
    public void AncestralPath_NestedHierarchy_ShouldHaveExpectedCount()
    {
        var collection = Node.NewCollection();
        var folder = collection.AddFolder();
        var spec = folder.AddSpec();

        var path = spec.AncestralPath();

        path.Should().HaveCount(3);
    }

    [Test]
    public void AncestralPath_NestedHierarchy_ShouldHaveExpectedOrder()
    {
        var collection = Node.NewCollection();
        var folder = collection.AddFolder();
        var spec = folder.AddSpec();

        var path = spec.AncestralPath().ToList();

        path[0].Should().BeSameAs(collection);
        path[1].Should().BeSameAs(folder);
        path[2].Should().BeSameAs(spec);
    }

    [Test]
    public void Descendents_WhenCalled_ShouldHAveExpectedCount()
    {
        var collection = Node.NewCollection();
        var folder = collection.AddFolder();
        folder.AddSpec();

        var descendents = collection.Descendents().ToList();

        descendents.Should().HaveCount(2);
    }

    [Test]
    public void Collection_GetFromNestedSpec_ShouldNotBeNullAndExpectedInstance()
    {
        var collection = Node.NewCollection();
        var folder = collection.AddFolder();
        var spec = folder.AddSpec();

        var result = spec.Collection;

        result.Should().NotBeNull();
        result.Should().BeSameAs(collection);
    }

    [Test]
    public void Specs_EmptyCollection_ShouldBeEmpty()
    {
        var node = Node.NewCollection();

        var specs = node.Specs();

        specs.Should().BeEmpty();
    }

    [Test]
    public void Spec_CollectionWithSpecs_ShouldHaveExpectedCount()
    {
        var collection = Node.NewCollection();
        collection.AddSpec();
        collection.AddSpec();
        collection.AddSpec();

        var specs = collection.Specs();

        specs.Should().HaveCount(3);
    }
    
    [Test]
    public void Spec_NestedSpecs_ShouldHaveExpectedCount()
    {
        var collection = Node.NewCollection();
        var folder = collection.AddFolder();
        collection.AddSpec();
        folder.AddSpec();
        folder.AddSpec();

        var specs = collection.Specs();

        specs.Should().HaveCount(3);
    }
    
    [Test]
    public void Spec_SpecNode_ShouldHaveExpectedCount()
    {
        var spec = Node.NewSpec();
        
        var specs = spec.Specs();

        specs.Should().HaveCount(1);
    }
}