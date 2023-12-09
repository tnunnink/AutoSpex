using AutoSpex.Client.Features.Nodes;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AutoSpex.Client.Features.Specifications;

public partial class SpecTreeView : UserControl
{
    public SpecTreeView()
    {
        InitializeComponent();
        DataContext = App.Container.GetInstance<SpecTreeViewModel>();
        
        // found2 = true | result2 = "Hello World" 
        var found2 = this.TryFindResource("TheKey", ActualThemeVariant, out var result2);
    }
    
    private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
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
    }
}