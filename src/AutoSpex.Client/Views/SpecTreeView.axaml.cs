using AutoSpex.Client.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using NodeObserver = AutoSpex.Client.Observers.NodeObserver;

namespace AutoSpex.Client.Views;

public partial class SpecTreeView : UserControl
{
    public SpecTreeView()
    {
        InitializeComponent();
        DataContext = Container.Resolve<SpecTreeViewModel>();
        ViewModel = (DataContext as SpecTreeViewModel)!;
    }

    public SpecTreeViewModel ViewModel { get; }
    
    /*private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)sender;
        var node = (Node)textBox.DataContext!;
        node.IsEditing = false;
    }

    private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter && e.Key != Key.Escape) return;
        
        var textBox = (TextBox)sender;
        var node = (Node)textBox.DataContext!;

        if (e.Key == Key.Escape)
        {
            node.IsEditing = false;
            return;
        }

        if (!string.IsNullOrWhiteSpace(textBox.Text) && !string.IsNullOrEmpty(textBox.Text))
        {
            node.Name = textBox.Text;
            var vm = (SpecTreeViewModel) DataContext!;
            vm.Rename(node);
        }

        e.Handled = true;
        node.IsEditing = false;
    }*/
    /*private void OnSpecTreeTapped(object? sender, TappedEventArgs e)
    {
        e.
        if (e.Source is not Control{DataContext: Node node} control) return;
        var 
    }*/

    private void OnTreePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.Source is not Control{DataContext: NodeObserver node} control) return;
        
        var point = e.GetCurrentPoint(control);
        
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            ViewModel.OpenCommand.Execute(node);
        }
    }
}