using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using DynamicData;
using DynamicData.Binding;

namespace AutoSpex.Client.Components;

public class ProjectList : ContentControl
{
    private const string PartFilterText = "FilterText";
    private const string PartProjectList = "ProjectList";

    #region AvaloniaProperties

    public static readonly StyledProperty<bool> ShowButtonsProperty =
        AvaloniaProperty.Register<ProjectList, bool>(
            nameof(ShowButtons));

    public static readonly DirectProperty<ProjectList, ICommand?> CreateCommandProperty =
        AvaloniaProperty.RegisterDirect<ProjectList, ICommand?>
            (nameof(CreateCommand), o => o.CreateCommand, (o, v) => o.CreateCommand = v);

    public static readonly DirectProperty<ProjectList, ICommand?> OpenCommandProperty =
        AvaloniaProperty.RegisterDirect<ProjectList, ICommand?>
            (nameof(OpenCommand), o => o.OpenCommand, (o, v) => o.OpenCommand = v);

    public static readonly DirectProperty<ProjectList, ObservableCollection<ProjectObserver>> ProjectSourceProperty =
        AvaloniaProperty.RegisterDirect<ProjectList, ObservableCollection<ProjectObserver>>
            (nameof(ProjectSource), o => o.ProjectSource, (c, v) => c.ProjectSource = v);

    public static readonly DirectProperty<ProjectList, ReadOnlyObservableCollection<ProjectObserver>> ProjectsProperty =
        AvaloniaProperty.RegisterDirect<ProjectList, ReadOnlyObservableCollection<ProjectObserver>>
            (nameof(Projects), o => o.Projects, defaultBindingMode: BindingMode.OneWayToSource);

    #endregion

    public ProjectList()
    {
        ProjectSourceProperty.Changed.AddClassHandler<ProjectList>((c, a) => c.OnProjectSourcePropertyChanged(a));

        _cache.Connect()
            .Sort(SortExpressionComparer<ProjectObserver>.Descending(t => t.OpenedOn))
            .Bind(out _projects)
            .Subscribe();
    }

    private ICommand? _createCommand;
    private ICommand? _openCommand;
    private ObservableCollection<ProjectObserver> _projectSource = [];
    private TextBox? _textBox;
    private readonly SourceCache<ProjectObserver, Uri> _cache = new(x => x.Uri);
    private readonly ReadOnlyObservableCollection<ProjectObserver> _projects;

    public bool ShowButtons
    {
        get => GetValue(ShowButtonsProperty);
        set => SetValue(ShowButtonsProperty, value);
    }

    public ICommand? CreateCommand
    {
        get => _createCommand;
        set => SetAndRaise(CreateCommandProperty, ref _createCommand, value);
    }

    public ICommand? OpenCommand
    {
        get => _openCommand;
        set => SetAndRaise(OpenCommandProperty, ref _openCommand, value);
    }

    public ObservableCollection<ProjectObserver> ProjectSource
    {
        get => _projectSource;
        set => SetAndRaise(ProjectSourceProperty, ref _projectSource, value);
    }

    public ReadOnlyObservableCollection<ProjectObserver> Projects => _projects;


    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterFilterTextChange(e);
    }

    private void RegisterFilterTextChange(TemplateAppliedEventArgs e)
    {
        if (_textBox is not null)
            _textBox.TextChanged -= OnFilterTextChanged;

        _textBox = e.NameScope.Find(PartFilterText) as TextBox;

        if (_textBox is not null)
            _textBox.TextChanged += OnFilterTextChanged;
    }

    private void OnFilterTextChanged(object? sender, TextChangedEventArgs? args)
    {
        var value = _textBox?.Text;

        _cache.Edit(innerList =>
        {
            innerList.Clear();

            var filtered = string.IsNullOrEmpty(value)
                ? ProjectSource
                : ProjectSource.Where(p => p.Name.Contains(value) || p.Directory.Contains(value));

            innerList.AddOrUpdate(filtered);
        });
    }

    private void OnProjectSourcePropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is ObservableCollection<ProjectObserver> oldValue)
        {
            oldValue.CollectionChanged -= ProjectSourceCollectionChanged;
        }

        if (args.NewValue is not ObservableCollection<ProjectObserver> projects) return;
        projects.CollectionChanged += ProjectSourceCollectionChanged;
        _cache.Clear();
        _cache.AddOrUpdate(projects);
    }

    private void ProjectSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e is {Action: NotifyCollectionChangedAction.Add, NewItems: not null})
        {
            _cache.AddOrUpdate(e.NewItems.OfType<ProjectObserver>());
            return;
        }

        _cache.Clear();
        _cache.AddOrUpdate(ProjectSource);
    }
}