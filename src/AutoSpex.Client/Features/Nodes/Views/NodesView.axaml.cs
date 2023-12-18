using Avalonia.Controls;

namespace AutoSpex.Client.Features.Nodes;

public partial class NodesView : UserControl
{
    public NodesView()
    {
        InitializeComponent();
        DataContext = App.Container.GetInstance<NodesViewModel>();
    }
}