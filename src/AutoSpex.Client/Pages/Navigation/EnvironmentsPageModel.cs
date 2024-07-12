using System;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class EnvironmentsPageModel : PageViewModel,
    IRecipient<Observer.Created>,
    IRecipient<Observer.Deleted>,
    IRecipient<Observer.Renamed>
{
    public override string Route => "Environments";
    public override string Title => "Environments";
    public override string Icon => "Environments";
    public ObserverCollection<Environment, EnvironmentObserver> Environments { get; } = [];
    public ObservableCollection<EnvironmentObserver> Selected { get; } = [];
    
    [ObservableProperty] private string? _filter;

    public override async Task Load()
    {
        var result = await Mediator.Send(new ListEnvironments());
        if (result.IsFailed) return;
        
        Environments.Refresh(result.Value.Select(e => new EnvironmentObserver(e)));
        Environments.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);
    }

    [RelayCommand]
    private async Task AddEnvironment()
    {
        var environment = new Environment();
        var result = await Mediator.Send(new CreateEnvironment(environment));
        if (result.IsFailed) return;
        
        var observer = new EnvironmentObserver(environment) { IsNew = true };

        Environments.Add(observer);
        Environments.Sort(n => n.Name, StringComparer.OrdinalIgnoreCase);

        Selected.Clear();
        observer.IsSelected = true;

        await Navigator.Navigate(observer);
    }

    public void Receive(Observer.Created message)
    {
    }

    public void Receive(Observer.Deleted message)
    {
    }

    public void Receive(Observer.Renamed message)
    {
    }
    
    /// <summary>
    /// When the filter text changes apply the filter function to filter the collection.
    /// </summary>
    partial void OnFilterChanged(string? value)
    {
        foreach (var environment in Environments)
        {
            environment.Filter(value);
        }
    }
}