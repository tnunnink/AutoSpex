using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using L5Sharp.Core;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SourceExplorerPageModel(Element element) : PageViewModel,
    IRecipient<ElementObserver.Open>
{
    public SourceExplorerPageModel() : this(Element.Tag)
    {
    }

    public override bool KeepAlive => false;

    [ObservableProperty] private SourceObserver? _source;

    [ObservableProperty] private Element _element = element;

    [ObservableProperty] private ElementObserver? _selectedElement;

    [ObservableProperty] private string? _filterText;

    [ObservableProperty] private int _pageSize = 100;

    public ObservableCollection<SourceObserver> Sources { get; } = [];
    public ObservableCollection<ElementObserver> Elements { get; } = [];

    /// <inheritdoc />
    public override async Task Load()
    {
        var result = await Mediator.Send(new ListAllSources());
        if (result.IsFailed) return;

        Sources.Refresh(result.Value.Select(s => new SourceObserver(s)));
        await ChangeSource(Sources.FirstOrDefault());
    }

    [RelayCommand]
    private async Task ChangeSource(SourceObserver? source)
    {
        if (source is null)
            return;

        if (Source is not null)
            Source.IsActive = false;

        await source.LoadContent();
        Source = source;
    }

    [RelayCommand]
    private void ChangeElement(Element? element)
    {
        if (element is null) return;
        Element = element;
    }

    public void Receive(ElementObserver.Open message)
    {
        SelectedElement = message.Element;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName is nameof(Source) or nameof(FilterText) or nameof(Element))
        {
            UpdateElements();
        }
    }

    /// <summary>
    /// Kicks off the async query element method and provides it with a cancellation of 10 seconds to prevent overly
    /// long processing times. In theory the page size limits should help precent cancellations.
    /// </summary>
    private async void UpdateElements()
    {
        var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        try
        {
            await QueryElements(cancellation.Token);
        }
        catch (OperationCanceledException)
        {
        }
    }
    
    private Task QueryElements(CancellationToken token)
    {
        return Task.Run(() =>
        {
            if (Source is null) return;

            //Source should already be loaded. This is just getting the reference to the loaded content.
            var content = Source.Model.Load();
            var element = Element;
            var filter = FilterText;
            var size = PageSize;

            //Whether this is a tag name or not, we can treat is as such to easily parse the string.
            var tagName = !string.IsNullOrEmpty(filter) ? new TagName(filter) : TagName.Empty;
            var root = tagName.Root;

            var results = content.Query(element.Type)
                .Select(e => new ElementObserver(e))
                .Where(x => x.Filter(root))
                .Take(size)
                .ToList();

            //If not a tag or the provided name is not a nested tag input, just return the top level results.
            if (element != Element.Tag || tagName.Depth < 1)
            {
                Dispatcher.UIThread.Invoke(() => Elements.Refresh(results));
                return;
            }

            //If a tag and the tag name has members, we want to return the results that match.
            var tags = results
                .Select(r => r.Model)
                .Where(x => x is Tag)
                .Cast<Tag>()
                .SelectMany(t => t.Members())
                .Where(t => t.TagName.Contains(tagName))
                .Take(size)
                .Select(t => new ElementObserver(t))
                .ToList();

            Dispatcher.UIThread.Invoke(() => Elements.Refresh(tags));
        }, token);
    }
}