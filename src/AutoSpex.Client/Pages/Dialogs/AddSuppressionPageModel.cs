using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class AddSuppressionPageModel : PageViewModel
{
    public override bool KeepAlive => false;

    [ObservableProperty] [Required] [NotifyDataErrorInfo] [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private NodeObserver? _node;

    [ObservableProperty] [Required] [NotifyDataErrorInfo] [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private string? _reason;

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

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add(Window? window)
    {
        if (Node is null) return;
        if (string.IsNullOrEmpty(Reason)) return;

        var suppression = new SuppressionObserver(new Suppression(Node.Id, Reason));
        window?.Close(suppression);
    }

    private bool CanAdd() => Node is not null && !string.IsNullOrEmpty(Reason);

    private async Task<IEnumerable<object>> FindNodes(string? filter, CancellationToken token)
    {
        var message = Messenger.Send(new Observer.Find<NodeObserver>(n => n.Type == NodeType.Spec));
        var nodes = await message.GetResponsesAsync(token);
        return nodes.Where(n => n.Filter(filter));
    }
}