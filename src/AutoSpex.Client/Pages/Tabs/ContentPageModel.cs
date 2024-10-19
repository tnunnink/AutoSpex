using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using L5Sharp.Core;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class ContentPageModel : PageViewModel,
    IRecipient<ElementObserver.ShowProperties>,
    IRecipient<Observer.GetSelected>
{
    private readonly SourceObserver _source;
    private readonly L5X? _content;

    /// <inheritdoc/>
    public ContentPageModel(SourceObserver source) : base("Content")
    {
        _source = source;
        _content = _source.Model.Content;
    }

    public override string Route => $"{nameof(Source)}/{_source.Id}/{Title}";

    [ObservableProperty] private Element _type = Element.Tag;

    [ObservableProperty] private ElementObserver? _selectedElement;

    [ObservableProperty] private string? _elementFilter;

    [ObservableProperty] private string? _propertyFilter;

    [ObservableProperty] private int _pageSize = 100;

    [ObservableProperty] private bool _showProperties = true;
    public ObservableCollection<ElementObserver> Elements { get; } = [];
    public ObserverCollection<Property, PropertyObserver> Properties { get; } = [];

    public override async Task Load()
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

    [RelayCommand]
    private void ToggleProperties()
    {
        ShowProperties = !ShowProperties;
    }

    /// <summary>
    /// Handles the show properties message by setting the selected element and toggling the properties view.
    /// </summary>
    public void Receive(ElementObserver.ShowProperties message)
    {
        var target = Elements.FirstOrDefault(e => e.Is(message.Element));
        if (target is null) return;
        SelectedElement = target;
        ShowProperties = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Receive(Observer.GetSelected message)
    {
        if (message.Observer is not ElementObserver observer) return;
        if (!Elements.Any(e => e.Is(observer))) return;
        if (SelectedElement is null) return;
        message.Reply(SelectedElement);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(Type) or nameof(ElementFilter):
                UpdateElements();
                break;
            case nameof(SelectedElement) or nameof(PropertyFilter):
                UpdateProperties();
                break;
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
            if (_content is null) return;

            var element = Type;
            var filter = ElementFilter;
            var size = PageSize;

            //Whether this is a tag name or not, we can treat is as such to easily parse the string.
            var tagName = !string.IsNullOrEmpty(filter) ? new TagName(filter) : TagName.Empty;
            var root = tagName.Root;

            var results = _content.Query(element.Type)
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

    /// <summary>
    /// Updates the <see cref="Properties"/> collection using the selected element and current property filter text.
    /// </summary>
    private void UpdateProperties()
    {
        //Get the properties from the selected element, or empty list of no selection is made.
        var properties = SelectedElement?.Properties.ToList() ?? [];

        //Apply filter to resulting properties.
        var filtered = properties.Where(p => p.Filter(PropertyFilter));

        //Update the bound collection with results.
        Properties.Refresh(filtered);
    }
}