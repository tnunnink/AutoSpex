using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AutoSpex.Client.Components;

public class ProjectList : ContentControl
{
    #region AvaloniaProperties

    public static readonly DirectProperty<ProjectList, ObserverCollection<Project, ProjectObserver>>
        ProjectSourceProperty =
            AvaloniaProperty.RegisterDirect<ProjectList, ObserverCollection<Project, ProjectObserver>>
                (nameof(ProjectSource), o => o.ProjectSource, (c, v) => c.ProjectSource = v);

    public static readonly DirectProperty<ProjectList, string?> FilterTextProperty =
        AvaloniaProperty.RegisterDirect<ProjectList, string?>(
            nameof(FilterText), o => o.FilterText, (o, v) => o.FilterText = v);

    #endregion

    private ObserverCollection<Project, ProjectObserver> _projectSource = [];
    private string? _filterText;

    static ProjectList()
    {
        PointerReleasedEvent.AddClassHandler<ProjectList>((_, a) => HandlePointerReleased(a),
            RoutingStrategies.Bubble);
    }

    public ObserverCollection<Project, ProjectObserver> ProjectSource
    {
        get => _projectSource;
        set => SetAndRaise(ProjectSourceProperty, ref _projectSource, value);
    }

    public string? FilterText
    {
        get => _filterText;
        set => SetAndRaise(FilterTextProperty, ref _filterText, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        UpdateRecent(ProjectSource);
        UpdatePinned(ProjectSource);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ProjectSourceProperty)
            OnProjectSourceChanged(change);

        if (change.Property == FilterTextProperty)
            OnFilterTextChanged(change);
    }

    public ObservableCollection<ProjectObserver> RecentCollection { get; } = [];

    public ObservableCollection<ProjectObserver> PinnedCollection { get; } = [];

    private void OnFilterTextChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var value = args.GetNewValue<string?>();

        var filtered = string.IsNullOrEmpty(value)
            ? ProjectSource.ToList()
            : ProjectSource.Where(p => p.Name.Contains(value) || p.Directory.Contains(value)).ToList();

        UpdateRecent(filtered);
        UpdatePinned(filtered);
    }

    private void OnProjectSourceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is ObserverCollection<Project, ProjectObserver> old)
        {
            old.CollectionChanged -= ProjectSourceCollectionChanged;
            old.ItemPropertyChanged -= HandleProjectPinChange;
        }

        if (args.NewValue is not ObserverCollection<Project, ProjectObserver> projects) return;

        projects.CollectionChanged += ProjectSourceCollectionChanged;
        projects.ItemPropertyChanged += HandleProjectPinChange;

        UpdateRecent(projects);
        UpdatePinned(projects);
    }

    private void ProjectSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateRecent(ProjectSource);
        UpdatePinned(ProjectSource);
    }

    private void HandleProjectPinChange(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ProjectObserver project || e.PropertyName != nameof(ProjectObserver.Pinned)) return;

        if (project.Pinned)
        {
            PinnedCollection.Add(project);
            return;
        }

        PinnedCollection.Remove(project);
    }

    private void UpdateRecent(IEnumerable<ProjectObserver>? projects)
    {
        RecentCollection.Clear();

        if (projects is null) return;

        foreach (var project in projects)
        {
            RecentCollection.Add(project);
        }
    }

    private void UpdatePinned(IEnumerable<ProjectObserver>? projects)
    {
        PinnedCollection.Clear();

        if (projects is null) return;

        foreach (var project in projects.Where(p => p.Pinned))
        {
            PinnedCollection.Add(project);
        }
    }

    private static async void HandlePointerReleased(PointerReleasedEventArgs args)
    {
        if (args.InitialPressMouseButton != MouseButton.Left) return;
        if (args.Source is not Control { DataContext: ProjectObserver project }) return;
        await project.Connect();
    }
}