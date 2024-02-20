using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Components;

public class CriteriaList : TemplatedControl
{
    private const string PartCheckAll = "CheckAll";

    #region Properties

    public static readonly DirectProperty<CriteriaList, Element?> ElementProperty =
        AvaloniaProperty.RegisterDirect<CriteriaList, Element?>(
            nameof(Element), o => o.Element, (o, v) => o.Element = v);

    public static readonly DirectProperty<CriteriaList, ObservableCollection<CriterionObserver>?> CriteriaProperty =
        AvaloniaProperty.RegisterDirect<CriteriaList, ObservableCollection<CriterionObserver>?>(
            nameof(Criteria), o => o.Criteria, (o, v) => o.Criteria = v);

    #endregion

    private Element? _element;
    private ObservableCollection<CriterionObserver>? _criteria = [];
    private CheckBox? _checkBox;

    public CriteriaList()
    {
        AddCriterionCommand = new RelayCommand(AddCriterion, CanAddCriterion);
        RemoveSelectedCommand = new RelayCommand(RemoveSelected, CriteriaAreSelected);
        InvertSelectedCommand = new RelayCommand(InvertSelected, CriteriaAreSelected);
        /*InvertSelectedCommand = new RelayCommand(RevertSelected, CriteriaAreSelected);*/
        DisableSelectedCommand = new RelayCommand(DisableSelected, CriteriaAreSelected);
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

    public IRelayCommand? AddCriterionCommand { get; }
    public IRelayCommand? RemoveSelectedCommand { get; }
    public IRelayCommand? InvertSelectedCommand { get; }
    public IRelayCommand? DisableSelectedCommand { get; }
    
    public IRelayCommand? DisableAllCommand { get; }
    
    public IRelayCommand? CopySelectedCommand { get; }


    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterCheckBox(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ElementProperty)
            AddCriterionCommand?.NotifyCanExecuteChanged();

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

    private void AddCriterion()
    {
        if (Criteria is null || Element is null) return;
        var criterion = new CriterionObserver(new Criterion(), Element.Type);
        Criteria.Add(criterion);
    }

    private bool CanAddCriterion() => Element is not null && Criteria is not null; 

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

    private void DisableSelected()
    {
        if (Criteria is null) return;

        foreach (var criterion in SelectedCriteria())
            criterion.IsEnabled = false;
    }
    
    private bool CriteriaAreSelected() => Criteria is not null && Criteria.Any(c => c.IsChecked);
    
    private IEnumerable<CriterionObserver> SelectedCriteria()
    {
        if (Criteria is null || Criteria.Count == 0) return Enumerable.Empty<CriterionObserver>();
        return Criteria.Where(c => c.IsChecked).ToList();
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
    }

    private void OnCriterionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CriterionObserver.IsChecked)) return;
        RemoveSelectedCommand?.NotifyCanExecuteChanged();
        InvertSelectedCommand?.NotifyCanExecuteChanged();
        DisableSelectedCommand?.NotifyCanExecuteChanged();
    }
}