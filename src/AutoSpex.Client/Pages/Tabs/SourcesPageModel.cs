﻿using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SourcesPageModel(EnvironmentObserver environment) : PageViewModel("Sources"),
    IRecipient<Observer.GetSelected>
{
    public override string Route => $"{nameof(Environment)}/{Environment.Id}/{Title}";
    public EnvironmentObserver Environment { get; } = environment;
    public ObservableCollection<SourceObserver> Selected { get; } = [];

    [ObservableProperty] private string? _filter;

    [RelayCommand]
    private async Task AddSource()
    {
        var uri = await Shell.StorageProvider.SelectSourceFile();
        if (uri is null) return;

        var observer = new SourceObserver(new Source(uri));
        Environment.Sources.Add(observer);
    }

    public void Receive(Observer.GetSelected message)
    {
        if (message.Observer is not SourceObserver source) return;
        if (!Selected.Any(x => x.Is(source))) return;

        foreach (var selected in Selected)
        {
            message.Reply(selected);
        }
    }

    partial void OnFilterChanged(string? value)
    {
        Environment.Sources.Filter(value);
    }
}