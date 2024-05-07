﻿using System;
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
using JetBrains.Annotations;
using L5Sharp.Core;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SourcePageModel : DetailPageModel,
    IRecipient<SourceObserver.Deleted>,
    IRecipient<SourceObserver.Renamed>
{
    private readonly Guid _sourceId;
    private CancellationTokenSource? _cancellation;

    /// <inheritdoc/>
    public SourcePageModel(Guid sourceId)
    {
        _sourceId = sourceId;
    }

    public override string Route => $"Source/{_sourceId}";
    public override string Title => Source?.Name ?? "Not Found";
    public override string Icon => "Source";

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Title))]
    private SourceObserver? _source;
    
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private Element _element = Element.Default;

    [ObservableProperty] private LogixElement? _selectedResult;

    [ObservableProperty] private ElementObserver? _selectedElement;

    [ObservableProperty] private bool _searching;

    [ObservableProperty] private Inclusion _inclusion = Inclusion.All;
    public ObservableCollection<LogixElement> Results { get; } = [];
    public ObservableCollection<CriterionObserver> Filters { get; } = [];

    /// <summary>
    /// When the selected result from the <see cref="Results"/> collection changes update the <see cref="SelectedElement"/>
    /// as a wrapped element so we can view the property data accordingly.
    /// </summary>
    /// <param name="value">The element to wrap.</param>
    partial void OnSelectedResultChanged(LogixElement? value)
    {
        if (value is not null)
        {
            SelectedElement = new ElementObserver(value);
        }
    }

    public override async Task Load()
    {
        var result = await Mediator.Send(new GetSource(_sourceId));
        if (result.IsFailed) return;

        Source = new SourceObserver(result.Value);
    }

    public void Receive(Observer<Source>.Deleted message)
    {
        if (message.Observer.Id != _sourceId) return;
        ForceClose();
    }

    public void Receive(NamedObserver<Source>.Renamed message)
    {
        if (message.Observer is not SourceObserver source) return;
        if (source.Id != _sourceId) return;
        OnPropertyChanged(nameof(Title));
    }
    
    /// <summary>
    /// Execute the current source search and populate the <see cref="Results"/> collection with the returned elements.
    /// This method will update the UI so we can know we are searching. It will also initialize the cancellation token
    /// so the user can cancel the search if it takes too long. The only process intensive searched right now are tags
    /// but we will working on fixing that hopefully.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task Search()
    {
        Searching = true;
        Results.Clear();
        _cancellation = new CancellationTokenSource();

        try
        {
            await ExecuteSearch(_cancellation.Token);
        }
        catch (Exception e)
        {
            //todo notify
            //todo logging
            Console.WriteLine(e);
        }
        finally
        {
            SearchCleanup();
        }
    }

    /// <summary>
    /// Determines if the search button can be clicked. This is available when the element is selected and we are not already
    /// searching.
    /// </summary>
    /// <returns></returns>
    private bool CanSearch() => Element != Element.Default && !Searching;

    /// <summary>
    /// Cancels the current running search using the local cancellation token.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        _cancellation?.Cancel();
        SearchCleanup();
    }

    private bool CanCancel() => Searching;

    /// <summary>
    /// The code that actually executes the search. just uses the selected element and current source and configured
    /// filters to iterate each item and check it it passes the filters. If so then adds the result to the collection
    /// using the dispatcher so the UI should update as results are added.
    /// </summary>
    /// <param name="token">The cancellation token to cancel the search.</param>
    /// <returns>The async task representing the search.</returns>
    private Task ExecuteSearch(CancellationToken token)
    {
        var element = Element;
        var content = Source.Model.L5X;
        var criteria = Filters.Select(f => f.Model).ToList();
        Func<object?, bool> predicate = Inclusion == Inclusion.All
            ? x => criteria.All(f => f.Evaluate(x))
            : x => criteria.Any(f => f.Evaluate(x));

        return Task.Run(() =>
        {
            foreach (var item in element.Query(content))
            {
                if (token.IsCancellationRequested) break;

                var passes = predicate.Invoke(item);
                if (passes && item is LogixElement result)
                {
                    Dispatcher.UIThread.Post(() => Results.Add(result));
                }
            }
        }, token);
    }

    /// <summary>
    /// Performs some cleanup by disposing and resetting the cancellation token so we are ready for the next search.
    /// </summary>
    private void SearchCleanup()
    {
        _cancellation?.Dispose();
        _cancellation = null;
        Searching = false;
    }
}