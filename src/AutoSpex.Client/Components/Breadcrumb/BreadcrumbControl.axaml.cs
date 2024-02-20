using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class BreadcrumbControl : TemplatedControl
{
    private Breadcrumb? _breadcrumb;
    
    public static readonly DirectProperty<BreadcrumbControl, Breadcrumb?> BreadcrumbProperty =
        AvaloniaProperty.RegisterDirect<BreadcrumbControl, Breadcrumb?>(
            nameof(Breadcrumb), o => o.Breadcrumb, (o, v) => o.Breadcrumb = v);

    public Breadcrumb? Breadcrumb
    {
        get => _breadcrumb;
        set => SetAndRaise(BreadcrumbProperty, ref _breadcrumb, value);
    }
}