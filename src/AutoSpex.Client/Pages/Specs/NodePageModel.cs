using System;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages.Specs;

public partial class NodePageModel : PageViewModel
{
    public NodePageModel(NodeObserver node)
    {
        Node = node ?? throw new ArgumentNullException(nameof(node));
    }

    public override Task Load()
    {
        return Task.CompletedTask;
        /*Dispatcher.UIThread.Invoke(() => Loading = true);
        var result = await Mediator.Send(new GetFullNode(Node.NodeId)).ConfigureAwait(true);
        if (result.IsFailed) return;
        Node = result.Value;
        Dispatcher.UIThread.Invoke(() => Loading = false);*/
    }

    public override string Route => $"{nameof(NodePageModel)}/{Node.NodeId}";
    public override string Title => Node.Name;
    public override string Icon => Node.NodeType.Name;

    [ObservableProperty] private NodeObserver _node;

    public override bool Equals(object? obj) => obj is NodePageModel other && Node.Model == other.Node.Model;

    public override int GetHashCode() => Node.Model.GetHashCode();
}