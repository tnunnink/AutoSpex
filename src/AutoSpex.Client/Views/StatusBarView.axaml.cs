using AutoSpex.Client.ViewModels;
using Avalonia.Controls;

namespace AutoSpex.Client.Views;

public partial class StatusBarView : UserControl
{
    public StatusBarView()
    {
        InitializeComponent();
        DataContext = Container.Resolve<StatusBarViewModel>();
    }
}