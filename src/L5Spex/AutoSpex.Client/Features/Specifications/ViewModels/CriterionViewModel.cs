using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Engine.Operations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Features.Specifications;

public partial class CriterionViewModel : ViewModelBase
{
    private readonly Element _element;

    public CriterionViewModel(Element element)
    {
        _element = element;
    }

    public CriterionViewModel(dynamic record)
    {
        _element = Element.FromName(record.Element);
        Property = record.Property;
        Operation = Engine.Operations.Operation.FromName(record.Operation);
        Property = record.Property;
    }

    [ObservableProperty] [Required] private string _property = string.Empty;

    [ObservableProperty] [Required] private Operation _operation = Operation.EqualTo;

    [ObservableProperty] private ObservableCollection<object> _arguments = new();

    [ObservableProperty] private ChainType _chainType = ChainType.And;
    

    public Criterion GenerateCriterion()
    {
        return new Criterion(_element, Property, Operation, Arguments.ToList());
    }
}