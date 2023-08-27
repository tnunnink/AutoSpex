using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace L5Spex.Views;

public partial class AppTreeView : UserControl
{
    public AppTreeView()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}