using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client;

[UsedImplicitly]
public partial class Shell : Window
{
    public Shell(IMessenger messenger)
    {
        InitializeComponent();
        
#if DEBUG
        this.AttachDevTools();
#endif

        messenger.RegisterAll(this);
    }
}