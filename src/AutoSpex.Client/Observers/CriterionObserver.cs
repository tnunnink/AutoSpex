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
using L5Sharp.Core;
using Range = AutoSpex.Engine.Range;

namespace AutoSpex.Client.Observers;

public partial class CriterionObserver : Observer<Criterion>
{
    public CriterionObserver(Criterion model) : base(model)
    {
        Argument = new ValueObserver(GetArgument, ParseArgument, SetArgument, GetSuggesstions);

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

    [ObservableProperty] private ValueObserver _argument;

    public Func<string?, CancellationToken, Task<IEnumerable<object>>> PopulateProperties => GetProperties;
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> PopulateOperations => GetOperations;

    /// <summary>
    /// Gets a value indicating whether this CriterionObserver accepts arguments based on its Operation.
    /// </summary>
    /// <remarks>
    /// Returns true if the Operation is not None and not a UnaryOperation.
    /// </remarks>
    public bool AcceptsArgs => Operation != Operation.None && Operation is not UnaryOperation;

    /// <summary>
    /// Gets the input property type for this criterion instance using the containing spec object.
    /// </summary>
    private Property Input => TryGetSpec(out var spec) ? spec.Model.InputTo(Model) : Property.Default;

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
    /// Command to update the configured <see cref="Property"/> for this Criterion given the input object.
    /// This input can be text that the user types or a selected property from the suggestion popup.
    /// </summary>
    [RelayCommand]
    private void UpdateProperty(object? value)
    {
        var origin = TryGetSpec(out var spec) ? spec.Model.InputTo(Model) : Property.Default;

        switch (value)
        {
            case Property property:
                Property = property;
                return;
            case string path:
                Property = origin.GetProperty(path);
                return;
            case TagName tagName:
                Property = origin.GetProperty(tagName);
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

    /// <inheritdoc />
    protected override Task Move(object? source)
    {
        if (source is not CriterionObserver criterion) return Task.CompletedTask;
        if (!TryGetStep(out var step)) return Task.CompletedTask;

        MoveItem(step.Criteria, criterion);

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
        if (!TryGetStep(out var step)) return false;
        return step.Criteria.Has(this) && step.Criteria.Has(other);
    }

    /// <inheritdoc />
    protected override Task Duplicate()
    {
        if (!TryGetStep(out var step) || !step.Criteria.Has(this)) return Task.CompletedTask;
        step.Criteria.Add(new CriterionObserver(Model.Duplicate()));
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

    /// <summary>
    /// Updates the criterion arguments collection based on the selected operation.
    /// Each operation type expects a certain number of arguments (except for In).
    /// Collection operations expect an inner criterion that's type needs to be the inner type of the collection.
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

        Forget(Argument);
        Argument = new ValueObserver(GetArgument, ParseArgument, SetArgument, GetSuggesstions);
        Track(Argument);
    }

    /// <summary>
    /// Updates the argument based on the received value type from the entry field.
    /// If user enters simple text we would like to parse it as the strong type to let our data templates work.
    /// If user enters a value observer object, then it was selected from the suggestions, and we can just use that.
    /// Anything else just wrap in a value observer and set accordingly.
    /// </summary>
    private object? GetArgument()
    {
        return Model.Argument switch
        {
            Range range => new RangeObserver(range),
            List<object?> list => list.ToObserver(x => new ValueObserver(x)),
            Criterion criterion => new CriterionObserver(criterion),
            _ => Model.Argument
        };
    }

    /// <summary>
    /// Updates the argument based on the received value type from the entry field. It is expected that this value is
    /// the parsed value that we can directly set argument to.
    /// </summary>
    private void SetArgument(object? value)
    {
        Model.Argument = value;
    }

    /// <summary>
    /// Parses the input value to the type expected by the criterion property.
    /// If user enters simple text we would like to parse it as the strong type to let our data templates work.
    /// If user enters a value observer object, then it was selected from the suggestions, and we can just use that.
    /// Anything else just wrap in a value observer and set accordingly.
    /// </summary>
    private object? ParseArgument(object? value)
    {
        var group = Property.Group;

        return value switch
        {
            string text when group.TryParse(text, out var parsed) && parsed is not null => parsed,
            ValueObserver observer => observer.Value,
            _ => value
        };
    }

    /// <summary>
    /// Retrieves a list of properties based on the specified filter.
    /// </summary>
    /// <param name="filter">The filter to apply to the properties. If null or empty, all properties are returned.</param>
    /// <param name="token">The cancellation token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of properties.</returns>
    private async Task<IEnumerable<object>> GetProperties(string? filter, CancellationToken token)
    {
        if (!TryGetSpec(out var spec) || token.IsCancellationRequested) return [];

        var origin = spec.Model.InputTo(Model);

        if (string.IsNullOrEmpty(filter))
        {
            return origin.Properties;
        }

        //While we are inside an indexer, we want to suggest tag names from the source instead of properties.
        if (filter.Count(x => x == '[') != filter.Count(x => x == ']'))
        {
            var tagName = filter[(filter.LastIndexOf('[') + 1)..];
            return await GetTagNames(tagName, token);
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
    /// Attempts to get the loaded target source and find suggestable values based on the current propert and
    /// input filter. This code is handled in the source object, and will basically query the file and use the
    /// configured property to pull out strongly typed values so the user can easily configure the criterion as needed.
    /// </summary>
    private Task<IEnumerable<object>> GetSuggesstions(string? filter, CancellationToken token)
    {
        if (!TryGetSource(out var source) || token.IsCancellationRequested)
            return Task.FromResult(Enumerable.Empty<object>());

        var values = source.Model.FindValues(Property)
            .Select(v => new ValueObserver(v))
            .Where(x => x.Filter(filter))
            .Cast<object>();

        return Task.FromResult(values);
    }

    /// <summary>
    /// Attempts to get the loaded target source and fine suggestable tag names based on the current spec and input
    /// filter. This code is handled in the source object, and will basically query the file and use the
    /// configured data to pull out known tag names so the user can easily configure the criterion as needed.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private Task<IEnumerable<object>> GetTagNames(string? filter, CancellationToken token)
    {
        if (!TryGetSource(out var source) || !TryGetSpec(out var spec) || token.IsCancellationRequested)
            return Task.FromResult(Enumerable.Empty<object>());

        var tagNames = source.Model.FindTagNames(spec, filter).Cast<object>();

        return Task.FromResult(tagNames);
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

    /// <summary>
    /// Tries to get the corresponding StepObserver for this criterion instance.
    /// </summary>
    private bool TryGetStep(out StepObserver step)
    {
        var result = GetObserver<StepObserver>(s => s.Criteria.Any(c => c.Is(this)));

        if (result is null)
        {
            step = default!;
            return false;
        }

        step = result;
        return true;
    }

    /// <summary>
    /// Tries to get the loaded target soruce to use as the basis for finding suggestible data.
    /// </summary>
    private bool TryGetSource(out SourceObserver source)
    {
        var result = GetObserver<SourceObserver>(s => s.Model is { IsTarget: true, Content: not null });

        if (result is null)
        {
            source = default!;
            return false;
        }

        source = result;
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