using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Components;

public class VariableList : TemplatedControl
{
    private const string PartCheckAll = "CheckAll";

    public static readonly DirectProperty<VariableList, ObservableCollection<VariableObserver>?> VariablesProperty =
        AvaloniaProperty.RegisterDirect<VariableList, ObservableCollection<VariableObserver>?>(
            nameof(Variables), o => o.Variables, (o, v) => o.Variables = v);

    public static readonly DirectProperty<VariableList, ICommand?> AddCommandProperty =
        AvaloniaProperty.RegisterDirect<VariableList, ICommand?>(
            nameof(AddCommand), o => o.AddCommand, (o, v) => o.AddCommand = v,
            defaultBindingMode: BindingMode.TwoWay);

    private ObservableCollection<VariableObserver>? _variables = [];
    private ICommand? _addCommand;
    private CheckBox? _checkBox;
    private List<VariableObserver> AllVariables => Variables?.ToList() ?? [];
    private List<VariableObserver> CheckedVariables => Variables?.Where(v => v.IsChecked).ToList() ?? [];

    public VariableList()
    {
        RemoveVariableCommand = new RelayCommand<VariableObserver>(RemoveVariable);
        RemoveCheckedCommand = new RelayCommand(RemoveCheckedVariables, AnyChecked);
    }

    public ObservableCollection<VariableObserver> VariablesSource { get; } = [];

    public ObservableCollection<VariableObserver>? Variables
    {
        get => _variables;
        set => SetAndRaise(VariablesProperty, ref _variables, value);
    }

    public ICommand? AddCommand
    {
        get => _addCommand;
        set => SetAndRaise(AddCommandProperty, ref _addCommand, value);
    }

    public IRelayCommand RemoveVariableCommand { get; }
    public IRelayCommand RemoveCheckedCommand { get; }
    
    public IRelayCommand CopyCheckedCommand { get; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterCheckBox(e);
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
        if (e.Source is not CheckBox checkBox) return;

        foreach (var variable in AllVariables)
        {
            variable.IsChecked = checkBox.IsChecked is true;
        }
    }

    private void RemoveVariable(VariableObserver? variable)
    {
        if (variable is null) return;
        Variables?.Remove(variable);
    }

    private void RemoveCheckedVariables()
    {
        foreach (var variable in CheckedVariables)
            Variables?.Remove(variable);
    }

    private bool AnyChecked() => CheckedVariables.Count > 0;
}