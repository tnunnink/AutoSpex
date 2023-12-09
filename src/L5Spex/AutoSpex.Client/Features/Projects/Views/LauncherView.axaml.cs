using Avalonia.Controls;

namespace AutoSpex.Client.Features.Projects;

public partial class LauncherView : UserControl
{
    public LauncherView()
    {
        InitializeComponent();
        DataContext = App.Container.GetInstance<LauncherViewModel>();
    }
}