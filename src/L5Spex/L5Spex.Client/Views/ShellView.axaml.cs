using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using L5Spex.Client.Features.Sets.AddSet;

namespace L5Spex.Client.Views;

public partial class ShellView : Window , IRecipient<AddSetMessage>
{
    public ShellView()
    {
        InitializeComponent();
    }

    public void Receive(AddSetMessage message)
    {
        /*var dialog = new AddSetDialog();
        var result = dialog.ShowDialog<Set>(this);
        message.Reply(result);*/
    }
}