using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using L5Sharp.Core;
using Argument = AutoSpex.Engine.Argument;

namespace AutoSpex.Client.Observers;

public partial class CriterionObserver : Observer<Criterion>,
    IRecipient<Observer.Get<CriterionObserver>>,
    IRecipient<ArgumentObserver.SuggestionRequest>
{
    public CriterionObserver(Criterion model) : base(model)
    {
        Argument = new ArgumentObserver(Model.Argument);

        Track(nameof(Property));
        Track(nameof(Negation));
        Track(nameof(Operation));
        Track(Argument);
    }

    [Required]
    public Property Property
    {
        get => Model.Property;
        set => SetProperty(Model.Property, value, Model, (c, p) => c.Property = p);
    }

    [Required]
    public Negation Negation
    {
        get => Model.Negation;
        set => SetProperty(Model.Negation, value, Model, (c, v) => c.Negation = v);
    }

    [Required]
    public Operation Operation
    {
        get => Model.Operation;
        set => SetProperty(Model.Operation, value, Model, (c, o) => c.Operation = o, true);
    }

    [ObservableProperty] private ArgumentObserver _argument;

    public Func<string?, CancellationToken, Task<IEnumerable<object>>> PopulateProperties => GetProperties;
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> PopulateOperations => GetOperations;

    /// <inheritdoc />
    protected override bool PromptForDeletion => false;

    /// <inheritdoc />
    /// <remarks>
    /// When the property changes we want to reset the operation which in turn resets the argument.
    /// This helps prevent invalid entry. When the operation changes we want to update the argument based
    /// on the selected operation type.
    /// </remarks>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(Property):
                Operation = Operation.Supports(Property) ? Operation : Operation.None;
                break;
            case nameof(Operation):
                UpdateArgument();
                break;
            case nameof(Argument):
                Model.Argument = Argument.Model;
                break;
        }
    }

    /// <summary>
    /// Command to update the configured <see cref="Property"/> for this Criterion given the input object.
    /// This input can be text that the user types or a selected property from the suggestion popup.
    /// </summary>
    [RelayCommand]
    private void UpdateProperty(object? value)
    {
        switch (value)
        {
            case Property property:
                Property = property;
                return;
            case string path:
                Property = Property.This(Model.Type).GetProperty(path);
                return;
            case TagName tagName:
                Property = Property.This(Model.Type).GetProperty(tagName);
                return;
        }
    }

    /// <summary>
    /// Toggles the state of the criterion <see cref="Negation"/> property to make the operation negate the output.
    /// </summary>
    [RelayCommand]
    private void ToggleNegation()
    {
        Negation = Negation.Negate;
    }

    /// <summary>
    /// Updates the criterion <see cref="Operation"/> based on the provided value type.
    /// If the user provides text we can try to parse it but if not then we need to set to null/none.
    /// </summary>
    [RelayCommand]
    private void UpdateOperation(object? value)
    {
        switch (value)
        {
            case Operation operation:
                Operation = operation;
                return;
            case string text:
                var found = Operation.TryFromName(text, out var parsed);
                Operation = found ? parsed : Operation.None;
                return;
        }
    }

    /// <summary>
    /// Updates the criterion arguments collection based on the selected operation.
    /// Each operation type expects a certain number of arguments (except for In).
    /// Collection operations expect an inner criterion that's type needs to be the inner type of the collection.
    /// </summary>
    private void UpdateArgument()
    {
        Forget(Argument);
        Argument.Dispose();

        Model.Argument = Operation switch
        {
            NoneOperation => new Argument(),
            BinaryOperation => new Argument(),
            TernaryOperation => new Argument(new List<Argument> { new(), new() }),
            CollectionOperation => new Argument(new Criterion(Property.InnerType)),
            InOperation => new Argument(new List<Argument> { new() }),
            _ => Model.Argument
        };

        Argument = new ArgumentObserver(Model.Argument);
        Track(Argument);
    }

    /// <inheritdoc />
    protected override Task Move(object? source)
    {
        if (source is not CriterionObserver criterion) return Task.CompletedTask;
        if (!TryGetSpec(out var spec)) return Task.CompletedTask;

        MoveItem(spec.Filters, criterion);
        MoveItem(spec.Verifications, criterion);

        return Task.CompletedTask;

        void MoveItem(ObserverCollection<Criterion, CriterionObserver> collection, CriterionObserver observer)
        {
            if (!collection.Contains(observer)) return;

            var oldIndex = collection.IndexOf(observer);
            var thisIndex = collection.IndexOf(this);
            var newIndex = thisIndex > oldIndex ? thisIndex : thisIndex + 1;

            collection.Move(oldIndex, newIndex);
        }
    }

    /// <inheritdoc />
    protected override bool CanMove(object? source)
    {
        if (source is not CriterionObserver other) return false;
        if (this == other) return false;
        if (!TryGetSpec(out var spec)) return false;
        return (spec.Filters.Contains(this) && spec.Filters.Contains(other)) ||
               (spec.Verifications.Contains(this) && spec.Verifications.Contains(other));
    }

    /// <inheritdoc />
    protected override Task Duplicate()
    {
        if (!TryGetSpec(out var spec)) return Task.CompletedTask;

        if (spec.Filters.Has(this))
            spec.Filters.Add(new CriterionObserver(Model.Duplicate()));

        if (spec.Verifications.Has(this))
            spec.Verifications.Add(new CriterionObserver(Model.Duplicate()));

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override async Task Copy()
    {
        var clipboard = Shell.Clipboard;
        if (clipboard is null) return;

        var selected = SelectedItems.Cast<CriterionObserver>().Select(c => c.Model).ToList();
        var data = JsonSerializer.Serialize(selected);

        await clipboard.SetTextAsync(data);
    }

    /// <summary>
    /// Reply with this criterion observer instance if the id matches or the id is that of a child argument.
    /// </summary>
    public void Receive(Get<CriterionObserver> message)
    {
        if (message.HasReceivedResponse) return;

        if (message.Predicate.Invoke(this))
        {
            message.Reply(this);
        }
    }

    /// <summary>
    /// Handle the suggesstion request for an argument by replying with the potential property options.
    /// </summary>
    public void Receive(ArgumentObserver.SuggestionRequest message)
    {
        if (!Model.Contains(message.Argument)) return;

        Property.Type.GetOptions().Select(x => new ValueObserver(x))
            .Where(v => v.Filter(message.Filter)).ToList()
            .ForEach(message.Reply);
    }

    /// <summary>
    /// Retrieves a list of properties based on the specified filter.
    /// </summary>
    /// <param name="filter">The filter to apply to the properties. If null or empty, all properties are returned.</param>
    /// <param name="token">The cancellation token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of properties.</returns>
    private async Task<IEnumerable<object>> GetProperties(string? filter, CancellationToken token)
    {
        var type = Model.Type;
        var origin = Property.This(type);

        if (string.IsNullOrEmpty(filter))
        {
            return origin.Properties;
        }

        //While we are inside an indexer, we want to suggest tag names from the source instead of properties.
        if (filter.Count(x => x == '[') != filter.Count(x => x == ']'))
        {
            //Only support tag lookup for verifications because we need to use the filters to narrow the search space.
            var spec = GetObserver<SpecObserver>(x => x.Verifications.Any(v => v.Id == Id));
            if (spec is null) return [];
            var tagName = filter[(filter.LastIndexOf('[') + 1)..];
            var message = Messenger.Send(new TagNameRequest(spec, tagName));
            return await message.GetResponsesAsync(token);
        }

        var memeberIndex = filter.LastIndexOf('.');
        var path = memeberIndex > -1 ? filter[..memeberIndex] : string.Empty;
        var member = memeberIndex > -1 ? filter[(memeberIndex + 1)..] : filter;

        var property = origin.GetProperty(path);
        var properties = property.Properties;
        return properties.Where(p => p.Name.Satisfies(member)).OrderBy(p => p.Name);
    }

    /// <summary>
    /// Gets all <see cref="Engine.Operation"/> types that are supported by the current configured property, and filters
    /// them based on the entry text.
    /// </summary>
    private Task<IEnumerable<object>> GetOperations(string? filter, CancellationToken token)
    {
        var filtered = Operation.Supporting(Property).Where(o => o.Name.Satisfies(filter));
        return Task.FromResult(filtered.Cast<object>());
    }

    /// <summary>
    /// Tries to get the corresponding SpecObserver for this criterion instance.
    /// </summary>
    private bool TryGetSpec(out SpecObserver spec)
    {
        var result = GetObserver<SpecObserver>(s => s.Model.Contains(Model));

        if (result is null)
        {
            spec = default!;
            return false;
        }

        spec = result;
        return true;
    }

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateMenuItems()
    {
        yield return new MenuActionItem
        {
            Header = "Copy",
            Icon = Resource.Find("IconFilledCopy"),
            Gesture = new KeyGesture(Key.C, KeyModifiers.Control),
            Command = CopyCommand
        };

        yield return new MenuActionItem
        {
            Header = "Duplicate",
            Icon = Resource.Find("IconFilledClone"),
            Command = DuplicateCommand,
            Gesture = new KeyGesture(Key.D, KeyModifiers.Control)
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

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateContextItems()
    {
        yield return new MenuActionItem
        {
            Header = "Copy",
            Icon = Resource.Find("IconFilledCopy"),
            Gesture = new KeyGesture(Key.C, KeyModifiers.Control),
            Command = CopyCommand,
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

    /// <summary>
    /// A request message the retrieve all potential tag name suggesstions for tag type properties
    /// when the user inputs a collection indexer expression.
    /// </summary>
    public class TagNameRequest(Spec spec, string? filter) : AsyncCollectionRequestMessage<TagName>
    {
        /// <summary>
        /// The spec instance that the criterion belongs to. We need to use this to execute filters to narrow the
        /// search space down to applicable tag names.
        /// </summary>
        public Spec Spec { get; } = spec;

        /// <summary>
        /// The current entered filter text for the property which we use to filter the list further.
        /// </summary>
        public string? Filter { get; } = filter;
    }

    public static implicit operator Criterion(CriterionObserver observer) => observer.Model;
    public static implicit operator CriterionObserver(Criterion model) => new(model);
}