using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Argument = AutoSpex.Engine.Argument;

// ReSharper disable TailRecursiveCall

namespace AutoSpex.Client.Observers;

/// <summary>
/// A observer wrapper over the <see cref="Engine.Argument"/> class for a parent criterion object.
/// </summary>
public partial class ArgumentObserver : Observer<Argument>
{
    public ArgumentObserver(Argument argument) : base(argument)
    {
        Value = GetValue();
        Track(nameof(Value));
        Track(Value);
    }

    /// <inheritdoc />
    public override Guid Id => Model.ArgumentId;

    /// <summary>
    /// The value of the argument. This is expected to be some <see cref="ITrackable"/> observer or observer collection
    /// so that we can detect and respond to changes. The underlying value is initially set from the passed in mode
    /// depending on how it's parent criterion created it based on the selected operation. From there we either update
    /// this value directly if a binary type operations or update the nested object for inner criterion or observer collections.
    /// </summary>
    [ObservableProperty] private ITrackable _value;

    /// <summary>
    /// Gets the parent criterion that contains the argument. We need this to determine the property type and operation
    /// so that we can get suggesstable values and parse the input text correctly.
    /// </summary>
    public CriterionObserver? Criterion => GetObserver<CriterionObserver>(IsParent);

    /// <summary>
    /// The function that retrieves a collection of object values that are suggestions to the entry control.
    /// </summary>
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> Suggestions => GetSuggestions;

    /// <summary>
    /// Gets a value indicating whether the Value property is empty.
    /// This is only used for basic input entries for binrary operations.
    /// </summary>
    public bool IsEmpty => Value is ValueObserver { IsEmpty: true };

    /// <summary>
    /// Updates the argument <see cref="Value"/> based on the received value type from the entry field.
    /// If user enters simple text we would like to parse it as the strong type to let our data templates work.
    /// If the user enters a variable reference (text starting with '@') then we want to find and resolve the object.
    /// If user enters a complex object, then it was selected from the suggestions, and we can just use that.
    /// Anything else just wrap in a value observer and set accordingly.
    /// </summary>
    [RelayCommand]
    private async Task UpdateValue(object? value)
    {
        var group = Criterion?.Property.Group;

        switch (value)
        {
            case string text when text.StartsWith(Reference.Prefix):
                var reference = await ResolveReference(text);
                Value = new ReferenceObserver(reference);
                return;
            case string text when group?.TryParse(text, out var parsed) is true && parsed is not null:
                Value = new ValueObserver(parsed);
                return;
            case ValueObserver observer:
                Value = observer.Model is Variable variable ? new ReferenceObserver(variable.Reference()) : observer;
                return;
            default:
                Value = new ValueObserver(value);
                break;
        }
    }

    /// <summary>
    /// Command to add an argument value to this arguments value collection. This is only supported for the In operation,
    /// since we need to allow the user to add an somwhat unbounded list of arguments.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddArgument))]
    private void AddArgument()
    {
        if (Criterion?.Operation != Operation.In) return;
        if (Value is not ObserverCollection<Argument, ArgumentObserver> collection) return;
        collection.Add(new ArgumentObserver(new Argument()));
    }

    /// <summary>
    /// Determin if the argument should allow for arguments to be added.
    /// </summary>
    private bool CanAddArgument() =>
        Criterion?.Operation == Operation.In && Value is ObserverCollection<Argument, ArgumentObserver>;

    /// <inheritdoc />
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        //Sync the obsever value with that of the underlying model object.
        if (e.PropertyName is not nameof(Value)) return;
        SetValue(Value);
        OnPropertyChanged(nameof(IsEmpty));
    }

    /// <summary>
    /// Returns the wrapped model value if the model has a corresponding observer type.
    /// Collections are wrapped in the ObserverCollection since it implements ITrackable, so we can notify changes when the user updates the list.
    /// And default value will be wrapped in a value observer to be used with data templates
    /// </summary>
    private ITrackable GetValue()
    {
        return Model.Value switch
        {
            Reference reference => new ReferenceObserver(reference),
            Criterion criterion => new CriterionObserver(criterion),
            List<Argument> arguments => new ObserverCollection<Argument, ArgumentObserver>(arguments,
                a => new ArgumentObserver(a)),
            _ => new ValueObserver(Model.Value)
        };
    }

    /// <summary>
    /// Sets the underlying argument value based on the provided object. This object should either be a nested observer
    /// Criterion or Reference, nested observer collection of argument observers which in turn wrap single values,
    /// or some default value wrapped in a value observer.
    /// </summary>
    private void SetValue(object? value)
    {
        Model.Value = value switch
        {
            ReferenceObserver reference => reference.Model,
            CriterionObserver criterion => criterion.Model,
            ObserverCollection<Argument, ArgumentObserver> collection => collection.Select(x => x.Model).ToList(),
            ValueObserver observer => observer.Model,
            _ => value
        };
    }

    /// <summary>
    /// Executes the code to retrieve possible suggestion values based on the current argument's type and scope,
    /// and returns a collection of values which are filtered based on the provided input text.
    /// </summary>
    private async Task<IEnumerable<object>> GetSuggestions(string? filter, CancellationToken token)
    {
        var message = Messenger.Send(new SuggestionRequest(this, filter));
        var response = await message.GetResponsesAsync(token);
        return response;
    }

    /// <summary>
    /// Queries the database for a variable in scope with the specified name and returns it, or null if not found.
    /// </summary>
    private async Task<Reference> ResolveReference(string name)
    {
        var reference = new Reference(name);
        var scoped = await Mediator.Send(new GetReferenceVariable(reference));
        return scoped.IsSuccess ? scoped.Value.Reference() : reference;
    }

    /// <summary>
    /// Determines whether the specified criterion is a parent of the current argument observer.
    /// </summary>
    /// <param name="criterion">The criterion to check if it is a parent.</param>
    /// <returns>True if the criterion is a parent, otherwise false.</returns>
    private bool IsParent(CriterionObserver criterion)
    {
        if (criterion.Argument.Id == Id) return true;
        if (criterion.Argument.Value is not ObserverCollection<Argument, ArgumentObserver> collection) return false;
        if (collection.Any(a => a.Id == Id)) return true;
        return false;
    }

    /// <summary>
    /// Represents an observer wrapper over the <see cref="Engine.Argument"/> class for a parent criterion object.
    /// </summary>
    public class SuggestionRequest(ArgumentObserver argument, string? filter)
        : AsyncCollectionRequestMessage<ValueObserver>
    {
        public ArgumentObserver Argument { get; } = argument;

        public string? Filter { get; } = filter;
    }

    public static implicit operator Argument(ArgumentObserver observer) => observer.Model;
    public static implicit operator ArgumentObserver(Argument model) => new(model);
}