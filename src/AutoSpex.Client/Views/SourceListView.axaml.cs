using AutoSpex.Client.ViewModels;
using Avalonia.Controls;

namespace AutoSpex.Client.Views;

public partial class SourceListView : UserControl
{
    public SourceListView()
    {
        InitializeComponent();
        DataContext = Container.Resolve<SourceListViewModel>();
    }
}