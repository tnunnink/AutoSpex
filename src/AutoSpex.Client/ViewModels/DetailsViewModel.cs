using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Features;

[UsedImplicitly]
public partial class DetailsViewModel : ViewModelBase, IRecipient<OpenDetailMessage>
{
    [ObservableProperty] private ObservableCollection<DetailViewModel> _details = new();

    [ObservableProperty] private DetailViewModel? _selected;

    public DetailsViewModel()
    {
        Messenger.RegisterAll(this);
    }

    public void Receive(OpenDetailMessage message)
    {
        ShowNode(message.Details, message.InNewTab);
        /*Dispatcher.UIThread.Post(() => message.Details = message.NewItem);*/
    }

    [RelayCommand]
    private void CloseNode(DetailViewModel details)
    {
        if (details.IsChanged)
        {
            //todo prompt user.
        }

        Details.Remove(details);
        details.Dispose();
    }

    private void ShowNode(DetailViewModel details, bool newTab)
    {
        var existing = Details.SingleOrDefault(x => x.Id == details.Id);
        if (existing is not null)
        {
            Selected = details;
            return;
        }

        if (newTab)
        {
            Details.Add(details);
            Selected = details;
            return;
        }

        for (var i = 0; i < Details.Count; i++)
        {
            if (Details[i].IsChanged) continue;
            Details[i] = details;
            Selected = Details[i];
            return;
        }

        Details.Add(details);
        Selected = details;
    }
}