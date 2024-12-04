using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence.References;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using L5Sharp.Core;
using Range = AutoSpex.Engine.Range;

namespace AutoSpex.Client.Observers;

public partial class ArgumentInput : Observer, IRecipient<ArgumentInput.SuggestionRequest>
{
    /// <summary>
    /// The function that retrieves the underlying value this observer wraps.
    /// </summary>
    private readonly Func<object?> _getter;

    /// <summary>
    /// The action that updates the underlying value that this observer wraps.
    /// This is optional. If not provided, we default to an action that does nothing (read only).
    /// </summary>
    private readonly Action<object?> _setter;

    /// <summary>
    /// A function that returns the expected input type for this argument value.
    /// </summary>
    private readonly Func<Type> _type;

    /// <summary>
    /// Instantiates a new <see cref="ArgumentInput"/> observer with the provided functions for getting and setting the
    /// object value this argument wraps.
    /// </summary>
    /// <param name="getter">The funtion that retrieves the underlying argument value.</param>
    /// <param name="setter">The function that sets teh underlying argument value. If not provided we default to nothing (read-only).</param>
    /// <param name="type">The function that returns the expected <see cref="TypeGroup"/> the argument should belong to.
    /// This is used to help parse text inputs to the correct strongly typed value.
    /// If not provided, text inputs will be parsed as the first parsable non-text type.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="getter"/> is null.</exception>
    public ArgumentInput(Func<object?> getter, Action<object?>? setter = default, Func<Type>? type = default)
    {
        _getter = getter ?? throw new ArgumentNullException(nameof(getter), "Value observer required a getter.");
        _setter = setter ?? (_ => { });
        _type = type ?? (() => typeof(object));

        Value = GetObservable();
        Track(Value);
        Track(nameof(Value));
    }

    /// <summary>
    /// The wrapped trackable argument value. This could be several different observer instance, but needs to be trackable,
    /// so we can respond to changes to notifiy for saving.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Text))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(Suggestions))]
    private ITrackable _value;

    /// <summary>
    /// The text representation of the current argument value.
    /// </summary>
    public string Text => Value.ToText();

    /// <summary>
    /// Indicates that the value is "empty" or has no value (null or empty text/collection).
    /// </summary>
    public bool IsEmpty => Value is ValueObserver { IsEmpty: true } or ICollection { Count: 0 };

    /// <summary>
    /// The expected argument input property.
    /// This is the type/property we should resolve inputs to and use to determine which types of values to suggest.
    /// This will/should ultimately match that of the parent criterion instance. 
    /// </summary>
    public Property Input => Property.This(_type.Invoke());

    /// <summary>
    /// The function that can retrieve potential values that fit the requirements of the object this observer wraps.
    /// </summary>
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> Suggestions => GetSuggestions;

    /// <inheritdoc />
    /// <remarks>
    /// We will allow external calls to refresh the value of this argument wrapper in case it's
    /// underlying model is updated externally.
    /// </remarks>
    public override void Refresh()
    {
        RefreshValue();
        base.Refresh();
    }

    #region Commands

    /// <summary>
    /// Updates the value based on the received value type from the entry field.
    /// If user enters simple text we would like to parse it as the strong type to let our data templates work.
    /// If user enters a value observer object, then it was selected from the suggestions, and we can just use that.
    /// Anything else just wrap in a value observer and set accordingly.
    /// </summary>
    [RelayCommand]
    private void UpdateValue(object? value)
    {
        var parsed = ParseInput(value);
        _setter.Invoke(parsed);
        RefreshValue();
    }

    /// <summary>
    /// Command to add an argument value to this arguments value collection. This is only supported for the In operation,
    /// since we need to allow the user to add an somwhat unbounded list of arguments.
    /// </summary>
    [RelayCommand]
    private void AddValue(object? value)
    {
        if (Value is not ObserverCollection<object?, ValueObserver> collection) return;
        var parsed = ParseInput(value);
        collection.Add(new ValueObserver(parsed));
        OnPropertyChanged(nameof(Text));
    }

    /// <summary>
    /// Command to remove a provided value from the wrapped observer collection.
    /// This is only intended to be suppored for values that wrap an ObserverCollection  
    /// </summary>
    [RelayCommand]
    private void RemoveValue(ValueObserver? value)
    {
        if (Value is not ObserverCollection<object?, ValueObserver> collection || value is null) return;
        collection.Remove(value);
        OnPropertyChanged(nameof(Text));
    }

    #endregion

    #region Messages

    /// <summary>
    /// Respond to suggestion requests from this observer by returning any static option value based on the expected
    /// argument input type (<see cref="_type"/>).
    /// </summary>
    public void Receive(SuggestionRequest message)
    {
        if (!message.Argument.Is(this)) return;

        var type = message.Argument._type.Invoke();
        var group = TypeGroup.FromType(type);

        if (group == TypeGroup.Enum && typeof(LogixEnum).IsAssignableFrom(type))
        {
            var options = LogixEnum.Options(type).Select(o => new ValueObserver(o)).ToList();
            options.ForEach(message.Reply);
        }

        if (group != TypeGroup.Boolean) return;
        message.Reply(new ValueObserver(false));
        message.Reply(new ValueObserver(true));
    }

