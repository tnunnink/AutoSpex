using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using L5Spex.Features.Sets.AddSet;
using L5Spex.Messages;
using L5Spex.Model;

namespace L5Spex.Views;

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