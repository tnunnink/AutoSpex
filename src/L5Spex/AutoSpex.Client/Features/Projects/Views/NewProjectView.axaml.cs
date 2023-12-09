using Avalonia.Controls;

namespace AutoSpex.Client.Features.Projects;

public partial class NewProjectView : UserControl
{
    public NewProjectView()
    {
        InitializeComponent();
        DataContext = App.Container.GetInstance<NewProjectViewModel>();
    }
}