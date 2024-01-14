using Avalonia.Controls;

namespace AutoSpex.Client.Features;

public partial class StatusBarView : UserControl
{
    public StatusBarView()
    {
        InitializeComponent();
        DataContext = Container.Resolve<StatusBarViewModel>();
    }
}