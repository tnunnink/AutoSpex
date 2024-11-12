using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;
using L5Sharp.Core;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class ContentPageModel(SourceObserver source) : PageViewModel("Content")
{
    private static readonly HashSet<string> ScopeTypes = ScopeType.All().Select(x => x.Name).ToHashSet();

    public override string Route => $"{nameof(Source)}/{source.Id}/{Title}";
    public ObserverCollection<LogixElement, ElementObserver> Elements { get; } = [];

    [ObservableProperty] private Element _type = Element.Tag;

    [ObservableProperty] private ElementObserver? _selected;

    [ObservableProperty] private int _pageSize = 50;

    /// <inheritdoc />
    protected override async void FilterChanged(string? filter)
    {
        await Search();
    }

    /// <summary>
    /// Kicks off the async query of elements and provides it with a cancellation of 10 seconds to prevent overly
    /// long processing times. In theory the page size limits should help prevent cancellations. We should jsut build in a cancel button, but most queries probably won't take more than 10 seconds.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task Search()
    {
        var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        try
        {
            var elements = await QueryElements(cancellation.Token);
            Elements.Refresh(elements);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            Notifier.ShowError("Failed to search content", e.Message);
        }
    }

    private bool CanSearch() => !string.IsNullOrEmpty(Filter) && source.Model.Content is not null;

    private Task<IEnumerable<ElementObserver>> QueryElements(CancellationToken token)
    {
        return Task.Run(() =>
        {
            if (source.Model.Content is null || string.IsNullOrEmpty(Filter)) return [];

            var content = source.Model.Content;
            var filter = Filter;
            var size = PageSize;

            var elements = content.Controller.Serialize().Descendants()
                .Where(e => e.Attributes().Any(a => a.Value.Satisfies(filter)) || e.Value.Satisfies(filter))
                .Select(e => e.AncestorsAndSelf().FirstOrDefault(a => ScopeTypes.Contains(a.Name.LocalName)))
                .Distinct()
                .Where(e => e is not null)
                .Cast<XElement>()
                .Select(e => e.Deserialize<LogixElement>())
                .Take(size);

            return elements.Select(e => new ElementObserver(e) { FilterText = filter });
        }, token);
    }
}