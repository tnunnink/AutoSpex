using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace AutoSpex.Client.Components;

public class SaveNodeButton : TemplatedControl
{
    public static readonly DirectProperty<SaveNodeButton, ICommand?> SaveCommandProperty =
        AvaloniaProperty.RegisterDirect<SaveNodeButton, ICommand?>(
            nameof(SaveCommand), o => o.SaveCommand, (o, v) => o.SaveCommand = v);

    public static readonly StyledProperty<ControlTheme> ButtonThemeProperty =
        AvaloniaProperty.Register<ElementSelector, ControlTheme>(
            nameof(ButtonTheme));

    private ICommand? _saveCommand;

    public ICommand? SaveCommand
    {
        get => _saveCommand;
        set => SetAndRaise(SaveCommandProperty, ref _saveCommand, value);
    }

    public ControlTheme ButtonTheme
    {
        get => GetValue(ButtonThemeProperty);
        set => SetValue(ButtonThemeProperty, value);
    }
}