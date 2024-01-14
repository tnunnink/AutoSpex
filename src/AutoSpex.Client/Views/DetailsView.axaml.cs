using Avalonia.Controls;

namespace AutoSpex.Client.Features;

public partial class NodesView : UserControl
{
    public NodesView()
    {
        InitializeComponent();
        DataContext = Container.Resolve<DetailsViewModel>();
    }
}