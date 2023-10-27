using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace L5Spex.Messages;

public class ShowDialogMessage<TResult> : AsyncRequestMessage<TResult>
{
    public string Title { get; set; }
    public Control Content { get; set; }
    public TResult Result { get; set; }
}