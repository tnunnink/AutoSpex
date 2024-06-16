using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;

namespace AutoSpex.Client.Components;

[PseudoClasses(ClassDragOver)]
public class OutcomeTree : TemplatedControl
{
    #region Properties

    public static readonly DirectProperty<OutcomeTree, RunObserver?> RunProperty =
        AvaloniaProperty.RegisterDirect<OutcomeTree, RunObserver?>(
            nameof(Run), o => o.Run, (o, v) => o.Run = v, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> TextFilterProperty =
        AvaloniaProperty.Register<OutcomeTree, string?>(
            nameof(TextFilter));

    #endregion

    private const string ClassDragOver = ":dragover";
    private RunObserver? _run;
    private IEnumerable<OutcomeObserver> OutcomeSource => Run?.Outcomes ?? Enumerable.Empty<OutcomeObserver>();


    static OutcomeTree()
    {
        DragDrop.DragEnterEvent.AddClassHandler<OutcomeTree>((x, a) => x.OnDragEnter(a));
        DragDrop.DragLeaveEvent.AddClassHandler<OutcomeTree>((x, a) => x.OnDragLeave(a));
        DragDrop.DragOverEvent.AddClassHandler<OutcomeTree>((_, a) => OnDragOver(a));
        DragDrop.DropEvent.AddClassHandler<OutcomeTree>((x, a) => x.OnDrop(a));
    }

    public RunObserver? Run
    {
        get => _run;
        set => SetAndRaise(RunProperty, ref _run, value);
    }

    public string? TextFilter
    {
        get => GetValue(TextFilterProperty);
        set => SetValue(TextFilterProperty, value);
    }

    public ObservableCollection<OutcomeObserver> OutcomeCollection { get; } = [];


    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == RunProperty)
        {
            RegisterOutcomeCollection(change);
            RegisterRunPropertyChange(change);
        }

        if (change.Property == TextFilterProperty)
            UpdateCollection();
    }

    /// <summary>
    /// Attach to the current run's outcomes collection change event, so we can update the local collection when items
    /// are added or removed.
    /// </summary>
    private void RegisterOutcomeCollection(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is RunObserver old)
            old.Outcomes.CollectionChanged -= OnOutcomeCollectionChanged;

        if (args.NewValue is not RunObserver run) return;
        run.Outcomes.CollectionChanged += OnOutcomeCollectionChanged;
        UpdateCollection();
    }

    /// <summary>
    /// Attach to the current run's property changed event so that we can respond to the custom filter property changes
    /// and update the local outcome collection.
    /// </summary>
    private void RegisterRunPropertyChange(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is RunObserver old) old.PropertyChanged -= OnRunPropertyChanged;
        if (args.NewValue is RunObserver run) run.PropertyChanged += OnRunPropertyChanged;
    }

    /// <summary>
    /// When an outcome is added or removed to the run collection then trigger update of the local collection.
    /// </summary>
    private void OnOutcomeCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateCollection();
    }

    /// <summary>
    /// When the result filter or node filter properties are updates the attached run then trigger update of the
    /// local collection to filter accordingly.
    /// </summary>
    private void OnRunPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(RunObserver.ResultFilter) or nameof(RunObserver.SourceFilter))
        {
            UpdateCollection();
        }
    }

    /// <summary>
    /// Refresh the local collection with the provided outcomes and optional filter text.
    /// </summary>
    private void UpdateCollection()
    {
        var outcomes = OutcomeSource;
        var filterText = TextFilter;
        var resultFilter = Run?.ResultFilter ?? ResultState.None;
        var sources = Run?.SourceFilter.Select(x => x.Id).ToHashSet() ?? Enumerable.Empty<Guid>().ToHashSet();

        var filtered = outcomes
            .Where(x => x.Filter(filterText)
                        && (resultFilter <= ResultState.Pending || x.Result == resultFilter)
                        && (sources.Count == 0 || sources.Contains(x.SourceId)))
            .ToList();

        OutcomeCollection.Refresh(filtered);
    }

    /// <summary>
    /// Handle the node drag enter event for the control.
    /// </summary>
    private void OnDragEnter(DragEventArgs e)
    {
        if (!TryGetValidNode(e, out _)) return;
        UpdateVisualStateDragOver(true);
        e.Handled = true;
    }

    /// <summary>
    /// Handle the node drag leave event for the control.
    /// </summary>
    private void OnDragLeave(DragEventArgs e)
    {
        if (!TryGetValidNode(e, out _)) return;
        UpdateVisualStateDragOver(false);
        e.Handled = true;
    }

    /// <summary>
    /// Handle the node drag over event for the control.
    /// </summary>
    private static void OnDragOver(DragEventArgs e)
    {
        if (!TryGetValidNode(e, out _)) return;
        e.Handled = true;
    }

    /// <summary>
    /// When the user drops a valid node object on this control, we want to either create a new run and seed it with
    /// the dropped node, or simply add the node to the current run instance.
    /// </summary>
    private void OnDrop(DragEventArgs e)
    {
        if (!TryGetValidNode(e, out var node)) return;
        if (node is null) return;

        if (Run is null)
        {
            Run = RunObserver.Virtual(node);
        }
        else
        {
            Run.AddNode(node);
        }

        UpdateVisualStateDragOver(false);
        e.Handled = true;
    }

    /// <summary>
    /// Determine if the current drag over event contains a valid node which can be dropped on this control.
    /// If valid, return the node instance along with the flag.
    /// </summary>
    private static bool TryGetValidNode(DragEventArgs e, out NodeObserver? node)
    {
        if (e.Data.Get("Node") is NodeObserver observer &&
            (observer.Feature == NodeType.Source || observer.Feature == NodeType.Spec))
        {
            node = observer;
            return true;
        }

        node = null;
        return false;
    }

    /// <summary>
    /// Update the visual state to indicate that the control is a valid drop location for the dragged node.
    /// </summary>
    private void UpdateVisualStateDragOver(bool dragover)
    {
        if (dragover)
        {
            PseudoClasses.Add(ClassDragOver);
            return;
        }

        PseudoClasses.Remove(ClassDragOver);
    }
}