using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class ActionsPageModel : PageViewModel,
    IRecipient<NavigationRequest>,
    IRecipient<Observer.GetSelected>,
    IRecipient<Observer.Deleted>
{
    private readonly SourceObserver _source;

    /// <inheritdoc/>
    public ActionsPageModel(SourceObserver source) : base("Actions")
    {
        _source = source;

        Rules = new ObserverCollection<Action, ActionObserver>(
            refresh: () => _source.Model.Rules.Select(m => new ActionObserver(m)).ToList(),
            add: (_, m) => _source.Model.AddRule(m),
            remove: (_, m) => _source.Model.RemoveRule(m),
            clear: () => _source.Model.ClearRules(),
            count: () => _source.Model.Rules.Count()
        );

        Track(Rules);
    }

    public override string Route => $"Source/{_source.Id}/{Title}";
    public override string Icon => "IconLineSliders";
    public ObserverCollection<Action, ActionObserver> Rules { get; }
    public ObservableCollection<ActionObserver> Selected { get; } = [];

    [ObservableProperty] private ActionConfigPageModel? _configPage;

    [RelayCommand]
    private async Task AddRule()
    {
        var rule = await Prompter.Show<ActionObserver?>(() => new AddActionPageModel());
        if (rule is null) return;
        Rules.Add(rule);
        Rules.Sort(r => r.Name);
    }

    [RelayCommand(CanExecute = nameof(CanAddNode))]
    private void AddNode(object? input)
    {
        if (input is not NodeObserver observer) return;

        //todo prompt the user for a reason
        var reason = "These don't apply";

        var rules = observer.Model.Descendants(NodeType.Spec).Select(n => Action.Suppress(n.NodeId, reason));
        Rules.AddRange(rules.Select(s => new ActionObserver(s)));
    }

    private static bool CanAddNode(object? input) => input is NodeObserver;


    /// <summary>
    /// Handle request to open the rule config page over this current page to allow the user to configure the settings.
    /// Destroy the page instance when closed. 
    /// </summary>
    public void Receive(NavigationRequest message)
    {
        if (message.Page is not ActionConfigPageModel config) return;

        ConfigPage = message.Action switch
        {
            NavigationAction.Open => config,
            NavigationAction.Close when ConfigPage is not null => null,
            _ => ConfigPage
        };
    }

    /// <summary>
    /// Handle the request to get selected rules for this list control.
    /// </summary>
    public void Receive(Observer.GetSelected message)
    {
        if (message.Observer is not ActionObserver observer) return;
        if (!Rules.Any(s => s.Is(observer))) return;

        foreach (var item in Selected)
            message.Reply(item);
    }

    /// <summary>
    /// Handle the request to delete the selected rule from the collection of rules.
    /// </summary>
    public void Receive(Observer.Deleted message)
    {
        if (message.Observer is not ActionObserver observer) return;
        Selected.Clear();
        Rules.Remove(observer);

        if (ConfigPage is not null && ConfigPage.Action.Is(observer))
        {
            Navigator.Close(ConfigPage);
        }
    }

    /// <inheritdoc />
    protected override void FilterChanged(string? filter)
    {
        Rules.Filter(filter);
    }
}