using System.Collections.ObjectModel;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Components;

public class VariableTable : TemplatedControl
{
    public static readonly DirectProperty<VariableTable, ObservableCollection<VariableObserver>>
        VariableSourceProperty = AvaloniaProperty.RegisterDirect<VariableTable, ObservableCollection<VariableObserver>>(
            nameof(VariableSource), o => o.VariableSource, (o, v) => o.VariableSource = v);

    private ObservableCollection<VariableObserver> _variableSource = [];

    public ObservableCollection<VariableObserver> VariableSource
    {
        get => _variableSource;
        set => SetAndRaise(VariableSourceProperty, ref _variableSource, value);
    }

    public IRelayCommand AddVariableCommand { get; }

    public IRelayCommand RemoveVariableCommand { get; }
}