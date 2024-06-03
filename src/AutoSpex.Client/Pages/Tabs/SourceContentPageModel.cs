using System.Collections.ObjectModel;
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

namespace AutoSpex.Client.Pages;

public partial class SourceContentPageModel(NodeObserver node) : DetailPageModel,
    IRecipient<ElementObserver.View>,
    IRecipient<CriterionObserver.Deleted>
{
    public override string Route => $"Source/{node.Id}/{Title}";
    public override string Title => "Content";

    [ObservableProperty] private SourceObserver _source = SourceObserver.Empty(node.Id);

    [ObservableProperty] private string? _filter = string.Empty;

    [ObservableProperty] private bool _searching;

    [ObservableProperty] private int _pageSize = 1000;

    [ObservableProperty] private bool _isDrawerOpen;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(SearchContentCommand))]
    private Element _element = Element.Default;

    public ObservableCollection<ElementObserver> Elements { get; } = [];

    [ObservableProperty] private ElementObserver? _selectedElement;
    public ObservableCollection<CriterionObserver> Filters { get; } = [];
    [ObservableProperty] private Inclusion _filterInclusion = Inclusion.All;

    public void Receive(ElementObserver.View message)
    {
        if (message.Element.SourceId != node.Id) return;
        SelectedElement = message.Element;
        IsDrawerOpen = true;
    }

    public void Receive(Observer<Criterion>.Deleted message)
    {
        if (message.Observer is not CriterionObserver criterion) return;
        if (!Filters.Contains(criterion)) return;
        Filters.Remove(criterion);
    }

    public override async Task Load()
    {
        var result = await Mediator.Send(new GetSource(node.Id));
        if (result.IsFailed) return;
        Source = default!; //Need to set null for next set to work because we implement equality based on id.
        Source = new SourceObserver(result.Value);
        SearchContentCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task LoadContent()
    {
        await Prompter.Show(() => new LoadSourcePageModel(Source));
    }

    [RelayCommand(IncludeCancelCommand = true, CanExecute = nameof(CanSearchContent))]
    private async Task SearchContent(CancellationToken token)
    {
        if (!Source.HasContent || Element == Element.Default) return;
        Elements.Clear();
        Searching = true;
        await ExecuteSearch(token);
        Searching = false;
    }

    private bool CanSearchContent() => Source.HasContent && Element != Element.Default;

    private Task ExecuteSearch(CancellationToken token)
    {
        var page = PageSize;
        var filters = Filters.Select(f => f.Model).ToList();
        var inclusion = FilterInclusion;
        var filter = Filter;
        var elements = Element.Query(Source.Model.L5X);

        return Task.Run(() =>
        {
            var number = 0;
            foreach (var element in elements)
            {
                if (token.IsCancellationRequested || number > page) break;

                var include = inclusion == Inclusion.All
                    ? filters.All(f => f.Evaluate(element))
                    : filters.Any(f => f.Evaluate(element));
                if (!include) continue;

                var pass = string.IsNullOrEmpty(filter) || element.Serialize().ToString().ContainsText(filter);
                if (!pass) continue;
                
                var observer = new ElementObserver(element);
                Dispatcher.UIThread.Post(() => Elements.Add(observer));
                number++;
            }
        }, token);
    }

    /// <summary>
    /// Reset the search when the selected element changes.
    /// </summary>
    /// <param name="value"></param>
    // ReSharper disable once UnusedParameterInPartialMethod
    partial void OnElementChanged(Element value)
    {
        Filters.Clear();
        Filter = string.Empty;
        Elements.Clear();
    }
}