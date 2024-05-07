using AutoSpex.Client.Components;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using FluentAssertions;

namespace AutoSpex.Client.Tests;

[TestFixture]
public class DesignDataTests
{
    [Test]
    public void DesignDataBreadcrumb()
    {
        using var context = new TestContext();
        var test = new Breadcrumb(Node.NewCollection().AddFolder().AddSpec(), CrumbType.Target);
        test.Items.ToList().Should().NotBeEmpty();
    }
    
    [Test]
    public void DesignDataSources()
    {
        using var context = new TestContext();
        var test = DesignData.Sources;
        test.Should().NotBeEmpty();
    }

    [Test]
    public void DesignSpecsCollection()
    {
        using var context = new TestContext();
        var collection = DesignData.Specs;
        collection.Should().NotBeEmpty();
    }
}