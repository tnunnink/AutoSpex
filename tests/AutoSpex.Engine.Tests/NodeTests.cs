using Task = System.Threading.Tasks.Task;

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
        node.Depth.Should().Be(0);
        node.Ordinal.Should().Be(0);
        node.Path.Should().Be(string.Empty);
        node.Collection.Should().BeSameAs(node);
        node.Spec.Should().BeNull();
        node.Variables.Should().BeEmpty();
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
        node.Ordinal.Should().Be(0);
        node.Collection.Should().BeNull();
        node.Spec.Should().BeNull();
        node.Variables.Should().BeEmpty();
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
        node.Ordinal.Should().Be(0);
        node.Collection.Should().BeNull();
        node.Spec.Should().NotBeNull();
        node.Variables.Should().BeEmpty();
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
        node.Ordinal.Should().Be(0);
        node.Spec.Should().BeNull();
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
        node.Ordinal.Should().Be(0);
        node.Spec.Should().NotBeNull();
    }

    [Test]
    public void Ordinal_MultipleSpec_ShouldBeExpected()
    {
        var collection = Node.NewCollection();
        var spec1 = collection.AddSpec();
        var spec2 = collection.AddSpec();
        var spec3 = collection.AddSpec();

        spec1.Ordinal.Should().Be(0);
        spec2.Ordinal.Should().Be(1);
        spec3.Ordinal.Should().Be(2);
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
    public void InsertNode_ValidNode_ShouldHaveExpectedCount()
    {
        var collection = Node.NewCollection();
        var first = collection.AddFolder();
        var second = collection.AddSpec();

        var folder = Node.NewFolder();
        collection.InsertNode(folder, 1);

        collection.Nodes.Should().HaveCount(3);
        folder.Ordinal.Should().Be(1);
        first.Ordinal.Should().Be(0);
        second.Ordinal.Should().Be(2);
    }

    [Test]
    public void InsertNode_NullNode_ShouldHaveExpectedCount()
    {
        var collection = Node.NewCollection();

        FluentActions.Invoking(() => collection.InsertNode(null!, 0)).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void InsertNode_InvalidNode_ShouldThrowArgumentException()
    {
        var collection = Node.NewCollection();
        var folder = Node.NewFolder();

        FluentActions.Invoking(() => folder.InsertNode(collection, 0)).Should().Throw<ArgumentException>();
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
    public void OrphanedCopy_WhenCalled_ShouldHaveExpectedValues()
    {
        var node = Node.NewCollection();
        var child = node.AddFolder();
        
        var copy = child.OrphanedCopy();

        copy.NodeId.Should().Be(child.NodeId);
        copy.ParentId.Should().BeEmpty();
        copy.Parent.Should().BeNull();
        copy.NodeType.Should().Be(child.NodeType);
        copy.Name.Should().Be(child.Name);
        copy.Depth.Should().Be(child.Depth);
        copy.Ordinal.Should().Be(child.Ordinal);
        copy.Path.Should().BeEmpty();
        copy.Should().NotBeSameAs(child);
        copy.Should().Be(child);
    }
    
    [Test]
    public void OrphanedCopy_NodeWithChildren_ShouldHaveNoChildren()
    {
        var node = Node.NewCollection();
        var child = node.AddFolder();
        child.AddSpec();
        child.AddSpec();
        child.AddSpec();
        
        var copy = child.OrphanedCopy();

        copy.Nodes.Should().BeEmpty();
    }
    
    [Test]
    public void OrphanedCopy_NodeWithVariables_ShouldHaveSameVariables()
    {
        var node = Node.NewCollection();
        var child = node.AddFolder();
        child.AddVariable("Test1", "Var");
        child.AddVariable("Test2", "Var");
        child.AddVariable("Test3", "Var");
        
        var copy = child.OrphanedCopy();

        copy.Variables.Should().HaveCount(3);
    }
    
    [Test]
    public void OrphanedCopy_NodeWithSpec_ShouldHaveSameChildren()
    {
        var node = Node.NewCollection();
        var child = node.AddSpec();
        
        var copy = child.OrphanedCopy();

        copy.Spec.Should().BeEquivalentTo(child.Spec);
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
    public void Configure_ValidSpecNode_ShouldUpdateSpecProperty()
    {
        var spec = Node.NewSpec();

        var config = new Spec {Element = Element.Tag};
        spec.Configure(config);

        spec.Spec.Should().BeEquivalentTo(config, options => options.Excluding(info => info.SpecId));
        spec.Spec?.Element.Should().Be(Element.Tag);
    }

    [Test]
    public void Configure_CollectionNode_ShouldThrowInvalidOperationException()
    {
        var node = Node.NewCollection();

        FluentActions.Invoking(() => node.Configure(new Spec())).Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Configure_FolderNode_ShouldThrowInvalidOperationException()
    {
        var node = Node.NewFolder();

        FluentActions.Invoking(() => node.Configure(new Spec())).Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Configure_SpecNode_ShouldHaveExpectedSpec()
    {
        var spec = Node.NewSpec();

        spec.Configure(c =>
        {
            c.Element = Element.DataType;
            c.Where("Name", Operation.Contains, "MyType");
            c.Verify("Members", Operation.Any, new Criterion("DataType", Operation.Equal, "DINT"));
        });

        spec.Spec?.Element.Should().Be(Element.DataType);
        spec.Spec?.Filters.Should().HaveCount(1);
        spec.Spec?.Verifications.Should().HaveCount(1);
    }

    [Test]
    public void AddVariable_ValidVariable_ShouldHaveExpectedCount()
    {
        var node = Node.NewCollection();
        var variable = new Variable("MyVar", "MyVal");

        node.AddVariable(variable);

        node.Variables.Should().HaveCount(1);
    }

    [Test]
    public void AddVariable_MultipleVariables_ShouldHaveExpectedCount()
    {
        var node = Node.NewCollection();

        node.AddVariable(new Variable("Var1", "Test1"));
        node.AddVariable(new Variable("Var2", "Test2"));
        node.AddVariable(new Variable("Var3", "Test3"));

        node.Variables.Should().HaveCount(3);
    }

    [Test]
    public void AddVariable_DuplicateName_ShouldThrowArgumentException()
    {
        var node = Node.NewCollection();
        node.AddVariable(new Variable("MyVar", "MyVal"));

        FluentActions.Invoking(() => node.AddVariable(new Variable("MyVar", "Test"))).Should()
            .Throw<ArgumentException>();
    }

    [Test]
    public void AddVariables_DifferentLevelsDifferentNames_EachLevelShouldHaveExpectedCount()
    {
        var collection = Node.NewCollection();
        var folder = collection.AddFolder();
        var spec = folder.AddSpec();

        collection.AddVariable(new Variable("CollectionVar", "Test"));
        folder.AddVariable(new Variable("FolderVar", "Test"));
        spec.AddVariable(new Variable("SpecVar", "Test"));

        collection.Variables.Should().HaveCount(1);
        folder.Variables.Should().HaveCount(1);
        spec.Variables.Should().HaveCount(1);
    }

    [Test]
    public void AddVariables_DifferentLevelsSameNames_EachLevelShouldHaveExpectedCount()
    {
        var collection = Node.NewCollection();
        var folder = collection.AddFolder();
        var spec = folder.AddSpec();

        collection.AddVariable(new Variable("Var", "Test"));
        folder.AddVariable(new Variable("Var", "Test"));
        spec.AddVariable(new Variable("Var", "Test"));

        collection.Variables.Should().HaveCount(1);
        folder.Variables.Should().HaveCount(1);
        spec.Variables.Should().HaveCount(1);
    }
    
    [Test]
    public void AddVariable_WithParameters_ShouldHaveExpectedCount()
    {
        var node = Node.NewCollection();

        node.AddVariable("Test", "Value");

        node.Variables.Should().HaveCount(1);
    }

    [Test]
    public void RemoveVariable_ValidVariable_ShouldHaveExpectedCount()
    {
        var node = Node.NewFolder();
        var variable = node.AddVariable("Test", "value");
        node.Variables.Should().HaveCount(1);
        
        node.RemoveVariable(variable);
        node.Variables.Should().BeEmpty();
    }

    [Test]
    public void FindVariable_ValidVariable_ShouldReturnExpected()
    {
        var node = Node.NewCollection();
        var expected = node.AddVariable("Test", "Var");
        var folder = node.AddFolder();
        var spec = folder.AddSpec();

        var result = spec.FindVariable("Test");

        result.Should().BeSameAs(expected);
    }
    
    [Test]
    public void FindVariable_NoVariable_ShouldReturnNull()
    {
        var node = Node.NewCollection();
        node.AddVariable("MyVar", "Var");
        var folder = node.AddFolder();
        var spec = folder.AddSpec();

        var result = spec.FindVariable("Test");

        result.Should().BeNull();
    }
    
    [Test]
    public async Task Run_SpecAgainstKnownTag_ShouldBePassedAndExpectedValues()
    {
        var source = new Source(L5X.Load(Known.Test));
        var spec = Node.NewSpec("MySpec");
        spec.Configure(c =>
        {
            c.Element = Element.Tag;
            c.Filters.Add(new Criterion("TagName", Operation.Equal, "TestSimpleTag"));
            c.Verifications.Add(new Criterion("DataType", Operation.Equal, "SimpleType"));
        });

        var result = await spec.Run(source);

        result.Result.Should().Be(ResultState.Passed);
        result.Node?.Name.Should().Be("MySpec");
        result.NodeId.Should().Be(spec.NodeId);
        result.Verifications.Should().HaveCount(2);
        result.Duration.Should().BeLessThan(1000);
        result.Passed.Should().Be(2);
        result.Failed.Should().Be(0);
        result.Errored.Should().Be(0);
    }
    
    [Test]
    public async Task Run_SpecAgainstKnownTagWithVariableInVerification_ShouldBePassedAndExpectedValues()
    {
        var source = new Source(L5X.Load(Known.Test));
        var spec = Node.NewSpec("MySpec");
        var variable = spec.AddVariable("ExpectedType", "SimpleType");
        spec.Configure(c =>
        {
            c.Element = Element.Tag;
            c.Filters.Add(new Criterion("TagName", Operation.Equal, "TestSimpleTag"));
            c.Verifications.Add(new Criterion("DataType", Operation.Equal, variable));
        });

        var result = await spec.Run(source);

        result.Result.Should().Be(ResultState.Passed);
    }
}