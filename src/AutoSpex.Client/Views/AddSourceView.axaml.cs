using AutoSpex.Client.ViewModels;
using Avalonia.Controls;

namespace AutoSpex.Client.Views;

public partial class AddSourceView : UserControl
{
    public AddSourceView()
    {
        InitializeComponent();
        DataContext = Container.Resolve<AddSourceViewModel>();
    }
}