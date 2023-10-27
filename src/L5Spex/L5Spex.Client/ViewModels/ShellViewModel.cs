using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using L5Spex.Messages;
using L5Spex.Model;

namespace L5Spex.ViewModels;

public partial class ShellViewModel : ObservableRecipient, IRecipient<ShowDialogMessage<Specification>>
{
    
    public void Receive(ShowDialogMessage<Specification> message)
    {
        throw new System.NotImplementedException();
    }
}