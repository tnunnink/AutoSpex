using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AutoSpex.Client.Pages;

public partial class RunnerPage : UserControl
{
    public RunnerPage()
    {
        InitializeComponent();
    }


    /*private void OnExpandButtonClick(object? sender, RoutedEventArgs e)
    {
        foreach (var container in ResultTree.GetRealizedTreeContainers())
        {
            if (container is not TreeViewItem item) return;
            ResultTree.ExpandSubTree(item);
        }
    }

    private void OnCollapseButtonClick(object? sender, RoutedEventArgs e)
    {
        foreach (var container in ResultTree.GetRealizedTreeContainers())
        {
            if (container is not TreeViewItem item) return;
            ResultTree.CollapseSubTree(item);
        }
    }*/
}