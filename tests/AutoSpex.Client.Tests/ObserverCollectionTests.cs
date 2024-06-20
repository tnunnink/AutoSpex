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
        using var context = new TestContext();
        
        var list = new List<Node>
        {
            Node.NewContainer(),
            Node.NewContainer(),
            Node.NewContainer(),
        };

        var collection = new ObserverCollection<Node, NodeObserver>(list, n => new NodeObserver(n));

        collection.Add(Node.NewContainer("Test"));

        collection.Should().HaveCount(4);
        list.Should().HaveCount(4);
    }
}