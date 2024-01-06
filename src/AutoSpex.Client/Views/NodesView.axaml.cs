using AutoSpex.Client.ViewModels;
using Avalonia.Controls;

namespace AutoSpex.Client.Views;

public partial class NodesView : UserControl
{
    public NodesView()
    {
        InitializeComponent();
        DataContext = Container.Resolve<NodesViewModel>();
    }
}