using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class CriterionObserver : Observer<Criterion>, IRecipient<Observer.Get<CriterionObserver>>
{
    public CriterionObserver(Criterion model) : base(model)
    {
        Argument = new ArgumentObserver(Model.Argument);

        Track(nameof(Property));
        Track(nameof(Negation));
        Track(nameof(Operation));
        Track(Argument);
    }

    public override Guid Id => Model.CriterionId;

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
                Operation = Operation.None;
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
                Property = Property.This(Model.Type).GetProperty(path) ?? Property.Default;
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
        if (Operation is NoneOperation)
        {
            Argument = new ArgumentObserver(Engine.Argument.Default);
        }

        if (Operation is BinaryOperation)
        {
            Argument = new ArgumentObserver(new Argument());
        }

        if (Operation is TernaryOperation)
        {
            Argument = new ArgumentObserver(new Argument(new List<Argument> { new(), new() }));
        }

        if (Operation is CollectionOperation)
        {
            Argument = new ArgumentObserver(new Argument(new Criterion(Property.InnerType)));
        }

        if (Operation is InOperation)
        {
            Argument = new ArgumentObserver(new Argument(new List<Argument> { new() }));
        }
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
    /// Retrieves a list of properties based on the specified filter.
    /// </summary>
    /// <param name="filter">The filter to apply to the properties. If null or empty, all properties are returned.</param>
    /// <param name="token">The cancellation token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of properties.</returns>
    private Task<IEnumerable<object>> GetProperties(string? filter, CancellationToken token)
    {
        var type = Model.Type;
        var origin = Property.This(type);

        if (string.IsNullOrEmpty(filter))
            return Task.FromResult(origin.Properties.Cast<object>());

        //We don't want to suggest properties while in the indexer part. Only after it is closed
        //Example: "[MyTagName.MyMem"
        if (filter.Count(x => x == '[') != filter.Count(x => x == ']'))
            return Task.FromResult(Enumerable.Empty<object>());

        var memeberIndex = filter.LastIndexOf('.');
        var path = memeberIndex > -1 ? filter[..memeberIndex] : string.Empty;
        var member = memeberIndex > -1 ? filter[(memeberIndex + 1)..] : filter;

        var property = origin.GetProperty(path);
        var properties = property?.Properties ?? [];

        var filtered = properties
            .Where(p => p.Name.Contains(member, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Name)
            .Cast<object>();

        return Task.FromResult(filtered);
    }

    /// <summary>
    /// Gets all <see cref="Engine.Operation"/> types that are supported by the current configured property, and filters
    /// them based on the entry text.
    /// </summary>
    private Task<IEnumerable<object>> GetOperations(string? filter, CancellationToken token)
    {
        var filtered = Operation.Supporting(Property);
        return Task.FromResult(filtered.Cast<object>());
    }

    protected override IEnumerable<MenuActionItem> GenerateContextItems()
    {
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

    public static implicit operator Criterion(CriterionObserver observer) => observer.Model;
    public static implicit operator CriterionObserver(Criterion model) => new(model);
}