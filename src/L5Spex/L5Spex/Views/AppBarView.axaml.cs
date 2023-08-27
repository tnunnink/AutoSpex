using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace L5Spex.Views;

public partial class AppBarView : UserControl
{
    public AppBarView()
    {
       InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}