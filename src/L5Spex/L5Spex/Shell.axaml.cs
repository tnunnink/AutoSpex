using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace L5Spex;

public partial class Shell : Window
{
    public Shell()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}