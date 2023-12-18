using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AutoSpex.Client.Features.Projects;

public partial class ProjectListView : UserControl
{
    public ProjectListView()
    {
        InitializeComponent();
        DataContext = App.Container.GetInstance<ProjectListViewModel>();
    }

    /// <summary>
    /// If this control is attached to a flyout menu then I want to close it have the user clicks a add or open button.
    /// </summary>
    private void OnActionClicked(object? sender, RoutedEventArgs e)
    {
        var flyout = this.FindAncestorOfType<Flyout>();
        flyout?.Hide();
    }
}