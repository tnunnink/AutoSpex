using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Components;

public class VariableList : TemplatedControl
{
    public static readonly DirectProperty<VariableList, ObservableCollection<VariableObserver>?> VariablesProperty =
        AvaloniaProperty.RegisterDirect<VariableList, ObservableCollection<VariableObserver>?>(
            nameof(Variables), o => o.Variables, (o, v) => o.Variables = v);

    public static readonly DirectProperty<VariableList, ICommand?> AddCommandProperty =
        AvaloniaProperty.RegisterDirect<VariableList, ICommand?>(
            nameof(AddCommand), o => o.AddCommand, (o, v) => o.AddCommand = v,
            defaultBindingMode: BindingMode.TwoWay);

    private ObservableCollection<VariableObserver>? _variables = [];
    private ICommand? _addCommand;

    public VariableList()
    {
        RemoveCommand = new RelayCommand(RemoveVariables, HasSelected);
        CopyCommand = new RelayCommand(CopyVariables, HasSelected);
    }

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

    public ObservableCollection<VariableObserver> SelectedVariables { get; } = [];

    public IRelayCommand RemoveCommand { get; }
    public IRelayCommand CopyCommand { get; }

    private void RemoveVariables()
    {
        if (Variables is null) return;
        
        var selected = SelectedVariables.ToList();

        foreach (var variable in selected)
            Variables.Remove(variable);
    }

    private void CopyVariables()
    {
        var selected = SelectedVariables.ToList();

        throw new NotImplementedException();
    }

    private bool HasSelected() => SelectedVariables.Count > 0;
}