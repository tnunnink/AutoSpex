using System.Threading;
using AutoSpex.Client.Messages;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Features;

[UsedImplicitly]
public partial class ProcessIndicatorViewModel : ViewModelBase, IRecipient<ProcessMessage>
{
    private CancellationTokenSource? _cancellation;
    
    public ProcessIndicatorViewModel(IMessenger messenger)
    {
        messenger.RegisterAll(this);
    }
    
    [ObservableProperty] private string _process = string.Empty;

    [ObservableProperty] private bool _isActive;

    [RelayCommand]
    private void Cancel()
    {
        _cancellation?.Cancel();
    }

    public void Receive(ProcessMessage message)
    {
        Process = message.Process;
        IsActive = message.IsActive;
        _cancellation = message.Cancellation;
    }
}