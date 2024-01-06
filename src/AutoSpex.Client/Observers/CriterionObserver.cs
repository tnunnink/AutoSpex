using System.ComponentModel.DataAnnotations;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Observers;

public class CriterionObserver : ObservableValidator
{
    private readonly Criterion _criterion;

    private CriterionObserver(Criterion criterion)
    {
        _criterion = criterion;
    }

    [Required]
    public string? Property
    {
        get => _criterion.Property;
        set => SetProperty(_criterion.Property, value, _criterion, (c, p) => c.Property = p, validate: true);
    }

    public static implicit operator CriterionObserver(Criterion criterion) => new(criterion);
}