using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Shared;

public abstract partial class ViewComponent(IMessenger? messenger = default) : UserControl
{
    public IMessenger Messenger { get; } = messenger ?? Container.Resolve<IMessenger>();
}