using AutoSpex.Client.Observers;
using FluentAssertions;
using L5Sharp.Core;

namespace AutoSpex.Client.Tests.ObserverTests;

[TestFixture]
public class ElementObserverTests
{
    private TestContext? _context;

    [SetUp]
    public void Setup()
    {
        _context = new TestContext();
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }
    
    [Test]
    public void ElementObserverPropertiesWillGetCollectionPropertiesToo()
    {
        var content = L5X.Load(Known.Test);
        var dataType = content.DataTypes.Get("ComplexType");
        
        var element = new ElementObserver(dataType);

        element.Should().NotBeNull();
        var members = element.Properties.First(p => p.Name == "Members");
        var first = members.Properties.First();
        var name = first.Properties.First(p => p.Name == "Name");
        var value = name.Value;
        value.Should().NotBeNull();
    }
}