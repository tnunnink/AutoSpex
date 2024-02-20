using AutoSpex.Client.Components;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
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
            Node.NewCollection(),
            Node.NewCollection(),
            Node.NewCollection(),
        };

        var collection = new ObserverCollection<Node, NodeObserver>(list, n => new NodeObserver(n));

        collection.Add(new NodeObserver(Node.NewCollection("Test")));

        collection.Should().HaveCount(4);
        list.Should().HaveCount(4);
    }

    [Test]
    public void Add_ToCollectionInitializedCustomFunctions_ShouldUpdateBothCollections()
    {
        var list = new List<Node>
        {
            Node.NewCollection(),
            Node.NewCollection(),
            Node.NewCollection(),
        };

        var collection = new ObserverCollection<Node, NodeObserver>(list.ToList(), m => new NodeObserver(m));

        collection.Should().HaveCount(4);
        list.Should().HaveCount(4);
    }
}