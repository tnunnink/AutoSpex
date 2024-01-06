using AutoSpex.Client.ViewModels;
using Avalonia.Controls;

namespace AutoSpex.Client.Views;

public partial class ProjectStartupView : UserControl
{
    public ProjectStartupView()
    {
        InitializeComponent();
        DataContext = Container.Resolve<ProjectStartupViewModel>();
    }
}