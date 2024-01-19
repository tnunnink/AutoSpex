using System.ComponentModel.DataAnnotations;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Components;

public class CriterionObserver(Criterion criterion) : Observer<Criterion>(criterion)
{
    [Required]
    public string? Property
    {
        get => Model.Property;
        set => SetProperty(Model.Property, value, Model, (c, p) => c.Property = p, validate: true);
    }

    public static implicit operator CriterionObserver(Criterion criterion) => new(criterion);
}