using System.Windows.Input;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class EditSourceView : TemplatedControl
{
    public static readonly DirectProperty<EditSourceView, SourceObserver?> SourceProperty =
        AvaloniaProperty.RegisterDirect<EditSourceView, SourceObserver?>(
            nameof(Source), o => o.Source, (o, v) => o.Source = v);

    public static readonly DirectProperty<EditSourceView, ICommand?> AddCommandProperty =
        AvaloniaProperty.RegisterDirect<EditSourceView, ICommand?>(
            nameof(AddCommand), o => o.AddCommand, (o, v) => o.AddCommand = v);

    private SourceObserver? _source;
    private ICommand? _addCommand;

    public SourceObserver? Source
    {
        get => _source;
        set => SetAndRaise(SourceProperty, ref _source, value);
    }
    
    public ICommand? AddCommand
    {
        get => _addCommand;
        set => SetAndRaise(AddCommandProperty, ref _addCommand, value);
    }
}