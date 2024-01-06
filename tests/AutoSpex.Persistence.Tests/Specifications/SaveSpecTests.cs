using AutoSpex.Engine;
using MediatR;
using Arg = AutoSpex.Client.Features.Criteria.Arg;
using TestContext = AutoSpex.Persistence.Tests.TestContext;

namespace AutoSpex.Persistence.Tests.Specifications;

[TestFixture]
public class SaveSpecTests
{
    [Test]
    public async Task SaveSpec_SpecWithUpdatedElement_ShouldHaveSuccessResult()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = Resolve<IMediator>();

        var collection = NodeObserver.SpecCollection("MyCollection");
        await mediator.Send(new AddNodeRequest(collection));

        var node = collection.NewSpec("TestSpec");
        await mediator.Send(new AddNodeRequest(node));

        var specification = new SpecificationViewModel(node);
        specification.Element = Element.Controller;

        var result = await mediator.Send(new SaveSpecRequest(specification));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task SaveSpec_SpecWithUpdatedCriterion_ShouldHaveSuccessResult()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = Resolve<IMediator>();

        var collection = NodeObserver.SpecCollection("MyCollection");
        await mediator.Send(new AddNodeRequest(collection));

        var node = collection.NewSpec("TestSpec");
        await mediator.Send(new AddNodeRequest(node));

        var specification = new SpecificationViewModel(node);
        specification.Element = Element.Controller;

        var criterion = new CriterionViewModel(specification.Node.NodeId, specification.Element, CriterionUsage.Filter);
        criterion.PropertyName = "Name";
        criterion.Operation = Operation.Equal;
        criterion.Args.Add(new Arg(criterion.Property, "Test"));
        specification.Filters.Add(criterion);
        
        var result = await mediator.Send(new SaveSpecRequest(specification));

        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task SaveSpec_SpecWithManyCriterion_ShouldHaveSuccessResult()
    {
        using var context = new TestContext();
        context.BuildProject();
        var mediator = Resolve<IMediator>();

        var collection = NodeObserver.SpecCollection("MyCollection");
        await mediator.Send(new AddNodeRequest(collection));

        var node = collection.NewSpec("TestSpec");
        await mediator.Send(new AddNodeRequest(node));

        var specification = new SpecificationViewModel(node);
        specification.Element = Element.Controller;
        
        specification.Filters.Add(GenerateTestFilter(specification));
        specification.Filters.Add(GenerateTestFilter(specification));
        specification.Filters.Add(GenerateTestFilter(specification));
        
        var result = await mediator.Send(new SaveSpecRequest(specification));

        result.IsSuccess.Should().BeTrue();
    }

    private static CriterionViewModel GenerateTestFilter(SpecificationViewModel specification)
    {
        var criterion = new CriterionViewModel(specification.Node.NodeId, specification.Element, CriterionUsage.Filter);
        criterion.PropertyName = "Name";
        criterion.Operation = Operation.Equal;
        criterion.Args.Add(new Arg(criterion.Property, "Test"));
        return criterion;
    }
}