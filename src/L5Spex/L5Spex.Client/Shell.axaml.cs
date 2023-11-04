using Avalonia;
using Avalonia.Controls;

namespace L5Spex.Client;

public partial class Shell : Window
{
    public Shell()
    {
        InitializeComponent();
        
#if DEBUG
        this.AttachDevTools();
#endif
    }
}