using Avalonia.Controls;
using L5Spex.Client.ViewModels;

namespace L5Spex.Client.Views;

public partial class AppView : UserControl
{
    public AppView()
    {
        InitializeComponent();
        DataContext = App.Instance.Container.GetInstance<AppViewModel>();
    }
}