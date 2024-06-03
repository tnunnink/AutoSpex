using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;

namespace AutoSpex.Client.Pages;

public partial class NodeVariablesPageModel : DetailPageModel,
    IRecipient<VariableObserver.Deleted>,
    IRecipient<VariableObserver.GetNames>
{
    private readonly NodeObserver _node;

    /// <inheritdoc/>
    public NodeVariablesPageModel(NodeObserver node)
    {
        _node = node;
        Selected.CollectionChanged += OnSelectedVariablesChanged;
    }

    public override string Route => $"{_node.Type}/{_node.Id}/{Title}";
    public override string Title => "Variables";
    public ObserverCollection<Variable, VariableObserver> Variables { get; } = [];
    public ObservableCollection<VariableObserver> Selected { get; } = [];

    public override async Task Load()
    {
        var result = await Mediator.Send(new GetNodeVariables(_node.Id));
        if (result.IsFailed) return;
        Variables.Refresh(result.Value.Select(v => new VariableObserver(v, _node)));
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

    [RelayCommand(CanExecute = nameof(VariablesSelected))]
    private async Task CopyVariables()
    {
        var clipboard = Shell.Clipboard;
        if (clipboard is null) return;

        var selected = Selected.ToList();

        var data = new DataObject();
        data.Set("Variables", selected);

        await clipboard.SetDataObjectAsync(data);
    }

    [RelayCommand(CanExecute = nameof(VariablesSelected))]
    private void DeleteVariables()
    {
        var selected = Selected.ToList();

        foreach (var observer in selected)
            Variables.Remove(observer);
    }

    private bool VariablesSelected() => Selected.Count > 0;

    public void Receive(Observer<Variable>.Deleted message)
    {
        if (message.Observer is not VariableObserver variable) return;
        Variables.Remove(variable);
    }

    public void Receive(VariableObserver.GetNames message)
    {
        if (message.Variable.Node?.Id != _node.Id) return;
        var names = Variables.Where(v => v.Id != message.Variable.Id && v.Name is not null).Select(v => v.Name!);
        message.Reply(names);
    }

    private void OnSelectedVariablesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        DeleteVariablesCommand.NotifyCanExecuteChanged();
        CopyVariablesCommand.NotifyCanExecuteChanged();
    }
}