using System;
using System.Collections.ObjectModel;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using DynamicData;
using DynamicData.Binding;
using Operation = AutoSpex.Engine.Operation;

namespace AutoSpex.Client.Components;

[PseudoClasses(UnsupportedClass)]
public class OperationEntry : TemplatedControl
{
    private const string UnsupportedClass = ":unsupported";
    private const string PartList = "OperationList";

    #region Properties

    public static readonly DirectProperty<OperationEntry, Operation?> OperationProperty =
        AvaloniaProperty.RegisterDirect<OperationEntry, Operation?>(
            nameof(Operation), o => o.Operation, (o, v) => o.Operation = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<OperationEntry, Property?> PropertyProperty =
        AvaloniaProperty.RegisterDirect<OperationEntry, Property?>(
            nameof(Property), o => o.Property, (o, v) => o.Property = v);

    #endregion

    private readonly SourceList<Operation> _source = new();
    private readonly ReadOnlyObservableCollection<Operation> _operations;
    private Operation? _operation;
    private Property? _property;
    private ListBox? _listBox;

    public OperationEntry()
    {
        _source.Connect()
            .Sort(SortExpressionComparer<Operation>.Ascending(t => t.Name))
            .Bind(out _operations)
            .Subscribe();
    }

    public Property? Property
    {
        get => _property;
        set => SetAndRaise(PropertyProperty, ref _property, value);
    }

    public Operation? Operation
    {
        get => _operation;
        set => SetAndRaise(OperationProperty, ref _operation, value);
    }

    public ReadOnlyObservableCollection<Operation> Operations => _operations;


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != PropertyProperty) return;
        UpdateOperations(change);
        SetUnsupportedClass();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterListBox(e);
    }

    private void UpdateOperations(AvaloniaPropertyChangedEventArgs args)
    {
        _source.Clear();
        if (args.NewValue is not Property property) return;
        _source.AddRange(Operation.Supporting(property));
    }

    private void RegisterListBox(TemplateAppliedEventArgs args)
    {
        _listBox?.RemoveHandler(PointerPressedEvent, HandleListPointerPressed);
        _listBox?.RemoveHandler(KeyDownEvent, HandleKeyDownEvent);

        _listBox = args.NameScope.Find<ListBox>(PartList);

        if (_listBox is null) return;
        _listBox.SelectedIndex = 0;
        _listBox.AddHandler(PointerPressedEvent, HandleListPointerPressed, RoutingStrategies.Tunnel);
        _listBox.AddHandler(KeyDownEvent, HandleKeyDownEvent, RoutingStrategies.Tunnel);
    }

    private void HandleListPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Control {DataContext: Operation operation} control) return;
        e.Handled = true;
        Operation = operation;
        ClosePopupContaining(control);
    }

    private void HandleKeyDownEvent(object? sender, KeyEventArgs args)
    {
        if (args.Source is not Control control) return;

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (args.Key)
        {
            case Key.Down:
                HandleKeyDownNavigation();
                args.Handled = true;
                break;
            case Key.Up:
                HandleKeyUpNavigation();
                args.Handled = true;
                break;
            case Key.Escape:
                ClosePopupContaining(control);
                args.Handled = true;
                break;
            case Key.Enter:
                UpdateAndClose();
                args.Handled = true;
                break;
        }
    }

    private void HandleKeyUpNavigation()
    {
        if (_listBox is null) return;

        if (_listBox.SelectedIndex > 0)
        {
            _listBox.SelectedIndex--;
        }
        else
        {
            _listBox.SelectedIndex = _listBox.ItemCount - 1;
        }
    }

    private void HandleKeyDownNavigation()
    {
        if (_listBox is null) return;

        if (_listBox.SelectedIndex < _listBox.ItemCount - 1)
        {
            _listBox.SelectedIndex++;
        }
        else
        {
            _listBox.SelectedIndex = 0;
        }
    }

    private void UpdateAndClose()
    {
        if (_listBox is null) return;

        if (_listBox.SelectedItem is Operation operation)
            Operation = operation;

        ClosePopupContaining(_listBox);
    }

    private static void ClosePopupContaining(ILogical logical)
    {
        var popup = logical.FindLogicalAncestorOfType<Popup>();
        popup?.Close();
    }

    private void SetUnsupportedClass()
    {
        PseudoClasses.Remove(UnsupportedClass);

        if (Operation is null || Operation == Operation.None) return;

        if (!Operations.Contains(Operation))
            PseudoClasses.Add(UnsupportedClass);
    }
}