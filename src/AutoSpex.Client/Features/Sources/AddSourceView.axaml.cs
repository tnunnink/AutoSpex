using Avalonia.Controls;

namespace AutoSpex.Client.Features;

public partial class AddSourceView : UserControl
{
    public AddSourceView()
    {
        InitializeComponent();
        DataContext = Container.Resolve<AddSourceViewModel>();
    }
}