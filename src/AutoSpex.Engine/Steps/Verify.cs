﻿namespace AutoSpex.Engine;

/// <summary>
/// Represents a step in the automated testing process which verifies the input objects based on a set of criteria.
/// </summary>
public class Verify : Step
{
    /// <summary>
    /// Creates a new default <see cref="Verify"/> step with default values.
    /// </summary>
    public Verify()
    {
    }
    
    /// <summary>
    /// Creates a new default <see cref="Filter"/> step initialized with a criterion defined by the provided parameters.
    /// </summary>
    public Verify(string property, Operation operation, object? argument = default)
    {
        Criteria.Add(new Criterion(property, operation, argument));
    }
    
    /// <inheritdoc />
    public override IEnumerable<object> Process(IEnumerable<object?> input)
    {
        var evaluations = new List<Evaluation>();

        foreach (var item in input)
        {
            var results = Criteria.Select(x => x.Evaluate(item));
            evaluations.AddRange(results);
        }

        return evaluations;
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// Technically this step always returns an <see cref="Evaluation"/> object. At this point we don't totally care
    /// about the return type, but we implement anyway.
    /// </remarks>
    public override Property Returns(Property input)
    {
        return Property.This(typeof(Evaluation));
    }
}