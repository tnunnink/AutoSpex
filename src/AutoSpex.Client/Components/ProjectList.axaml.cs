using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;

namespace AutoSpex.Client.Components;

[TemplatePart("PART_CreateButton", typeof(Button))]
public class ProjectList : ItemsControl
{
    private ICommand? _createCommand;
    private ICommand? _openCommand;
    private string? _filter;
    private Button? _createButton;

    public static readonly DirectProperty<ProjectList, ICommand?> CreateCommandProperty =
        AvaloniaProperty.RegisterDirect<ProjectList, ICommand?>
            (nameof(CreateCommand), o => o.CreateCommand, (o, v) => o.CreateCommand = v);

    public static readonly DirectProperty<ProjectList, ICommand?> OpenCommandProperty =
        AvaloniaProperty.RegisterDirect<ProjectList, ICommand?>
            (nameof(OpenCommand), o => o.OpenCommand, (o, v) => o.OpenCommand = v);

    public static readonly DirectProperty<ProjectList, string?> FilterProperty =
        AvaloniaProperty.RegisterDirect<ProjectList, string?>
            (nameof(Filter), o => o.Filter, (o, v) => o.Filter = v);

    public ICommand? CreateCommand
    {
        get => _createCommand;
        set => SetAndRaise(CreateCommandProperty, ref _createCommand, value);
    }

    public ICommand? OpenCommand
    {
        get => _openCommand;
        set => SetAndRaise(OpenCommandProperty, ref _openCommand, value);
    }

    public string? Filter
    {
        get => _filter;
        set => SetAndRaise(FilterProperty, ref _filter, value);
    }


    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_createButton is not null)
            _createButton.PointerReleased -= OnItemPointerReleased;

        _createButton = e.NameScope.Find("PART_CreateButton") as Button;

        if (_createButton is not null)
            _createButton.PointerReleased += OnItemPointerReleased;
    }

    /// <summary>
    /// This will look to see if this control is contained within a popup and if soe close it upon clicking of the button.
    /// </summary>
    private void OnItemPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var popup = this.FindLogicalAncestorOfType<Popup>();
        popup?.Close();
    }
}