using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;

namespace AutoSpex.Client.Pages;

public partial class VariablesPageModel : PageViewModel,
    IRecipient<Observer.GetSelected>,
    IRecipient<Observer.Deleted>,
    IRecipient<VariableObserver.GetNames>
{
    private readonly NodeObserver _node;

    /// <inheritdoc/>
    public VariablesPageModel(NodeObserver node)
    {
        _node = node;
    }

    public override string Route => $"{_node.Type}/{_node.Id}/{Title}";
    public override string Title => "Variables";
    public ObserverCollection<Variable, VariableObserver> Variables { get; } = [];
    public ObservableCollection<VariableObserver> Selected { get; } = [];

    public override async Task Load()
    {
        var result = await Mediator.Send(new GetNodeVariables(_node.Id));
        if (result.IsFailed) return;
        Variables.Refresh(result.Value.Select(v => new VariableObserver(v)));
        Track(Variables);
    }

    public override Task<Result> Save()
    {
        var variables = Variables.Select(v => v.Model);
        return Mediator.Send(new SaveVariables(_node.Id, variables));
    }

    [RelayCommand]
    private void AddVariable()
    {
        Variables.Add(new VariableObserver(_node));
    }

    public void Receive(Observer.Deleted message)
    {
        if (message.Observer is not VariableObserver variable) return;
        Variables.Remove(variable);
    }

    public void Receive(VariableObserver.GetNames message)
    {
        if (message.HasReceivedResponse) return;
        if (message.Variable.Node?.Id != _node.Id) return;
        var names = Variables.Where(v => v.Id != message.Variable.Id).Select(v => v.Name);
        message.Reply(names);
    }

    public void Receive(Observer.GetSelected message)
    {
        if (message.Observer is not VariableObserver variable) return;
        if (!Variables.Any(v => v.Is(variable))) return;

        foreach (var selected in Selected.ToList())
            message.Reply(selected);
    }
}