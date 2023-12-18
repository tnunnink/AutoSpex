using Avalonia.Controls;

namespace AutoSpex.Client.Windows;

public partial class LauncherView : Window
{
    public LauncherView()
    {
        InitializeComponent();
        DataContext = App.Container.GetInstance<LauncherViewModel>();
    }
}