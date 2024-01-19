using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Components.Sources;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SourcesPageModel : PageViewModel
{
    private readonly List<SourceObserver> _all = [];
    private readonly SourceList<SourceObserver> _cache = new();
    private readonly ReadOnlyObservableCollection<SourceObserver> _sources;
    
    public SourcesPageModel()
    {
        
    }

    public override string Title => "Sources";
    public override string Icon => "Sources";

    public ReadOnlyObservableCollection<SourceObserver> Sources => _sources;

    [ObservableProperty] private SourceObserver? _selected;

    [ObservableProperty] private string _filter = string.Empty;

    protected override async Task Load()
    {
        var result = await Mediator.Send(new ListSources());

        if (result.IsSuccess)
        {
            _all.Clear();
            _all.AddRange(result.Value.Select(s => new SourceObserver(s)));
            _cache.AddRange(_all.ToArray());
        }
    }
}