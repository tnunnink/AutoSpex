using Avalonia.Controls;

namespace AutoSpex.Client.Pages;

public partial class AppPage : UserControl
{
    public AppPage()
    {
        InitializeComponent();
        DataContext = App.Container.GetInstance<AppPageModel>();
    }
}