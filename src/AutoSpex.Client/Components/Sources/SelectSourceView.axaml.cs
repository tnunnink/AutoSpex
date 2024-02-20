using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace AutoSpex.Client.Components;

public class SelectSourceView : TemplatedControl
{
    public static readonly DirectProperty<SelectSourceView, string?> LocationProperty =
        AvaloniaProperty.RegisterDirect<SelectSourceView, string?>(
            nameof(Location), o => o.Location, (o, v) => o.Location = v, defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<SelectSourceView, ICommand?> PickFileCommandProperty =
        AvaloniaProperty.RegisterDirect<SelectSourceView, ICommand?>(
            nameof(PickFileCommand), o => o.PickFileCommand, (o, v) => o.PickFileCommand = v);

    private string? _location;
    private ICommand? _pickFileCommand;

    public string? Location
    {
        get => _location;
        set => SetAndRaise(LocationProperty, ref _location, value);
    }

    public ICommand? PickFileCommand
    {
        get => _pickFileCommand;
        set => SetAndRaise(PickFileCommandProperty, ref _pickFileCommand, value);
    }
}