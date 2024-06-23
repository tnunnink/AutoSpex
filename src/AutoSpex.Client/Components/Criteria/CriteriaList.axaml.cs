using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Components;

public class CriteriaList : TemplatedControl
{
    #region Properties

    public static readonly StyledProperty<string?> HeadingProperty =
        AvaloniaProperty.Register<CriteriaList, string?>(
            nameof(Heading));

    public static readonly StyledProperty<string?> InfoTextProperty =
        AvaloniaProperty.Register<CriteriaList, string?>(
            nameof(InfoText));

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

    #endregion

    private Element? _element;
    private ObservableCollection<CriterionObserver>? _criteria = [];
    private Inclusion _inclusion;

    public CriteriaList()
    {
        AddCriterionCommand = new RelayCommand(AddCriterion, CanAddCriterion);
        ToggleInclusionCommand = new RelayCommand(ToggleInclusion);
        RemoveCriterionCommand = new RelayCommand<CriterionObserver>(RemoveCriterion);
        RemoveSelectedCommand = new RelayCommand(RemoveSelected, HasSelected);
        CopySelectedCommand = new AsyncRelayCommand(CopySelected, HasSelected);
        PasteSelectedCommand = new AsyncRelayCommand(PasteCriteria);
    }

    public string? Heading
    {
        get => GetValue(HeadingProperty);
        set => SetValue(HeadingProperty, value);
    }

    public string? InfoText
    {
        get => GetValue(InfoTextProperty);
        set => SetValue(InfoTextProperty, value);
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

    public ObservableCollection<CriterionObserver> SelectedCriteria { get; } = [];

    public IRelayCommand? AddCriterionCommand { get; }
    public IRelayCommand? ToggleInclusionCommand { get; }
    public IRelayCommand? RemoveCriterionCommand { get; }
    public IRelayCommand? RemoveSelectedCommand { get; }
    public IRelayCommand? CopySelectedCommand { get; }
    public IRelayCommand? PasteSelectedCommand { get; }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ElementProperty)
        {
            AddCriterionCommand?.NotifyCanExecuteChanged();
        }
    }

    private void AddCriterion()
    {
        if (Criteria is null || Element is null) return;
        var criterion = new CriterionObserver(new Criterion(Element.Type));
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

        var selected = SelectedCriteria.ToList();

        foreach (var criterion in selected)
            Criteria.Remove(criterion);
    }

    private Task CopySelected()
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard ??
                        throw new InvalidOperationException("Could not retrieve clipboard from current context.");

        var data = new DataObject();
        data.Set(nameof(Criterion), SelectedCriteria.ToList());

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
            Criteria.Add(criterion);
    }

    private bool HasSelected() => SelectedCriteria.Count > 0;

    /// <summary>
    /// Changes the state of the inclusion property to the opposite value.
    /// </summary>
    private void ToggleInclusion()
    {
        Inclusion = Inclusion == Inclusion.All ? Inclusion.Any : Inclusion.All;
    }
}