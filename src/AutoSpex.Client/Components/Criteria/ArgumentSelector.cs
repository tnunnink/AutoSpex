using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace AutoSpex.Client.Components;

public class ArgumentSelector : IDataTemplate
{
    public IDataTemplate? DefaultTemplate { get; set; }
    public IDataTemplate? RangeTemplate { get; set; }
    public IDataTemplate? CollectionTemplate { get; set; }
    public IDataTemplate? CriterionTemplate { get; set; }

    public Control? Build(object? param)
    {
        if (param is not ArgumentInput input)
            return default;

        return input.Value switch
        {
            RangeObserver => RangeTemplate?.Build(input),
            ObserverCollection<object?, ValueObserver> => CollectionTemplate?.Build(input),
            CriterionObserver criterion => CriterionTemplate?.Build(criterion),
            _ => DefaultTemplate?.Build(input)
        };
    }

    public bool Match(object? data) => data is ArgumentInput;
}