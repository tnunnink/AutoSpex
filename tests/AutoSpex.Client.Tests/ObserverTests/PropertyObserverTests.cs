using AutoSpex.Client.Observers;
using FluentAssertions;
using L5Sharp.Core;

namespace AutoSpex.Client.Tests.ObserverTests;

[TestFixture]
public class PropertyObserverTests
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
    public void FindProperties_WhenCalled_ShouldWork()
    {
        var instance = new DataType("Test");
        instance.Members.Add(new DataTypeMember { Name = "Child1", DataType = "DINT" });
        instance.Members.Add(new DataTypeMember { Name = "Child2", DataType = "BOOL" });
        instance.Members.Add(new DataTypeMember { Name = "Child3", DataType = "TIMER" });
        var element = new ElementObserver(instance);
        var properties = element.Properties;

        var results = properties.SelectMany(p => p.FindProperties("Child"));

        results.Should().NotBeEmpty();
    }
    
    [Test]
    public void FindProperties_LoadedComplexType_ShouldWork()
    {
        var content = L5X.Load(Known.Test);
        var instance = content.DataTypes.Get("ComplexType");
        var element = new ElementObserver(instance);
        var properties = element.Properties;

        var results = properties.SelectMany(p => p.FindProperties("Test"));

        results.Should().NotBeEmpty();
    }
}