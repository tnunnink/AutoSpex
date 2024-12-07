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
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Range = AutoSpex.Engine.Range;

namespace AutoSpex.Client.Observers;

public partial class CriterionObserver : Observer<Criterion>,
    IRecipient<PropertyInput.GetInputTo>
{
    public CriterionObserver(Criterion model) : base(model)
    {
        Property = new PropertyInput(this);
        Argument = new ArgumentInput(() => Model.Argument, x => Model.Argument = x, () => Property.Value);

        Track(Property);
        Track(nameof(Negation));
        Track(nameof(Operation));
        Track(Argument);
    }

    /// <summary>
    /// The <see cref="PropertyInput"/> that wraps this model and the underlying Property. This observer contains logic
    /// for getting, setting, and finding suggestions for this criterion instance.
    /// </summary>
    public PropertyInput Property { get; }

    /// <summary>
    /// The negation option for the criterion.
    /// </summary>
    [Required]
    public Negation Negation
    {
        get => Model.Negation;
        set => SetProperty(Model.Negation, value, Model, (c, v) => c.Negation = v);
    }

    /// <summary>
    /// The selected operation type for the criterion.
    /// </summary>
    [Required]
    public Operation Operation
    {
        get => Model.Operation;
        set => SetProperty(Model.Operation, value, Model, (c, o) => c.Operation = o, true);
    }

    /// <summary>
    /// The argument value for the criterion. This is using a specialized observer wrappers to assist with getting,
    /// setting, parsing, and suggesting values for the argument input.
    /// </summary>
    public ArgumentInput Argument { get; }

    /// <summary>
    /// Gets the collection of supported operations based on the current selected <see cref="Property"/>.
    /// </summary>
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> Operations => GetOperations;

    /// <summary>
    /// Gets a value indicating whether the criterion accepts arguments based on its current <see cref="Operation"/>.
    /// </summary>
    public bool AcceptsArgs => Operation != Operation.None && Operation is not UnaryOperation;

    /// <inheritdoc />
    protected override bool PromptForDeletion => false;

    /// <summary>
    /// Checks if the specified ArgumentInput is contained within the current object or any inner/nested criterion
    /// argument value.
    /// </summary>
    /// <param name="argument">The argument to check for containment.</param>
    /// <returns>True if the argument is contained, otherwise false.</returns>
    public bool Contains(ArgumentInput argument)
    {
        if (Argument.Is(argument)) return true;
        if (Argument.Value is CriterionObserver inner) return inner.Contains(argument);
        return false;
    }

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
                Operation = Operation.Supports(Property.Value) ? Operation : Operation.None;
                break;
            case nameof(Operation):
                OnPropertyChanged(nameof(AcceptsArgs));
                ResetArgument();
                break;
        }
    }

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        FilterText = filter;

        return Property.Path.Satisfies(filter)
               || Negation.Name.Satisfies(filter)
               || Operation.Name.Satisfies(filter)
               || Argument.Filter(filter);
    }

    #region Commands

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

    /// <inheritdoc />
    protected override Task Move(object? source)
    {
        if (source is not CriterionObserver criterion) return Task.CompletedTask;
        if (!TryGetCollection(out var criteria)) return Task.CompletedTask;

        MoveItem(criteria, criterion);

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
        if (!TryGetCollection(out var criteria)) return false;
        return criteria.Has(this) && criteria.Has(other);
    }

    /// <inheritdoc />
    protected override Task Duplicate()
    {
        if (!TryGetCollection(out var criteria) || !criteria.Has(this)) return Task.CompletedTask;
        criteria.Add(new CriterionObserver(Model.Duplicate()));
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

    #endregion

    #region Messages

    /// <summary>
    /// Handles requests for a property input origin value. Since a criterion may contain nested criterion instance,
    /// and the nested instance will contain a <see cref="PropertyInput"/> object, we need to handle requests for the
    /// parent property type to the nested object here.
    /// </summary>
    public void Receive(PropertyInput.GetInputTo message)
    {
        if (message.HasReceivedResponse) return;
        if (Argument.Value is not CriterionObserver criterion) return;
        if (message.Observer is not CriterionObserver observer) return;
        if (!observer.Is(criterion)) return;

        //For nested criterion the input type is the inner type of this collection property.
        var input = Engine.Property.This(Property.Value.InnerType);
        message.Reply(input);
    }

    #endregion

    /// <summary>
    /// Gets all <see cref="Engine.Operation"/> types that are supported by the current configured property, and filters
    /// them based on the entry text.
    /// </summary>
    private Task<IEnumerable<object>> GetOperations(string? filter, CancellationToken token)
    {
        var operations = Operation.Supporting(Property.Value).ToList();

        if (string.IsNullOrEmpty(filter))
            return Task.FromResult(operations.Cast<object>());

        var filtered = operations
            .Where(o => o.Name.Satisfies(filter))
            .OrderByDescending(p => p.Name.StartsWith(filter))
            .ThenBy(p => p.Name);

        return Task.FromResult(filtered.Cast<object>());
    }

    /// <summary>
    /// Updates the criterion argument based on the selected operation.
    /// Some operations type expect a certain object type (Between, In, Collection types).
    /// </summary>
    private void ResetArgument()
    {
        Model.Argument = Operation switch
        {
            BetweenOperation => new Range(),
            InOperation => new List<object?>(),
            CollectionOperation => new Criterion(),
            _ => null
        };

        Argument.Refresh();
    }

    /// <summary>
    /// Tries to get the corresponding StepObserver for this criterion instance.
    /// </summary>
    private bool TryGetCollection(out ObserverCollection<Criterion, CriterionObserver> collection)
    {
        var step = GetObserver<StepObserver>(s => s.Criteria.Has(this));

        if (step is null)
        {
            collection = default!;
            return false;
        }

        collection = step.Criteria;
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
            Command = DeleteCommand,
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

    public static implicit operator Criterion(CriterionObserver observer) => observer.Model;
    public static implicit operator CriterionObserver(Criterion model) => new(model);
}