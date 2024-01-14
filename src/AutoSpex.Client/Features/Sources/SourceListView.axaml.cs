using Avalonia.Controls;

namespace AutoSpex.Client.Features;

public partial class SourceListView : UserControl
{
    public SourceListView()
    {
        InitializeComponent();
        DataContext = Container.Resolve<SourceListViewModel>();
    }
}