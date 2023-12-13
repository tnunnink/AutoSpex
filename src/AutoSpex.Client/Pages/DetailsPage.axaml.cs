using Avalonia.Controls;

namespace AutoSpex.Client.Pages;

public partial class DetailsPage : UserControl
{
    public DetailsPage()
    {
        InitializeComponent();
        DataContext = App.Container.GetInstance<DetailsPageModel>();
    }
}