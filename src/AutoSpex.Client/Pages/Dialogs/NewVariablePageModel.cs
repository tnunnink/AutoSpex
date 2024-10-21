using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class NewVariablePageModel(object? value) : PageViewModel
{
    private List<NodeObserver> _nodes = [];

    public override bool KeepAlive => false;

    [ObservableProperty] [Required] [NotifyDataErrorInfo] [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private NodeObserver? _node;

    [ObservableProperty] [Required] [NotifyDataErrorInfo] [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private string? _name;

    public Func<string?, CancellationToken, Task<IEnumerable<object>>> PopulateNodes => FindNodes;

    [RelayCommand]
    private void UpdateNode(object? input)
    {
        Node = input switch
        {
            NodeObserver observer => observer,
            _ => null
        };
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task Create(Window window)
    {
        if (string.IsNullOrEmpty(Name)) return;
        if (Node is null) return;

        var nodeId = Node.Id;
        var variable = new Variable(Name, value);

        var result = await Mediator.Send(new CreateVariable(nodeId, variable));
        if (Notifier.ShowIfFailed(result)) return;

        var observer = new VariableObserver(variable);
        Messenger.Send(new Observer.Created<VariableObserver>(observer));
        window.Close(observer);
    }

    private bool CanCreate() => !string.IsNullOrEmpty(Name) && Node is not null;

    public override async Task Load()
    {
        var result = await Mediator.Send(new ListNodes());
        _nodes = result.SelectMany(n => n.DescendantsAndSelf()).Select(node => new NodeObserver(node)).ToList();
    }

    private Task<IEnumerable<object>> FindNodes(string? filter, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Task.FromResult(Enumerable.Empty<object>());

        var nodes = _nodes.Where(n => n.Model.Route.Satisfies(filter)).Cast<object>();

        return Task.FromResult(nodes);
    }
}