using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using L5Spex.Client.Messages;
using L5Spex.Client.Model;

namespace L5Spex.Client.ViewModels;

public partial class ShellViewModel : ObservableRecipient, IRecipient<ShowDialogMessage<Specification>>
{
    
    public void Receive(ShowDialogMessage<Specification> message)
    {
        throw new System.NotImplementedException();
    }
}