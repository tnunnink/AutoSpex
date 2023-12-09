using Avalonia.Controls;

namespace AutoSpex.Client.Features.StatusBar;

public partial class StatusBarView : UserControl
{
    public StatusBarView()
    {
        InitializeComponent();
        DataContext = App.Container.GetInstance<StatusBarViewModel>();
    }
}