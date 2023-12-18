using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AutoSpex.Client.Features.Projects;

public partial class ProjectStartupView : UserControl
{
    public ProjectStartupView()
    {
        InitializeComponent();
        DataContext = App.Container.GetInstance<ProjectStartupViewModel>();
    }
}