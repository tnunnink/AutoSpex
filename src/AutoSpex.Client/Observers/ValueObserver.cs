using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

/// <summary>
/// A simple object observer that wraps a object value and adds some type and formatted UI friendly text for the
/// value.
/// </summary>
/// <param name="value">The object value to wrap.</param>
public class ValueObserver(object? value) : Observer
{
    /// <summary>
    /// The internal model value of the observer instance.
    /// </summary>
    public object? Model { get; } = value;

    /// <summary>
    /// The value of the observer instance. 
    /// </summary>
    /// <remarks>
    /// If the underlying value is a <see cref="Variable"/> or <see cref="Criterion"/>, then this value return a wrapped
    /// observer instance of those types. Any other type is the direct value.
    /// </remarks>
    public object? Value => GetValue();

    /// <summary>
    /// The text display for the value. This should be a common name that can be used to identify the value.
    /// </summary>
    public override string Name => Model.ToText();

    /// <summary>
    /// The user-friendly type name of the value.
    /// </summary>
    public string Type => GetTypeName();

    /// <summary>
    /// The <see cref="TypeGroup"/> in which this value belongs. 
    /// </summary>
    public TypeGroup Group => GetTypeGroup();

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        return base.Filter(filter)
               || Name.Satisfies(filter)
               || Type.Satisfies(filter)
               || Group.Name.Satisfies(filter);
    }

    private object? GetValue()
    {
        return Model switch
        {
            Variable variable => new VariableObserver(variable),
            Criterion criterion => new CriterionObserver(criterion),
            List<Argument> arguments => arguments.Select(a => new ArgumentObserver(a)),
            _ => Model
        };
    }

    private string GetTypeName()
    {
        var type = Model switch
        {
            Variable variable => variable.GetType(),
            Criterion criterion => criterion.GetType(),
            _ => Model?.GetType()
        };

        return type?.CommonName() ?? string.Empty;
    }

    private TypeGroup GetTypeGroup()
    {
        var type = Model switch
        {
            Variable variable => variable.GetType(),
            Criterion criterion => criterion.GetType(),
            _ => Model?.GetType()
        };

        return TypeGroup.FromType(type);
    }
}