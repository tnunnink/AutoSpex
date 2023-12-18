using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Features.Nodes;

public partial class NodeNameViewModel : DialogViewModel
{
    public NodeNameViewModel(string title, NodeType nodeType, string? name = null)
    {
        Title = title;
        NodeType = nodeType;
        Name = name ?? string.Empty;
    }
    
    [ObservableProperty] private string _title;
    
    [ObservableProperty] private NodeType _nodeType;
    
    [ObservableProperty] private string _name;
}