using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Components;

public class CriteriaList : TemplatedControl
{
    private const string PartCheckAll = "CheckAll";

    #region Properties

    public static readonly DirectProperty<CriteriaList, Element?> ElementProperty =
        AvaloniaProperty.RegisterDirect<CriteriaList, Element?>(
            nameof(Element), o => o.Element, (o, v) => o.Element = v, defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<CriteriaList, ObservableCollection<CriterionObserver>?> CriteriaProperty =
        AvaloniaProperty.RegisterDirect<CriteriaList, ObservableCollection<CriterionObserver>?>(
            nameof(Criteria), o => o.Criteria, (o, v) => o.Criteria = v);

    public static readonly DirectProperty<CriteriaList, Inclusion> InclusionProperty =
        AvaloniaProperty.RegisterDirect<CriteriaList, Inclusion>(
            nameof(Inclusion), o => o.Inclusion, (o, v) => o.Inclusion = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<CriteriaList, ICommand?> RunCommandProperty =
        AvaloniaProperty.RegisterDirect<CriteriaList, ICommand?>(
            nameof(RunCommand), o => o.RunCommand, (o, v) => o.RunCommand = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> UseElementSelectorProperty =
        AvaloniaProperty.Register<CriteriaList, bool>(
            nameof(UseElementSelector));
    
    public static readonly StyledProperty<bool> UseRunButtonProperty =
        AvaloniaProperty.Register<CriteriaList, bool>(
            nameof(UseRunButton));

    #endregion

    private Element? _element;
    private ObservableCollection<CriterionObserver>? _criteria = [];
    private Inclusion _inclusion;
    private ICommand? _runCommand;
    private CheckBox? _checkBox;

    public CriteriaList()
    {
        AddCriterionCommand = new RelayCommand(AddCriterion, CanAddCriterion);
        RemoveCriterionCommand = new RelayCommand<CriterionObserver>(RemoveCriterion);
        RemoveSelectedCommand = new RelayCommand(RemoveSelected, CriteriaAreSelected);
        InvertSelectedCommand = new RelayCommand(InvertSelected, CriteriaAreSelected);
        /*InvertSelectedCommand = new RelayCommand(RevertSelected, CriteriaAreSelected);*/
        EnableSelectedCommand = new RelayCommand(EnableSelected, CriteriaAreSelected);
        DisableSelectedCommand = new RelayCommand(DisableSelected, CriteriaAreSelected);
        EnableAllCommand = new RelayCommand(EnableAll);
        DisableAllCommand = new RelayCommand(DisableAll);
        CopySelectedCommand = new AsyncRelayCommand(CopySelected, CriteriaAreSelected);
        PasteSelectedCommand = new AsyncRelayCommand(PasteCriteria);
    }

    public Element? Element
    {
        get => _element;
        set => SetAndRaise(ElementProperty, ref _element, value);
    }

    public ObservableCollection<CriterionObserver>? Criteria
    {
        get => _criteria;
        set => SetAndRaise(CriteriaProperty, ref _criteria, value);
    }
    
    public Inclusion Inclusion
    {
        get => _inclusion;
        set => SetAndRaise(InclusionProperty, ref _inclusion, value);
    }
    
    public ICommand? RunCommand
    {
        get => _runCommand;
        set => SetAndRaise(RunCommandProperty, ref _runCommand, value);
    }

    public bool UseElementSelector
    {
        get => GetValue(UseElementSelectorProperty);
        set => SetValue(UseElementSelectorProperty, value);
    }
    
    public bool UseRunButton
    {
        get => GetValue(UseElementSelectorProperty);
        set => SetValue(UseElementSelectorProperty, value);
    }

    public IRelayCommand? AddCriterionCommand { get; }
    public IRelayCommand? RemoveCriterionCommand { get; }
    public IRelayCommand? RemoveSelectedCommand { get; }
    public IRelayCommand? InvertSelectedCommand { get; }
    public IRelayCommand? EnableSelectedCommand { get; }
    public IRelayCommand? DisableSelectedCommand { get; }
    public IRelayCommand? EnableAllCommand { get; }
    public IRelayCommand? DisableAllCommand { get; }
    public IRelayCommand? CopySelectedCommand { get; }
    public IRelayCommand? PasteSelectedCommand { get; }


    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterCheckBox(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ElementProperty)
            HandleElementChanged(change);

        if (change.Property == CriteriaProperty)
            AttachCollectionChanged(change);
    }

    private void RegisterCheckBox(TemplateAppliedEventArgs args)
    {
        if (_checkBox is not null)
            _checkBox.IsCheckedChanged += HandleCheckChange;

        _checkBox = args.NameScope.Find<CheckBox>(PartCheckAll);

        if (_checkBox is null) return;
        _checkBox.IsCheckedChanged += HandleCheckChange;
    }

    private void HandleCheckChange(object? sender, RoutedEventArgs e)
    {
        if (Criteria is null) return;
        if (e.Source is not CheckBox checkBox) return;

        var isChecked = checkBox.IsChecked is true;

        foreach (var criterion in Criteria)
            criterion.IsChecked = isChecked;
    }


    private void HandleElementChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var element = change.GetNewValue<Element>();
        UpdateCriteriaElement(element);
        AddCriterionCommand?.NotifyCanExecuteChanged();
    }

    private void AddCriterion()
    {
        if (Criteria is null || Element is null) return;
        var criterion = new CriterionObserver(new Criterion(), Element.Type);
        Criteria.Add(criterion);
    }

    private bool CanAddCriterion() => Element is not null && Element != Element.Default && Criteria is not null;

    private void RemoveCriterion(CriterionObserver? observer)
    {
        if (observer is null) return;
        Criteria?.Remove(observer);
    }

    private void RemoveSelected()
    {
        if (Criteria is null) return;

        foreach (var criterion in SelectedCriteria())
            Criteria.Remove(criterion);
    }

    private void InvertSelected()
    {
        if (Criteria is null) return;

        foreach (var criterion in SelectedCriteria())
            criterion.Invert = true;
    }

    private void RevertSelected()
    {
        if (Criteria is null) return;

        foreach (var criterion in SelectedCriteria())
            criterion.Invert = false;
    }

    private void EnableSelected()
    {
        if (Criteria is null) return;

        foreach (var criterion in SelectedCriteria())
            criterion.IsEnabled = true;
    }

    private void EnableAll()
    {
        if (Criteria is null) return;

        foreach (var criterion in Criteria)
            criterion.IsEnabled = true;
    }

    private void DisableSelected()
    {
        if (Criteria is null) return;

        foreach (var criterion in SelectedCriteria())
            criterion.IsEnabled = false;
    }

    private void DisableAll()
    {
        if (Criteria is null) return;

        foreach (var criterion in Criteria)
            criterion.IsEnabled = false;
    }

    private Task CopySelected()
    {
        if (Criteria is null) return Task.CompletedTask;

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard ??
                        throw new InvalidOperationException("Could not retrieve clipboard from current context.");

        var data = new DataObject();
        data.Set(nameof(Criterion), SelectedCriteria().ToList());

        return clipboard.SetDataObjectAsync(data);
    }

    private async Task PasteCriteria()
    {
        if (Criteria is null) return;

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard ??
                        throw new InvalidOperationException("Could not retrieve clipboard from current context.");

        var data = await clipboard.GetDataAsync(nameof(Criterion));

        if (data is not List<CriterionObserver> criteria) return;

        foreach (var criterion in criteria)
        {
            Criteria.Add(criterion);
        }
    }

    private bool CriteriaAreSelected() => Criteria is not null && Criteria.Any(c => c.IsChecked);

    private IEnumerable<CriterionObserver> SelectedCriteria()
    {
        if (Criteria is null || Criteria.Count == 0) return Enumerable.Empty<CriterionObserver>();
        return Criteria.Where(c => c.IsChecked).ToList();
    }

    private void UpdateCriteriaElement(Element? element)
    {
        if (Criteria is null || element is null) return;

        foreach (var criterion in Criteria)
            criterion.Type = element.Type;
    }

    private void AttachCollectionChanged(AvaloniaPropertyChangedEventArgs change)
    {
        DetachCollection(change);
        AttachCollection(change);
    }

    private void AttachCollection(AvaloniaPropertyChangedEventArgs change)
    {
        var collection = change.GetNewValue<ObservableCollection<CriterionObserver>?>();
        if (collection is null) return;

        collection.CollectionChanged += OnCriteriaCollectionChanged;
        foreach (var observer in collection)
            observer.PropertyChanged += OnCriterionPropertyChanged;
    }

    private void DetachCollection(AvaloniaPropertyChangedEventArgs change)
    {
        var collection = change.GetOldValue<ObservableCollection<CriterionObserver>?>();
        if (collection is null) return;

        collection.CollectionChanged -= OnCriteriaCollectionChanged;
        foreach (var observer in collection)
            observer.PropertyChanged -= OnCriterionPropertyChanged;
    }

    private void OnCriteriaCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var criterion in e.OldItems?.Cast<CriterionObserver>() ?? Enumerable.Empty<CriterionObserver>())
            criterion.PropertyChanged -= OnCriterionPropertyChanged;

        foreach (var criterion in e.NewItems?.Cast<CriterionObserver>() ?? Enumerable.Empty<CriterionObserver>())
            criterion.PropertyChanged += OnCriterionPropertyChanged;

        RefreshButtonExecutionStates();
    }

    private void OnCriterionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CriterionObserver.IsChecked)) return;
        RefreshButtonExecutionStates();
    }
    
    private void RefreshButtonExecutionStates()
    {
        RemoveSelectedCommand?.NotifyCanExecuteChanged();
        InvertSelectedCommand?.NotifyCanExecuteChanged();
        EnableSelectedCommand?.NotifyCanExecuteChanged();
        DisableSelectedCommand?.NotifyCanExecuteChanged();
        CopySelectedCommand?.NotifyCanExecuteChanged();
        PasteSelectedCommand?.NotifyCanExecuteChanged();
    }
}