using System.Collections.Generic;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

// ReSharper disable ConvertIfStatementToSwitchStatement

namespace AutoSpex.Client.Components;

public class ArgumentSelector : Dictionary<string, IDataTemplate>, IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is not ArgumentObserver argument) return default;

        var criterion = argument.Criterion;
        if (criterion is null || criterion.Operation is UnaryOperation) return default;

        if (criterion.Operation is BinaryOperation)
        {
            return this[nameof(BinaryOperation)].Build(argument);
        }

        if (criterion.Operation is TernaryOperation)
        {
            return this[nameof(TernaryOperation)].Build(argument);
        }

        if (criterion.Operation is CollectionOperation)
        {
            return this[nameof(CollectionOperation)].Build(argument);
        }

        if (criterion.Operation is InOperation)
        {
            return this[nameof(InOperation)].Build(argument);
        }

        return default;
    }

    public bool Match(object? data) => data is ArgumentObserver;
}