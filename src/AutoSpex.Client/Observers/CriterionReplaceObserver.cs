﻿using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using FluentResults;

namespace AutoSpex.Client.Observers;

public class CriterionReplaceObserver(Criterion model, Node node) : Observer<Criterion>(model)
{
    /// <summary>
    /// The name of the node/spec that contains the criterion.
    /// </summary>
    public override string Name => Node.Name;

    /// <summary>
    /// The node that contains the criterion instance this observer wraps.
    /// </summary>
    public Node Node { get; } = node;

    /// <summary>
    /// The textual representation of the criterion.
    /// </summary>
    public string Criteria => Model.ToString();

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        FilterText = filter;
        return !string.IsNullOrEmpty(filter) && Criteria.Satisfies(filter);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="search"></param>
    /// <param name="replace"></param>
    /// <returns></returns>
    public Result Replace(string search, string replace)
    {
        var updated = Criteria.Replace(search, replace, StringComparison.OrdinalIgnoreCase);

        try
        {
            var parsed = Criterion.Parse(Model.Type, updated);
            Model.Property = parsed.Property;
            Model.Negation = parsed.Negation;
            Model.Operation = parsed.Operation;
            Model.Argument = parsed.Argument;
            return Result.Ok();
        }
        catch (FormatException e)
        {
            return Result.Fail(new ExceptionalError(e));
        }
    }
}