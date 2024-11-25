using System.Globalization;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class TypeGroupTests
{
    [Test]
    public void Default_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Default;

        group.Name.Should().Be("Default");
        group.Value.Should().Be(0);
    }

    [Test]
    public void Boolean_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Boolean;

        group.Name.Should().Be("Boolean");
        group.Value.Should().Be(1);
    }

    [Test]
    public void Number_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Number;

        group.Name.Should().Be("Number");
        group.Value.Should().Be(2);
    }

    [Test]
    public void Text_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Text;

        group.Name.Should().Be("Text");
        group.Value.Should().Be(3);
    }

    [Test]
    public void Date_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Date;

        group.Name.Should().Be("Date");
        group.Value.Should().Be(4);
    }

    [Test]
    public void Enum_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Enum;

        group.Name.Should().Be("Enum");
        group.Value.Should().Be(5);
    }

    [Test]
    public void Element_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Element;

        group.Name.Should().Be("Element");
        group.Value.Should().Be(6);
    }

    [Test]
    public void Collection_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Collection;

        group.Name.Should().Be("Collection");
        group.Value.Should().Be(7);
    }

    [Test]
    public void Criterion_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Criterion;

        group.Name.Should().Be("Criterion");
        group.Value.Should().Be(8);
    }

    [Test]
    public void Range_WhenCalled_ShouldBeExpected()
    {
        var group = TypeGroup.Range;

        group.Name.Should().Be("Range");
        group.Value.Should().Be(9);
    }

    [Test]
    public void FromType_Bool_ShouldBeBoolean()
    {
        var group = TypeGroup.FromType(typeof(bool));

        group.Should().Be(TypeGroup.Boolean);
    }

    [Test]
    public void FromType_Int_ShouldBeNumber()
    {
        var group = TypeGroup.FromType(typeof(int));

        group.Should().Be(TypeGroup.Number);
    }

    [Test]
    public void FromType_String_ShouldBeText()
    {
        var group = TypeGroup.FromType(typeof(string));

        group.Should().Be(TypeGroup.Text);
    }

    [Test]
    public void FromType_IEnumerable_ShouldBeCollection()
    {
        var group = TypeGroup.FromType(typeof(IEnumerable<>));

        group.Should().Be(TypeGroup.Collection);
    }

    [Test]
    public void FromType_IEnumerableOfString_ShouldBeCollection()
    {
        var group = TypeGroup.FromType(typeof(IEnumerable<string>));

        group.Should().Be(TypeGroup.Collection);
    }

    [Test]
    public void FromType_LogixContainer_ShouldBeCollection()
    {
        var group = TypeGroup.FromType(typeof(LogixContainer<Tag>));

        group.Should().Be(TypeGroup.Collection);
    }

    #region ParseTests

    [Test]
    public void Parse_BooleanTrueValue_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("true");

        result.Should().BeOfType<bool>();
        result.Should().Be(true);
    }

    [Test]
    public void Parse_BooleanFalseValue_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("false");

        result.Should().BeOfType<bool>();
        result.Should().Be(false);
    }

    [Test]
    public void Parse_NumberZeroValue_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("0");

        result.Should().BeOfType<int>();
        result.Should().Be(0);
    }

    [Test]
    public void Parse_NumberOneValue_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("1");

        result.Should().BeOfType<int>();
        result.Should().Be(1);
    }

    [Test]
    public void Parse_NumberPositiveValue_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("134567");

        result.Should().BeOfType<int>();
        result.Should().Be(134567);
    }

    [Test]
    public void Parse_NumberNegativeValue_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("-19435");

        result.Should().BeOfType<int>();
        result.Should().Be(-19435);
    }

    [Test]
    public void Parse_NumberDoubleZeroValue_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("0.0");

        result.Should().BeOfType<double>();
        result.Should().Be(0.0);
    }

    [Test]
    public void Parse_NumberDoublePositiveValue_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("12.3456");

        result.Should().BeOfType<double>();
        result.Should().Be(12.3456);
    }

    [Test]
    public void Parse_NumberDoubleNegativeValue_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("-19.435");

        result.Should().BeOfType<double>();
        result.Should().Be(-19.435);
    }

    [Test]
    public void Parse_TextValue_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("Testing");

        result.Should().BeOfType<string>();
        result.Should().Be("Testing");
    }

    [Test]
    public void Parse_EnumRadixValue_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("Float");

        result.Should().BeAssignableTo<Radix>();
        result.Should().Be(Radix.Float);
    }

    [Test]
    public void Parse_EnumRoutineTypeValue_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("RLL");

        result.Should().BeAssignableTo<RoutineType>();
        result.Should().Be(RoutineType.RLL);
    }

    [Test]
    public void Parse_EnumProgramValue_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("Program");

        result.Should().BeAssignableTo<ReferenceType>();
        result.Should().Be(ReferenceType.Program);
    }

    [Test]
    public void Parse_DateTime_ShouldBeExpected()
    {
        var result = TypeGroup.Parse(DateTime.Today.ToString(CultureInfo.InvariantCulture));

        result.Should().BeOfType<DateTime>();
        result.Should().Be(DateTime.Today);
    }

    [Test]
    public void Parse_ElementValidDataTypeElement_ShouldBeExpected()
    {
        const string xml = """
                           <DataType Name="SimpleType" Family="NoFamily" Class="User">
                               <Description>
                                   <![CDATA[This is a test data type that contains simple atomic types with an updated description]]>
                               </Description>
                               <Members>
                                   <Member Name="ZZZZZZZZZZSimpleType0" DataType="SINT" Dimension="0" Radix="Decimal" Hidden="true"
                                           ExternalAccess="Read/Write"/>
                                   <Member Name="BoolMember" DataType="BIT" Dimension="0" Radix="Hex" Hidden="false"
                                           Target="ZZZZZZZZZZSimpleType0" BitNumber="0" ExternalAccess="Read/Write">
                                       <Description>
                                           <![CDATA[Test Bool]]>
                                       </Description>
                                   </Member>
                                   <Member Name="SintMember" DataType="SINT" Dimension="0" Radix="Hex" Hidden="false"
                                           ExternalAccess="Read/Write">
                                       <Description>
                                           <![CDATA[Test Sint]]>
                                       </Description>
                                   </Member>
                                   <Member Name="IntMember" DataType="INT" Dimension="0" Radix="Octal" Hidden="false"
                                           ExternalAccess="Read/Write">
                                       <Description>
                                           <![CDATA[Test Int]]>
                                       </Description>
                                   </Member>
                                   <Member Name="DintMember" DataType="DINT" Dimension="0" Radix="ASCII" Hidden="false"
                                           ExternalAccess="None">
                                       <Description>
                                           <![CDATA[Test Dint]]>
                                       </Description>
                                   </Member>
                                   <Member Name="LintMember" DataType="LINT" Dimension="0" Radix="Decimal" Hidden="false"
                                           ExternalAccess="Read/Write">
                                       <Description>
                                           <![CDATA[Test Lint]]>
                                       </Description>
                                   </Member>
                                   <Member Name="RealMember" DataType="REAL" Dimension="0" Radix="Float" Hidden="false"
                                           ExternalAccess="Read/Write">
                                       <Description>
                                           <![CDATA[Test Real]]>
                                       </Description>
                                   </Member>
                               </Members>
                           </DataType>
                           """;

        var result = TypeGroup.Parse(xml);

        result.Should().BeAssignableTo<LogixElement>();
        result.Should().BeOfType<DataType>();
    }

    [Test]
    public void Parse_CollectionOfInteger_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("[1,2,3,4]");

        result.Should().BeOfType<List<object>>();
        result.Should().BeEquivalentTo(new List<object> { 1, 2, 3, 4 });
        result.As<List<object>>().Should().AllBeOfType<int>();
    }

    [Test]
    public void Parse_CollectionOfDouble_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("[1.1,2.2,3.3,4.4]");

        result.Should().BeOfType<List<object>>();
        result.Should().BeEquivalentTo(new List<object> { 1.1, 2.2, 3.3, 4.4 });
        result.As<List<object>>().Should().AllBeOfType<double>();
    }

    [Test]
    public void Parse_CollectionOfText_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("[First,second,third,another]");

        result.Should().BeOfType<List<object>>();
        result.Should().BeEquivalentTo(new List<object> { "First", "second", "third", "another" });
        result.As<List<object>>().Should().AllBeOfType<string>();
    }

    [Test]
    public void Parse_CriterionUnaryOperation_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("TagName Is Null");

        result.Should().BeOfType<Criterion>();
        result.As<Criterion>().Property.Should().Be("TagName");
        result.As<Criterion>().Negation.Should().Be(Negation.Is);
        result.As<Criterion>().Operation.Should().Be(Operation.Null);
        result.As<Criterion>().Argument.Should().BeNull();
    }

    [Test]
    public void Parse_CriterionBinaryOperation_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("Value Is Equal To 123");

        result.Should().BeOfType<Criterion>();
        result.As<Criterion>().Property.Should().Be("Value");
        result.As<Criterion>().Negation.Should().Be(Negation.Is);
        result.As<Criterion>().Operation.Should().Be(Operation.EqualTo);
        result.As<Criterion>().Argument.Should().Be(123);
    }

    [Test]
    public void Parse_CriterionBetweenOperation_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("Value Is Between 1 and 10");

        result.Should().BeOfType<Criterion>();
        result.As<Criterion>().Property.Should().Be("Value");
        result.As<Criterion>().Negation.Should().Be(Negation.Is);
        result.As<Criterion>().Operation.Should().Be(Operation.Between);
        result.As<Criterion>().Argument.Should().BeEquivalentTo(new Range(1, 10));
    }

    [Test]
    public void Parse_CriterionInOperation_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("Radix Not In [Decimal,Octal,Binary]");

        result.Should().BeOfType<Criterion>();
        result.As<Criterion>().Property.Should().Be("Radix");
        result.As<Criterion>().Negation.Should().Be(Negation.Not);
        result.As<Criterion>().Operation.Should().Be(Operation.In);
        result.As<Criterion>().Argument.Should().BeOfType<List<object>>();
        result.As<Criterion>().Argument.As<List<object>>().Should()
            .ContainInOrder([Radix.Decimal, Radix.Octal, Radix.Binary]);
    }

    [Test]
    public void Parse_CriterionCollectionOperation_ShouldBeExpected()
    {
        var result = TypeGroup.Parse("Members Is Any TagName Is Containing This is some text");

        result.Should().BeOfType<Criterion>();
        result.As<Criterion>().Property.Should().Be("Members");
        result.As<Criterion>().Negation.Should().Be(Negation.Is);
        result.As<Criterion>().Operation.Should().Be(Operation.Any);
        result.As<Criterion>().Argument.Should().BeOfType<Criterion>();
        result.As<Criterion>().Argument.As<Criterion>().Property.Should().Be("TagName");
        result.As<Criterion>().Argument.As<Criterion>().Negation.Should().Be(Negation.Is);
        result.As<Criterion>().Argument.As<Criterion>().Operation.Should().Be(Operation.Containing);
        result.As<Criterion>().Argument.As<Criterion>().Argument.Should().Be("This is some text");
    }

    [Test]
    public void Parse_CriterionInvalidPropertyName_ShouldWorkButNotParseValue()
    {
        var result = TypeGroup.Parse("WTF Not Equal To Something");

        result.Should().BeOfType<Criterion>();
        result.As<Criterion>().Property.Should().Be("WTF");
        result.As<Criterion>().Negation.Should().Be(Negation.Not);
        result.As<Criterion>().Operation.Should().Be(Operation.EqualTo);
        result.As<Criterion>().Argument.Should().Be("Something");
    }

    [Test]
    public void Parse_CriterionInvalidNegation_ShouldReturnString()
    {
        var result = TypeGroup.Parse("TagName IsNot Equal To Something");

        result.Should().BeOfType<string>();
        result.Should().Be("TagName IsNot Equal To Something");
    }

    [Test]
    public void Parse_CriterionInvalidOperation_ShouldThrowException()
    {
        var result = TypeGroup.Parse("TagName Is Whatever Something");

        result.Should().BeOfType<string>();
        result.Should().Be("TagName Is Whatever Something");
    }

    #endregion
}