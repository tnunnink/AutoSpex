using AutoSpex.Client.Shared;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;

namespace AutoSpex.Client.Features;

public partial class ElementMenuView : ViewBase<ElementMenuViewModel>
{
    public ElementMenuView()
    {
        InitializeComponent();
    }

    private void OnListPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var popup = this.FindLogicalAncestorOfType<Popup>();
        if (popup is null) return;
        popup.IsOpen = false;
    }
}