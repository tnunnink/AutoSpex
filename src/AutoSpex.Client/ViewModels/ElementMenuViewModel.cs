using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Messages;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using JetBrains.Annotations;

namespace AutoSpex.Client.ViewModels;

[UsedImplicitly]
public partial class ElementMenuViewModel : ViewModelBase
{
    private readonly Guid _nodeId;
    private readonly SourceList<Element> _source = new();
    private readonly ReadOnlyObservableCollection<Element> _elements;

    public ElementMenuViewModel(Guid nodeId)
    {
        _nodeId = nodeId;
        
        _source.Connect()
            .Sort(SortExpressionComparer<Element>.Ascending(t => t.Name))
            .Bind(out _elements)
            .Subscribe();
        
        _source.AddRange(Engine.Element.List.Where(e => e.IsComponent));
    }

    public ReadOnlyObservableCollection<Element> Elements => _elements;
    
    [ObservableProperty] private Element? _element;

    [ObservableProperty] private string _filter = string.Empty;

    [ObservableProperty] private bool _showingAll;
    
    [ObservableProperty] private bool _filtered;

    [RelayCommand]
    private void ShowAll()
    {
        _source.Clear();
        _source.AddRange(Element.List);
        ShowingAll = true;
    }
    
    [RelayCommand]
    private void ShowComponents()
    {
        _source.Clear();
        _source.AddRange(Element.List.Where(e => e.IsComponent));
        ShowingAll = false;
    }

    partial void OnFilterChanged(string value)
    {
        _source.Clear();
        
        if (!string.IsNullOrEmpty(value))
        {
            _source.AddRange(Element.List.Where(e => e.Name.Contains(value, StringComparison.OrdinalIgnoreCase)));
            Filtered = true;
            return;
        }
        
        _source.AddRange(Element.List.Where(e => e.IsComponent));
        Filtered = false;
        ShowingAll = false;
    }

    partial void OnElementChanged(Element? value)
    {
        Messenger.Send(new ElementSelectedMessage(value), _nodeId);
    }
}