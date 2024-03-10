using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Components;

public class VariableList : TemplatedControl
{
    private const string PartCheckAll = "CheckAll";

    public static readonly DirectProperty<VariableList, ObservableCollection<VariableObserver>> VariablesProperty =
        AvaloniaProperty.RegisterDirect<VariableList, ObservableCollection<VariableObserver>>(
            nameof(Variables), o => o.Variables, (o, v) => o.Variables = v);

    private ObservableCollection<VariableObserver> _variables = [];
    private CheckBox? _checkBox;

    public VariableList()
    {
        AddVariableCommand = new RelayCommand(AddVariable);
        RemoveVariableCommand = new RelayCommand<VariableObserver>(RemoveVariable);
        RemoveCheckedCommand = new RelayCommand(RemoveCheckedVariables, AnyChecked);
    }

    public ObservableCollection<VariableObserver> VariablesSource { get; } = [];

    public ObservableCollection<VariableObserver> Variables
    {
        get => _variables;
        set => SetAndRaise(VariablesProperty, ref _variables, value);
    }

    public IRelayCommand AddVariableCommand { get; }

    public IRelayCommand RemoveVariableCommand { get; }
    public IRelayCommand RemoveCheckedCommand { get; }

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

        foreach (var variable in Variables)
        {
            variable.IsChecked = checkBox.IsChecked is true;
        }
    }

    private void AddVariable()
    {
        Variables.Add(new VariableObserver(new Variable("Var01")));
    }

    private void RemoveVariable(VariableObserver? variable)
    {
        if (variable is null) return;
        Variables.Remove(variable);
    }

    private void RemoveCheckedVariables()
    {
        var variables = GetCheckedVariables();

        foreach (var variable in variables)
            Variables.Remove(variable);
    }

    private bool AnyChecked() => GetCheckedVariables().Count > 0;

    private List<VariableObserver> GetCheckedVariables() => Variables.Where(v => v.IsChecked).ToList();
}