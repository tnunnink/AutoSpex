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
        if (param is not ValueObserver observer)
            return default;

        return observer.Value switch
        {
            RangeObserver => RangeTemplate?.Build(observer),
            ObserverCollection<object?, ValueObserver> => CollectionTemplate?.Build(observer),
            CriterionObserver criterion => CriterionTemplate?.Build(criterion),
            _ => DefaultTemplate?.Build(observer)
        };
    }

    public bool Match(object? data) => data is ValueObserver;
}