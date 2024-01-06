namespace AutoSpex.Engine.Tests.Specifications;

[TestFixture]
public class SpecificationTests
{
    [Test]
    public void For_TagElement_ShouldHaveExpectedValues()
    {
        var spec = Specification.For(Element.Tag);

        spec.Element.Should().Be(Element.Tag);
        spec.Filters.Should().BeEmpty();
        spec.Verifications.Should().BeEmpty();
        spec.Range.Operation.Should().Be(Operation.GreaterThan);
        spec.Range.Arguments.Should().NotBeEmpty();
        spec.Options.Should().NotBeNull();

        spec.Verify(c =>
        {
            c.Property = "Name";
            c.Operation = Operation.In;
            c.Arguments = ["Test", "another", "something"];
        });

        var test = spec.Verify();
    }
}