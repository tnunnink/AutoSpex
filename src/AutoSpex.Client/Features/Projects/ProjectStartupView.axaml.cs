using Avalonia.Controls;

namespace AutoSpex.Client.Features;

public partial class ProjectStartupView : UserControl
{
    public ProjectStartupView()
    {
        InitializeComponent();
        DataContext = Container.Resolve<ProjectStartupViewModel>();
    }
}