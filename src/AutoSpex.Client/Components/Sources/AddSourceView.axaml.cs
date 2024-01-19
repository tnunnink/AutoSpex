using Avalonia.Controls;

namespace AutoSpex.Client.Components.Sources;

public partial class AddSourceView : UserControl
{
    public AddSourceView()
    {
        InitializeComponent();
        DataContext = Container.Resolve<AddSourceViewModel>();
    }
}