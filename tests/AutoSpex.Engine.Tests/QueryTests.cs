namespace AutoSpex.Engine.Tests;

[TestFixture]
public class QueryTests
{
    [Test]
    public void New_Default_ShouldBeExpected()
    {
        var query = new Query();

        query.Element.Should().Be(Element.Default);
        query.Name.Should().BeNull();
        query.ContainerName.Should().BeNull();
        query.LocalName.Should().BeNull();
    }

    [Test]
    public void New_EnabledValidName_ShouldBeExpected()
    {
        var query = new Query(Element.DataType, "SimpleType");

        query.Element.Should().Be(Element.DataType);
        query.Name.Should().Be("SimpleType");
        query.ContainerName.Should().BeNull();
        query.LocalName.Should().Be("SimpleType");
    }

    [Test]
    public void New_WithProgramSpecifier_ShouldHaveExpectedProgramAndTagName()
    {
        var query = new Query(Element.Tag, "Program:MyProgram_01.SomeTagName[1].Member.1");

        query.Element.Should().Be(Element.Tag);
        query.Name.Should().Be("Program:MyProgram_01.SomeTagName[1].Member.1");
        query.ContainerName.Should().Be("MyProgram_01");
        query.LocalName.Should().Be("SomeTagName[1].Member.1");
    }

    [Test]
    public void Execute_AllDataTypes_ShouldNotBeEmpty()
    {
        var query = new Query(Element.DataType);

        var result = query.Execute(L5X.Load(Known.Test));

        result.Should().NotBeEmpty();
    }

    [Test]
    public void Execute_SimpleType_ShouldReturnSingleElement()
    {
        var query = new Query(Element.DataType, "SimpleType");

        var result = query.Execute(L5X.Load(Known.Test));

        result.Should().HaveCount(1);
    }

    [Test]
    public void Execute_AllTags_ShouldNotBeEmpty()
    {
        var query = new Query(Element.Tag);

        var result = query.Execute(L5X.Load(Known.Test));

        result.Should().NotBeEmpty();
    }
    
    [Test]
    public void Execute_KnownTag_ShouldHaveExpectedCount()
    {
        var query = new Query(Element.Tag, "SimpleDint");

        var result = query.Execute(L5X.Load(Known.Test)).ToList();

        result.Should().HaveCount(1);
    }
    
    [Test]
    public void Execute_AllModules_ShouldNotBeEmpty()
    {
        var query = new Query(Element.Module);

        var result = query.Execute(L5X.Load(Known.Test));

        result.Should().NotBeEmpty();
    }
}