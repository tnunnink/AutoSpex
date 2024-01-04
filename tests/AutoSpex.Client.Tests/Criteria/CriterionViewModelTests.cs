using System.Collections.ObjectModel;
using AutoSpex.Client.Features.Criteria;
using AutoSpex.Engine;
using AutoSpex.Engine.Operations;
using FluentAssertions;
using Arg = AutoSpex.Client.Features.Criteria.Arg;

namespace AutoSpex.Client.Tests.Criteria;

[TestFixture]
public class CriterionViewModelTests
{
    [Test]
    public void ToRecord_WhenCalled_ShouldNotBeNull()
    {
        using var context = new TestContext();
        
        var vm = new CriterionViewModel(Guid.NewGuid(), Element.Tag, CriterionUsage.Filter)
        {
            PropertyName = "Name",
            Operation = Operation.Equal,
            Args = new ObservableCollection<Arg>(new []{new Arg(Element.Tag.Property("Name"))})
        };

        var record = vm.ToRecord();

        record.Should().NotBeNull();
    }
}