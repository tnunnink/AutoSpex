using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;
using Environment = AutoSpex.Engine.Environment;

namespace AutoSpex.Client.Observers;

public partial class EnvironmentObserver : Observer<Environment>,
    IRecipient<EnvironmentObserver.Targeted>,
    IRecipient<Observer.Deleted>,
    IRecipient<Observer.Duplicated>
{
    /// <inheritdoc/>
    public EnvironmentObserver(Environment model) : base(model)
    {
        Sources = new ObserverCollection<Source, SourceObserver>(Model.Sources, s => new SourceObserver(s));
        Track(nameof(Comment));
        Track(Sources);
    }
    
    public override Guid Id => Model.EnvironmentId;
    public override string Icon => nameof(Environment);

    public override string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v);
    }

    public string? Comment
    {
        get => Model.Comment;
        set => SetProperty(Model.Comment, value, Model, (s, v) => s.Comment = v);
    }

    public bool IsTarget
    {
        get => Model.IsTarget;
        private set => SetProperty(Model.IsTarget, value, Model, (s, v) => s.IsTarget = v);
    }

    public ObserverCollection<Source, SourceObserver> Sources { get; }

    [RelayCommand]
    private async Task Target()
    {
        var result = await Mediator.Send(new TargetEnvironment(Id));
        if (result.IsFailed) return;
        Messenger.Send(new Targeted(this));
    }

    /// <inheritdoc />
    protected override async Task Delete()
    {
        if (!IsSelected) return;

        var message = Messenger.Send(new GetSelected(this));
        var selected = message.Responses.Where(x => x is EnvironmentObserver).Cast<EnvironmentObserver>().ToList();

        if (selected.Count == 1)
        {
            var delete = await Prompter.PromptDeleteItem(Name);
            if (delete is not true) return;
        }
        else
        {
            var delete = await Prompter.PromptDeleteItems($"{selected.Count.ToString()} selected items");
            if (delete is not true) return;
        }

        var result = await Mediator.Send(new DeleteEnvironments(selected.Select(n => n.Id)));
        if (result.IsFailed) return;

        foreach (var deleted in selected)
            Messenger.Send(new Deleted(deleted));
    }

    /// <summary>
    /// Update the local <see cref="IsTarget"/> property when the message has been received to keep all instances in sync.
    /// </summary>
    public void Receive(Targeted message)
    {
        if (Id == message.Environment.Id)
        {
            IsTarget = true;
            return;
        }

        IsTarget = false;
    }

    /// <summary>
    /// Remove the source when requested.
    /// </summary>
    public void Receive(Deleted message)
    {
        if (message.Observer is not SourceObserver source) return;
        Sources.Remove(source);
    }

    /// <summary>
    /// Handles the duplication of a child source observer.
    /// </summary>
    /// <param name="message"></param>
    public void Receive(Duplicated message)
    {
        if (message.Source is not SourceObserver source) return;
        if (message.Duplicate is not SourceObserver duplicate) return;

        if (Sources.Contains(source) && !Sources.Contains(duplicate))
        {
            Sources.Add(duplicate);
        }
    }

    /// <inheritdoc />
    protected override Task<Result> UpdateName(string name)
    {
        Model.Name = name;
        return Mediator.Send(new RenameEnvironment(this));
    }

    /// <summary>
    /// A message that notifies when an environment is selected as the target environment to be run.
    /// </summary>
    /// <param name="Environment"></param>
    public record Targeted(EnvironmentObserver Environment);

    public static implicit operator Environment(EnvironmentObserver observer) => observer.Model;
    public static implicit operator EnvironmentObserver(Environment model) => new(model);
}