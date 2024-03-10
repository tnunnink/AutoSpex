using Avalonia.Controls;
using Avalonia.Input;

namespace AutoSpex.Client.Components;

public class VariableNameEntry : TextBox
{
    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        SelectAll();
        base.OnGotFocus(e);
    }
}