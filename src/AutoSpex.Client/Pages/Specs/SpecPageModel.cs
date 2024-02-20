using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class SpecPageModel(Node node) : NodePageModel(node),
    IRecipient<NavigationRequest>,
    IRecipient<ArgumentObserver.GetSuggestions>
{
    [ObservableProperty] private SpecObserver _spec = new(new Spec());

    [ObservableProperty] private PageViewModel? _outcomePage;

    protected override void OnActivated()
    {
        base.OnActivated();
        Messenger.Register<ArgumentObserver.GetSuggestions>(this);
    }

    public override async Task Load()
    {
        await base.Load();

        if (Node.Model.Spec is null)
        {
            //todo navigate failure page?
            return;
        }

        Spec = new SpecObserver(Node.Model.Spec);
        Track(Spec);

        await Navigator.Navigate(() => new OutcomePageModel(Spec));
    }

    public void Receive(NavigationRequest message)
    {
        if (message.Page is OutcomePageModel page && page.Spec == Spec)
        {
            OutcomePage = message.Page;
        }
    }

    public void Receive(ArgumentObserver.GetSuggestions message)
    {
        var criterion = message.Argument.Owner;
        if (!Spec.Model.Contains(criterion)) return;

        var suggestions = new List<Argument>();

        //First make sure to get known options if the type is an enum or boolean.
        var options = criterion.Property?.Options.Select(o => new Argument(o)) ?? Enumerable.Empty<Argument>();
        suggestions.AddRange(options);
        
        //Second get all scoped variables as potential options for the argument.
        var variables = Variables.Select(v => new Argument(v));
        suggestions.AddRange(variables);
        
        //todo get source value which we will probably load in the NodePageModel 
        
        message.Reply(suggestions);
    }
    
}