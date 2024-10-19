using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class VariablesPageModel : PageViewModel,
    IRecipient<Observer.Renamed>,
    IRecipient<Observer.Deleted>,
    IRecipient<Observer.MakeCopy>,
    IRecipient<Observer.GetSelected>
{
    private readonly NodeObserver _node;

    /// <inheritdoc/>
    public VariablesPageModel(NodeObserver node) : base("Variables")
    {
        _node = node;

        Variables = new ObserverCollection<Variable, VariableObserver>(
            refresh: () => _node.Model.Variables.Select(v => new VariableObserver(v)).ToList(),
            add: (_, v) => _node.Model.AddVariable(v),
            remove: (_, v) => _node.Model.RemoveVariable(v),
            clear: () => _node.Model.ClearVariables(),
            count: () => _node.Model.Variables.Count()
        );
        Variables.Sort(x => x.Name, StringComparer.OrdinalIgnoreCase);
        Track(Variables);
    }

    public override string Route => $"{_node.Type}/{_node.Id}/{Title}";
    public override string Icon => Title;
    public ObserverCollection<Variable, VariableObserver> Variables { get; }
    public ObservableCollection<VariableObserver> Selected { get; } = [];

    [ObservableProperty] private string? _filter;

    /// <summary>
    /// Command to add a new variable to the node. Perform sort after to ensure proper order.
    /// </summary>
    [RelayCommand]
    private void AddVariable()
    {
        Variables.Add(new VariableObserver(new Variable()));
    }

    /// <summary>
    /// Handle reception of the delete message. If the observer is a variable remove any instance with the matching id.
    /// </summary>
    public void Receive(Observer.Deleted message)
    {
        if (message.Observer is not VariableObserver variable) return;
        Variables.RemoveAny(v => v.Id == variable.Id);
    }

    /// <summary>
    /// Handle reception of messages to make a copy of a given spec instance. Check that the instance comes from
    /// this node object, and that it is indeed a spec, and then create and add the duplicate.
    /// </summary>
    public void Receive(Observer.MakeCopy message)
    {
        if (message.Observer is not VariableObserver variable) return;
        if (!Variables.Any(x => x.Is(variable))) return;

        var duplicate = new VariableObserver(variable.Model.Duplicate());
        Variables.Add(duplicate);
        Variables.Sort(x => x.Name, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Handle reception of the get selected variables message. If this page contains the instance return the bound
    /// selected collection of variables.
    /// </summary>
    public void Receive(Observer.GetSelected message)
    {
        if (message.Observer is not VariableObserver variable) return;
        if (!Variables.Any(v => v.Is(variable))) return;

        foreach (var selected in Selected.ToList())
            message.Reply(selected);
    }

    /// <summary>
    /// Handle variable renaming by resotring the variables collection to show all in alphabetical order.
    /// </summary>
    public void Receive(Observer.Renamed message)
    {
        if (message.Observer is not VariableObserver observer) return;
        if (!Variables.Contains(observer)) return;
        Variables.Sort(x => x.Name, StringComparer.OrdinalIgnoreCase);
    }
}