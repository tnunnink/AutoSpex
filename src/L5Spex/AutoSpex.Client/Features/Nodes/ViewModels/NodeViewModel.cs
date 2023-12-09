﻿using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Features.Nodes;

public abstract partial class NodeViewModel : ViewModelBase
{
    protected NodeViewModel(Node node, Control view)
    {
        Node = node;
        View = view;
        View.DataContext = this;
    }

    [ObservableProperty] private Node _node;

    [ObservableProperty] private Control _view;

    [RelayCommand]
    protected abstract Task Save();

    public override bool Equals(object? obj) => obj is NodeViewModel other && Node.NodeId == other.Node.NodeId;

    public override int GetHashCode() => Node.NodeId.GetHashCode();
}