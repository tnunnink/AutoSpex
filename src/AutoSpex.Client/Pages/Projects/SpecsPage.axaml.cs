using Avalonia.Controls;

namespace AutoSpex.Client.Pages.Projects;

public partial class SpecsPage : UserControl
{
    public SpecsPage()
    {
        InitializeComponent();
    }
    
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
}