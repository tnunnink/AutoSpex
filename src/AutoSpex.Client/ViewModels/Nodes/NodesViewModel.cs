using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Messages;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.ViewModels;

[UsedImplicitly]
public partial class NodesViewModel : ViewModelBase, IRecipient<OpenNodeMessage>
{
    [ObservableProperty] private ObservableCollection<ViewModels.NodeDetailViewModel> _nodes = new();

    [ObservableProperty] private ViewModels.NodeDetailViewModel? _selected;

    public NodesViewModel()
    {
        Messenger.RegisterAll(this);
    }

    public void Receive(OpenNodeMessage message)
    {
        message.Node.Model.NodeType
            .When(NodeType.Collection).Then(() =>
            {
                var collection = new CollectionViewModel(message.Node);
                ShowNode(collection, message.InTab);
                Dispatcher.UIThread.Post(() => collection.InFocus = message.InFocus);
            })
            .When(NodeType.Folder).Then(() =>
            {
                var folder = new FolderViewModel(message.Node);
                ShowNode(folder, message.InTab);
                Dispatcher.UIThread.Post(() => folder.InFocus = message.InFocus);
            })
            .When(NodeType.Spec).Then(() =>
            {
                var spec = new SpecificationViewModel(message.Node);
                ShowNode(spec, message.InTab);
                Dispatcher.UIThread.Post(() => spec.InFocus = message.InFocus);
            })
            .When(NodeType.Source).Then(() =>
            {
                var source = new SourceViewModel(message.Node);
                ShowNode(source, message.InTab);
            })
            .Default(() => throw new ArgumentOutOfRangeException(nameof(message),
                $"The provided node type {message.Node.Model.NodeType} is not supported by the details view model."));
    }

    [RelayCommand]
    private void CloseNode(NodeDetailViewModel node)
    {
        if (node.IsChanged)
        {
            //todo prompt user.
        }
        
        Nodes.Remove(node);
        node.Dispose();
    }

    private void ShowNode(NodeDetailViewModel vm, bool newTab)
    {
        var existing = Nodes.SingleOrDefault(n => n.Node.Model.NodeId == vm.Node.Model.NodeId);
        if (existing is not null)
        {
            Selected = vm;
            return;
        }

        if (newTab)
        {
            Nodes.Add(vm);
            Selected = vm;
            return;
        }

        for (var i = 0; i < Nodes.Count; i++)
        {
            if (Nodes[i].IsChanged) continue;
            Nodes[i] = vm;
            Selected = Nodes[i];
            return;
        }

        Nodes.Add(vm);
        Selected = vm;
    }
}