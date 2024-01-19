using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Shared;

public abstract class PageView<TViewModel> : UserControl where TViewModel : PageViewModel
{
    protected TViewModel ViewModel =>
        DataContext as TViewModel ?? throw new InvalidOperationException(
            $"View {GetType().Name} DataContext must be of type {typeof(TViewModel)}.");
}

public abstract partial class ViewComponent(IMessenger? messenger = default) : UserControl
{
    public IMessenger Messenger { get; } = messenger ?? Container.Resolve<IMessenger>();
}