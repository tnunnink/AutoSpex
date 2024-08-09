using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class EnvironmentListPageModel : PageViewModel,
    IRecipient<Observer.Created>,
    IRecipient<Observer.Deleted>,
    IRecipient<Observer.GetSelected>,
    IRecipient<EnvironmentObserver.Targeted>
{
    public ObserverCollection<Environment, EnvironmentObserver> Environments { get; private set; } = [];
    public ObservableCollection<EnvironmentObserver> Selected { get; } = [];

    [ObservableProperty] private EnvironmentObserver? _targeted;

    [ObservableProperty] private string? _filter;

    [ObservableProperty] private int _total;

    /// <inheritdoc />
    public override async Task Load()
    {
        var result = await Mediator.Send(new ListEnvironments());
        if (result.IsFailed) return;

        Environments = new ObserverCollection<Environment, EnvironmentObserver>(
            result.Value.ToList(),
            e => new EnvironmentObserver(e)
        );

        Environments.Sort(n => n.Name);
        Total = Environments.Count;
        Targeted = Environments.SingleOrDefault(e => e.IsTarget);
    }

    /// <summary>
    /// Command to create a new <see cref="EnvironmentObserver"/> and navigate it into the view of the application.
    /// </summary>
    [RelayCommand]
    private async Task NewEnvironment()
    {
        var environment = new Environment();

        var result = await Mediator.Send(new CreateEnvironment(environment));
        if (result.IsFailed) return;

        var observer = new EnvironmentObserver(environment) { IsNew = true };
        Messenger.Send(new Observer.Created(observer));

        await Navigator.Navigate(observer);

        if (Targeted is null)
        {
            await observer.TargetCommand.ExecuteAsync(null);
        }
    }

    /// <summary>
    /// When an environment is created in the application we need to add it to our list to be displayed/navigable.
    /// </summary>
    public void Receive(Observer.Created message)
    {
        if (message.Observer is not EnvironmentObserver observer) return;
        if (Environments.Contains(observer)) return;
        Environments.Add(observer);
        Environments.Sort(x => x.Name);
        Total = Environments.Count;
    }

    /// <summary>
    /// When an environment is removed from the application we need to remove it from our list.
    /// </summary>
    public void Receive(Observer.Deleted message)
    {
        if (message.Observer is not EnvironmentObserver observer) return;

        var removed = Environments.Remove(observer);
        if (!removed) return;

        Environments.Sort(x => x.Name);
        Total = Environments.Count;

        //Reset the targeted reference if it was the observer that was deleted.
        //This forces the user to select or create a new environment.
        if (Targeted is not null && observer == Targeted)
        {
            Targeted = null;
        }
    }

    /// <summary>
    /// Returns the selected environment observers from the <see cref="Selected"/> collection.
    /// </summary>
    public void Receive(Observer.GetSelected message)
    {
        if (message.Observer is not EnvironmentObserver environment) return;
        if (!Environments.Any(e => e.Is(environment))) return;

        foreach (var observer in Selected.ToList())
            message.Reply(observer);
    }

    /// <summary>
    /// When the user targets an environment from anywhere in the app, update the local
    /// <see cref="Targeted"/> reference.
    /// </summary>
    public void Receive(EnvironmentObserver.Targeted message)
    {
        Targeted = message.Environment;
    }

    /// <summary>
    /// Apply the text filter to the environments list when changed.
    /// </summary>
    partial void OnFilterChanged(string? value)
    {
        Environments.Filter(value);
    }
}