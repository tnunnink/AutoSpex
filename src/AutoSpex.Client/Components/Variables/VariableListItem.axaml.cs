using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace AutoSpex.Client.Components;

public class VariableListItem : TemplatedControl
{
    public static readonly DirectProperty<VariableListItem, ICommand?> RemoveCommandProperty =
        AvaloniaProperty.RegisterDirect<VariableListItem, ICommand?>(
            nameof(RemoveCommand), o => o.RemoveCommand, (o, v) => o.RemoveCommand = v,
            defaultBindingMode: BindingMode.TwoWay);
    
    private ICommand? _removeCommand;

    public ICommand? RemoveCommand
    {
        get => _removeCommand;
        set => SetAndRaise(RemoveCommandProperty, ref _removeCommand, value);
    }
}