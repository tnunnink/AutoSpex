using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Pages;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;
using L5Sharp.Core;

namespace AutoSpex.Client.Observers;

public partial class SourceObserver : Observer<Source>,
    IRecipient<SourceObserver.Targeted>,
    IRecipient<ArgumentObserver.SuggestionRequest>,
    IRecipient<CriterionObserver.TagNameRequest>
{
    private readonly List<TypeGroup> _groups = [TypeGroup.Number, TypeGroup.Text, TypeGroup.Date, TypeGroup.Element];

    /// <inheritdoc/>
    public SourceObserver(Source source) : base(source)
    {
    }

    public override Guid Id => Model.SourceId;
    public override string Icon => nameof(Source);

    public override string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v);
    }

    public bool IsTarget
    {
        get => Model.IsTarget;
        private set => SetProperty(Model.IsTarget, value, Model, (s, v) => s.IsTarget = v);
    }

    public string TargetName => Model.TargetName;
    public string TargetType => Model.TargetType;
    public string ExportedOn => Model.ExportedOn;
    public string ExportedBy => Model.ExportedBy;
    public string Description => Model.Description;


    #region Commands

    /// <inheritdoc />
    protected override async Task Navigate()
    {
        var result = await Mediator.Send(new LoadSource(Id));
        if (Notifier.ShowIfFailed(result)) return;
        var source = new SourceObserver(result.Value);
        await Navigator.Navigate(source);
    }

    /// <summary>
    /// Sends a target source command to the mediator and notifies observers of the target.
    /// </summary>
    [RelayCommand]
    private async Task Target()
    {
        var result = await Mediator.Send(new TargetSource(Id));
        if (Notifier.ShowIfFailed(result)) return;
        Messenger.Send(new Targeted(result.Value));
    }

    /// <summary>
    /// Command to create and navigate a new run object using this source observer instance.
    /// </summary>
    [RelayCommand]
    private async Task Run()
    {
        //todo need to prompt for node
        var node = Node.NewCollection();

        var result = await Mediator.Send(new NewRun(node.NodeId, Id));
        if (Notifier.ShowIfFailed(result)) return;

        var run = new RunObserver(result.Value);
        await Navigator.Navigate(() => new RunDetailPageModel(run, true));
    }

    #endregion

    #region Messages

    /// <summary>
    /// Update the local <see cref="IsTarget"/> property when the message has been received to keep all instances in sync.
    /// </summary>
    public void Receive(Targeted message)
    {
        if (Id == message.Source.Id)
        {
            IsTarget = true;
            return;
        }

        IsTarget = false;
    }

    /// <summary>
    /// Handle the request for argument suggesstions be getting possible values for the configured criterion property
    /// from this source content. This will only run for the target source that has loaded content (non-loaded content is empty anyway).
    /// We return distinct values that are retreived from all instances of the specified type.
    /// </summary>
    public void Receive(ArgumentObserver.SuggestionRequest message)
    {
        if (!IsTarget) return;

        var property = message.Argument.Criterion?.Property ?? Property.Default;

        //Only search source for numbers, text, dates, or elements.
        //bools and enums are static and everything else is internal or not wanted.
        if (!_groups.Contains(property.Group)) return;

        try
        {
            //Since every property origin is/should be the L5Sharp type,
            //we can use that to query for elements in the source.
            var elements = Model.Content.Query(property.Origin);

            //Essentially just get non-null distinct values for the specified property
            //for all elements of the origin type.
            var values = elements.Select(property.GetValue)
                .Where(x => x is not null)
                .Distinct()
                .Select(x => new ValueObserver(x))
                .Where(x => x.Filter(message.Filter))
                .ToList();

            values.ForEach(message.Reply);
        }
        catch (Exception)
        {
            // ignored because this is just optional.
            // If the user enteres invalid property it will result in and errored evaluation telling them the issue.
        }
    }

    /// <summary>
    /// Handles the request for tag names to suggest to the user as then enter text in a property entry with indexer
    /// notation. This is very usefuly because we don't need to look up the tag structure,
    /// and instead we can have it prompted to us.
    /// </summary>
    public void Receive(CriterionObserver.TagNameRequest message)
    {
        //This only applies to tag elements. Guard agains anything else.
        if (message.Spec.Element != Element.Tag) return;

        try
        {
            //Ideally we want to narrow the search space for tag names using the currently configured filters to
            //improve the performance of this lookup which will happen continuously as text changes
            var elements = message.Spec.GetCandidates(Model.Content);

            var tagNames = elements.Cast<Tag>()
                .SelectMany(t => t.TagNames())
                .Select(t => t.Path)
                .Distinct()
                .Where(t => !string.IsNullOrEmpty(t) && t.Satisfies(message.Filter))
                .OrderBy(t => t)
                .Select(t => new TagName($"[{t}]"))
                .ToList();

            tagNames.ForEach(message.Reply);
        }
        catch (Exception)
        {
            // ignored because this is just optional.
            // If the user enteres invalid tagnames it will result in and errored evaluation telling them the issue.
        }
    }

    /// <summary>
    /// A message that notifies when a source is selected as the target source to be run.
    /// </summary>
    /// <param name="Source">The source instance that was targeted.</param>
    public record Targeted(SourceObserver Source);

    #endregion


    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        FilterText = filter;
        return Name.Satisfies(filter) || Description.Satisfies(filter) || TargetType.Satisfies(filter);
    }

    /// <inheritdoc />
    protected override Task<Result> UpdateName(string name)
    {
        Model.Name = name;
        return Mediator.Send(new RenameSource(Model));
    }

    /// <inheritdoc />
    protected override Task<Result> DeleteItems(IEnumerable<Observer> observers)
    {
        return Mediator.Send(new DeleteSources(observers.Select(o => o.Id)));
    }

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateMenuItems()
    {
        yield return new MenuActionItem
        {
            Header = "Run",
            Icon = Resource.Find("IconFilledLightning"),
            Classes = "accent",
            Command = RunCommand
        };

        yield return new MenuActionItem
        {
            Header = "Target",
            Icon = Resource.Find("IconLineTarget"),
            Command = TargetCommand
        };

        yield return new MenuActionItem
        {
            Header = "Open",
            Icon = Resource.Find("IconLineLaunch"),
            Command = NavigateCommand
        };

        yield return new MenuActionItem
        {
            Header = "Rename",
            Icon = Resource.Find("IconFilledPencil"),
            Command = RenameCommand,
            Gesture = new KeyGesture(Key.E, KeyModifiers.Control),
        };

        yield return new MenuActionItem
        {
            Header = "Duplicate",
            Icon = Resource.Find("IconFilledClone"),
            Command = DuplicateCommand,
            Gesture = new KeyGesture(Key.D, KeyModifiers.Control),
        };

        yield return new MenuActionItem
        {
            Header = "Delete",
            Icon = Resource.Find("IconFilledTrash"),
            Classes = "danger",
            Command = DeleteCommand,
            Gesture = new KeyGesture(Key.Delete)
        };
    }

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateContextItems()
    {
        yield return new MenuActionItem
        {
            Header = "Run",
            Icon = Resource.Find("IconFilledLightning"),
            Classes = "accent",
            Command = RunCommand,
            DetermineVisibility = () => HasSingleSelection
        };

        yield return new MenuActionItem
        {
            Header = "Target",
            Icon = Resource.Find("IconLineTarget"),
            Command = TargetCommand,
            DetermineVisibility = () => HasSingleSelection
        };

        yield return new MenuActionItem
        {
            Header = "Open",
            Icon = Resource.Find("IconLineLaunch"),
            Command = NavigateCommand,
            Gesture = new KeyGesture(Key.Enter),
            DetermineVisibility = () => HasSingleSelection
        };

        yield return new MenuActionItem
        {
            Header = "Rename",
            Icon = Resource.Find("IconFilledPencil"),
            Command = RenameCommand,
            Gesture = new KeyGesture(Key.E, KeyModifiers.Control),
            DetermineVisibility = () => HasSingleSelection
        };

        yield return new MenuActionItem
        {
            Header = "Duplicate",
            Icon = Resource.Find("IconFilledClone"),
            Command = DuplicateCommand,
            Gesture = new KeyGesture(Key.D, KeyModifiers.Control),
            DetermineVisibility = () => HasSingleSelection
        };

        yield return new MenuActionItem
        {
            Header = "Delete",
            Icon = Resource.Find("IconFilledTrash"),
            Classes = "danger",
            Command = DeleteSelectedCommand,
            Gesture = new KeyGesture(Key.Delete)
        };
    }

    public static implicit operator Source(SourceObserver observer) => observer.Model;
    public static implicit operator SourceObserver(Source model) => new(model);
}