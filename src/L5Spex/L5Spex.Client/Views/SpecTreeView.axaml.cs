using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using L5Spex.Client.Observers;
using L5Spex.Client.ViewModels;

namespace L5Spex.Client.Views;

public partial class SpecTreeView : UserControl
{
    public SpecTreeView()
    {
        InitializeComponent();
        DataContext = App.Instance.Container.GetInstance<SpecTreeViewModel>();
    }
    
    private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)sender;
        var node = (NodeObserver)textBox.DataContext!;
        node.IsEditing = false;
    }

    private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter && e.Key != Key.Escape) return;
        
        var textBox = (TextBox)sender;
        var node = (NodeObserver)textBox.DataContext!;

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