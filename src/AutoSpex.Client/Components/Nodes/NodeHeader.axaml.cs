using System.Windows.Input;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class NodeHeader : TemplatedControl
{
    #region Properties
    
    public static readonly DirectProperty<NodeHeader, Breadcrumb?> BreadcrumbProperty =
        AvaloniaProperty.RegisterDirect<NodeHeader, Breadcrumb?>(
            nameof(Breadcrumb), o => o.Breadcrumb, (o, v) => o.Breadcrumb = v);

    public static readonly DirectProperty<NodeHeader, bool> IsChangedProperty =
        AvaloniaProperty.RegisterDirect<NodeHeader, bool>(
            nameof(IsChanged), o => o.IsChanged, (o, v) => o.IsChanged = v);

    public static readonly DirectProperty<NodeHeader, ICommand?> SaveCommandProperty =
        AvaloniaProperty.RegisterDirect<NodeHeader, ICommand?>(
            nameof(SaveCommand), o => o.SaveCommand, (o, v) => o.SaveCommand = v);

    public static readonly DirectProperty<NodeHeader, ICommand?> RunCommandProperty =
        AvaloniaProperty.RegisterDirect<NodeHeader, ICommand?>(
            nameof(RunCommand), o => o.RunCommand, (o, v) => o.RunCommand = v);

    #endregion
    
    private Breadcrumb? _breadcrumb;
    private bool _isChanged;
    private ICommand? _saveCommand;
    private ICommand? _runCommand;

    public Breadcrumb? Breadcrumb
    {
        get => _breadcrumb;
        set => SetAndRaise(BreadcrumbProperty, ref _breadcrumb, value);
    }

    public bool IsChanged
    {
        get => _isChanged;
        set => SetAndRaise(IsChangedProperty, ref _isChanged, value);
    }

    public ICommand? SaveCommand
    {
        get => _saveCommand;
        set => SetAndRaise(SaveCommandProperty, ref _saveCommand, value);
    }

    public ICommand? RunCommand
    {
        get => _runCommand;
        set => SetAndRaise(RunCommandProperty, ref _runCommand, value);
    }
}