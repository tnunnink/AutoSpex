using Avalonia.Controls;

namespace AutoSpex.Client.Features.Sources;

public partial class AddSourceView : UserControl
{
    public AddSourceView()
    {
        InitializeComponent();
        DataContext = App.Container.GetInstance<AddSourceViewModel>();
    }
}