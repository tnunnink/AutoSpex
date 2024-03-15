using System;
using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public class RunnerObserver(Runner model) : Observer<Runner>(model),
    IRecipient<Observer<Runner>.Deleted>,
    IRecipient<Observer<Runner>.Renamed>
{
    public override Guid Id => Model.RunnerId;

    public string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (m, s) => m.Name = s, true);
    }
    
    
    public IEnumerable<NodeObserver> Collections => Model.Collections.Select(n => new NodeObserver(n));
    public IEnumerable<NodeObserver> Specs => Model.Specs.Select(n => new NodeObserver(n));

    public static RunnerObserver New => new(new Runner());


    public async Task AddNode(NodeObserver node)
    {
        var id = node.Id;

        var result = await Mediator.Send(new GetFullNode(id));
        if (result.IsFailed) return;

        Model.AddNode(node);
        OnPropertyChanged(nameof(Collections));
        OnPropertyChanged(nameof(Spec));
    }

    #region Commands

    /// <inheritdoc />
    protected override async Task Delete()
    {
        var delete = await Prompter.PromptDelete(Name);
        if (delete is not true) return;

        var result = await Mediator.Send(new DeleteRunner(Id));
        if (result.IsFailed) return;

        Messenger.Send(new Deleted(this));
    }

    /// <inheritdoc />
    protected override async Task Rename(string? name)
    {
        if (string.IsNullOrEmpty(name) || string.Equals(name, Name) || name.Length > 100) return;

        var previous = Name;
        Name = name;

        var result = await Mediator.Send(new RenameRunner(this));

        if (result.IsFailed)
        {
            Name = previous;
            return;
        }

        Messenger.Send(new Renamed(this));
    }

    #endregion

    #region MessageHandlers

    public void Receive(Deleted message)
    {
        if (message.Observer is not RunnerObserver runner) return;
        if (Id != runner.Id) return;
        Messenger.UnregisterAll(this);
    }

    public void Receive(Renamed message)
    {
        if (message.Observer is not RunnerObserver runner) return;
        if (ReferenceEquals(this, runner)) return;
        if (Id != runner.Id) return;

        if (Name != runner.Name)
        {
            Name = runner.Name;
            Messenger.Send(new Renamed(this));
            return;
        }

        OnPropertyChanged(nameof(Name));
    }

    #endregion

    public static implicit operator Runner(RunnerObserver observer) => observer.Model;

    public static implicit operator RunnerObserver(Runner model) => new(model);
}