using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using FluentAssertions;

namespace AutoSpex.Client.Tests;

[TestFixture]
public class ObserverCollectionTests
{
    [Test]
    public void Add_ToCollectionInitializedWithModelListAndWrapper_ShouldUpdateBothCollections()
    {
        var list = new List<Node>
        {
            Node.Collection(Feature.Specifications, "Test1"),
            Node.Collection(Feature.Specifications, "Test2"),
            Node.Collection(Feature.Specifications, "Test3"),
        };

        var collection = new ObserverCollection<Node, NodeObserver>(list, n => new NodeObserver(n));
        
        collection.Add(new NodeObserver(Node.Collection(Feature.Runners, "Test")));

        collection.Should().HaveCount(4);
        list.Should().HaveCount(4);
    }
    
    [Test]
    public void Add_ToCollectionInitializedCustomFunctions_ShouldUpdateBothCollections()
    {
        var list = new List<NodeObserver>
        {
            new(Node.Collection(Feature.Specifications, "Test1")),
            new(Node.Collection(Feature.Specifications, "Test2")),
            new(Node.Collection(Feature.Specifications, "Test3")),
        };

        var collection = new ObserverCollection<Node, NodeObserver>(
            list,
            (_, n) => list.Add(n),
            (i, _) => list.RemoveAt(i));
        
        collection.Add(new NodeObserver(Node.Collection(Feature.Runners, "Test")));

        collection.Should().HaveCount(4);
        list.Should().HaveCount(4);
    }
}