    /// <summary>
    /// A request message that should return suggestable values to present to the user for this argument input type.
    /// This will be either static, source, or available reference values. These may come from an in-memory source,
    /// or may be a database requrest, but will be hadled in the observer which is most appropriate.
    /// </summary>
    /// <param name="argument">The instance that is requesting suggestions. This should help give parent observers
    /// the context it needs to determine what values to return.</param>
    public class SuggestionRequest(ArgumentInput argument) : AsyncCollectionRequestMessage<ValueObserver>
    {
        public ArgumentInput Argument { get; } = argument;
    }

    #endregion

    #region Internal

    /// <summary>
    /// Parses the input value to the appropriate type.
    /// If user enters text we would like to parse it as the strong type to let our data templates work.
    /// This will first try to use the configured group and if not provided will use the generic TypeGroup.Parse.
    /// If user enters a value observer object, then it was selected from the suggestions, and we can just use the wrapped value.
    /// Anything else just return the provided value.
    /// </summary>
    private object? ParseInput(object? value)
    {
        var group = TypeGroup.FromType(_type.Invoke());

        return value switch
        {
            string text when Reference.IsValid(text) => Reference.Parse(text),
            string text when group.TryParse(text, out var parsed) => parsed,
            ValueObserver observer => observer.Model,
            _ => value
        };
    }

    /// <summary>
    /// Whenever we reset the current <see cref="Value"/> instance we want to first forget and dispose of the old
    /// observer instance. Then start tracking the new instance. If it is an inner complex observer then changes
    /// can be made to child properties, and we need to respond to that.
    /// </summary>
    private void RefreshValue()
    {
        Forget(Value);
        Value.Dispose();
        Value = GetObservable();
        Track(Value);
    }

    /// <summary>
    /// Gets the underlying argument value and wraps it in a corresponding observer instance based on the set type.
    /// Argument can contain simple primitive values or inner complex objects like Range, Criterion, and Lists.
    /// </summary>
    private ITrackable GetObservable()
    {
        var value = _getter();

        return value switch
        {
            Range range => new RangeObserver(range),
            List<object?> list => list.ToObserver(x => new ValueObserver(x)),
            Criterion criterion => new CriterionObserver(criterion),
            _ => new ValueObserver(value)
        };
    }

    /// <summary>
    /// Sends the <see cref="SuggestionRequest"/> message to other observers in hope to recieve values that we can
    /// suggest based on the context of this value observer and the provided filter text input.
    /// </summary>
    private async Task<IEnumerable<object>> GetSuggestions(string? filter, CancellationToken token)
    {
        var suggestions = new List<ValueObserver>();

        suggestions.AddRange(await GetValueSuggestions(filter, token));
        suggestions.AddRange(GetSpecialReferences(filter, token));
        suggestions.AddRange(await GetSourceReferences(filter, token));

        return suggestions
            .Where(x => x.Filter(filter))
            .OrderByDescending(x => x.Text.StartsWith(filter ?? string.Empty))
            .ThenBy(x => x.Text);
    }

    /// <summary>
    /// Sends request to get in memory or static values to suggest based on the input filter and this argument observer input.
    /// </summary>
    private async Task<IEnumerable<ValueObserver>> GetValueSuggestions(string? filter, CancellationToken token)
    {
        if (filter is not null && filter.StartsWith(Reference.KeyStart)) return [];
        var message = Messenger.Send(new SuggestionRequest(this));
        return await message.GetResponsesAsync(token);
    }

    /// <summary>
    /// Returns the static special reference suggestion values if the provided input text contains the correct
    /// starting reference text.
    /// </summary>
    private static IEnumerable<ValueObserver> GetSpecialReferences(string? filter, CancellationToken token)
    {
        if (token.IsCancellationRequested) return [];
        if (string.IsNullOrEmpty(filter) || !filter.StartsWith(Reference.KeyStart)) return [];
        if (filter.Count(c => c == Reference.KeyStart) == filter.Count(c => c == Reference.KeyEnd)) return [];
        return Reference.Special.Select(r => new ValueObserver(r));
    }

    /// <summary>
    /// Sends requests to get source scope references from the database based on the provided input text.
    /// This will return any source scope reference that matches/contains the input text.
    /// </summary>
    private async Task<IEnumerable<ValueObserver>> GetSourceReferences(string? filter, CancellationToken token)
    {
        if (token.IsCancellationRequested) return [];
        if (string.IsNullOrEmpty(filter) || !filter.StartsWith(Reference.KeyStart)) return [];
        if (filter.Count(c => c == Reference.KeyStart) == filter.Count(c => c == Reference.KeyEnd)) return [];
        
        var key = filter.TrimStart(Reference.KeyStart);
        var references = await Mediator.Send(new ListSourceReferences(key), token);
        return references.Select(r => new ValueObserver(r));
    }
    
    

    #endregion
}