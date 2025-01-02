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
using JetBrains.Annotations;
using Action = AutoSpex.Engine.Action;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class AddActionPageModel : PageViewModel
{
    private SpecObserver? _config;

    public override bool KeepAlive => false;

    // ReSharper disable once MemberCanBeMadeStatic.Global this is a bound property
#pragma warning disable CA1822
    public IEnumerable<ActionType> Actions => ActionType.List.OrderBy(x => x.Value);
#pragma warning restore CA1822

    [ObservableProperty] [Required] [NotifyDataErrorInfo] [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private ActionType _type = ActionType.Suppress;

    [ObservableProperty] [Required] [NotifyDataErrorInfo] [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private NodeObserver? _node;

    [ObservableProperty] [Required] [NotifyDataErrorInfo] [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private string? _reason;

    public Func<string?, CancellationToken, Task<IEnumerable<object>>> PopulateNodes => FindNodes;

    [RelayCommand]
    private async Task UpdateNode(object? input)
    {
        Node = input switch
        {
            NodeObserver observer => observer,
            _ => null
        };

        var spec = Node is not null ? (await Mediator.Send(new LoadSpec(Node.Id))).ValueOrDefault : default;
        _config = spec is not null ? new SpecObserver(spec) : default;
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add(Window? window)
    {
        if (Node is null) return;
        if (string.IsNullOrEmpty(Reason)) return;

        var rule = new Action(Node.Id, Type, Reason, _config?.Model);

        window?.Close(new ActionObserver(rule));
    }

    private bool CanAdd() => Node is not null && !string.IsNullOrEmpty(Reason);

    private async Task<IEnumerable<object>> FindNodes(string? filter, CancellationToken token)
    {
        var message = Messenger.Send(new Observer.Find<NodeObserver>(n => n.Type == NodeType.Spec));
        var nodes = await message.GetResponsesAsync(token);
        return nodes.Where(n => n.Filter(filter));
    }
}