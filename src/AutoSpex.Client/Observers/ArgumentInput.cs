using System;
using System.Collections;
using System.Collections.Generic;
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
using L5Sharp.Core;
using Range = AutoSpex.Engine.Range;

namespace AutoSpex.Client.Observers;

public partial class ArgumentInput : Observer
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
    private readonly Func<Property> _input;

    /// <summary>
    /// Instantiates a new <see cref="ArgumentInput"/> observer with the provided functions for getting and setting the
    /// object value this argument wraps.
    /// </summary>
    /// <param name="getter">The funtion that retrieves the underlying argument value.</param>
    /// <param name="setter">The function that sets teh underlying argument value. If not provided we default to nothing (read-only).</param>
    /// <param name="input">The function that returns the expected <see cref="Engine.Property"/> the argument should belong to.
    /// This is used to help parse text inputs to the correct strongly typed value.
    /// If not provided, text inputs will be parsed as the first parsable non-text type.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="getter"/> is null.</exception>
    public ArgumentInput(Func<object?> getter, Action<object?>? setter = default, Func<Property>? input = default)
    {
        _getter = getter ?? throw new ArgumentNullException(nameof(getter), "Value observer required a getter.");
        _setter = setter ?? (_ => { });
        _input = input ?? (() => Property.Default);

        Value = GetObservable();

        if (Value is ValueObserver)
        {
            Track(nameof(Value));
        }
        else
        {
            Track(Value);
        }
    }

    /// <summary>
    /// The wrapped trackable argument value. This could be several different observer instance, but needs to be trackable,
    /// so we can respond to changes to notifiy for saving.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValueText))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(Suggestions))]
    private ITrackable _value;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty] private string? _inputText;

    /// <summary>
    /// The text representation of the current argument value.
    /// </summary>
    public string ValueText => Value.ToText();

    /// <summary>
    /// Indicates that the value is "empty" or has no value (null or empty text/collection).
    /// </summary>
    public bool IsEmpty => Value is ValueObserver { IsEmpty: true } or ICollection { Count: 0 };

    /// <summary>
    /// The function that can retrieve potential values that fit the requirements of the object this observer wraps.
    /// </summary>
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> Suggestions => GetSuggestions;

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
        Value = GetObservable();
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
        OnPropertyChanged(nameof(ValueText));
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
        OnPropertyChanged(nameof(ValueText));
    }

    #endregion

    #region Messages

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
        var group = _input.Invoke().Group;

        return value switch
        {
            string text when Reference.IsValid(text) => Reference.Parse(text),
            string text when group.TryParse(text, out var parsed) => parsed,
            ValueObserver observer => observer.Model,
            _ => value
        };
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
            Criterion criterion => new CriterionObserver(criterion, () => Property.This(_input().InnerType)),
            _ => new ValueObserver(value)
        };
    }

    /// <summary>
    /// Sends the <see cref="SuggestionRequest"/> message to other observers in hope to recieve values that we can
    /// suggest based on the context of this value observer and the provided filter text input.
    /// </summary>
    private async Task<IEnumerable<object>> GetSuggestions(string? filter, CancellationToken token)
    {
        var suggestions = new List<object>();

        suggestions.AddRange(GetStaticSuggestions(filter, token));
        suggestions.AddRange(await GetSourceSuggestions(filter, token));
        suggestions.AddRange(GetSpecialReferences(filter, token));
        suggestions.AddRange(await GetSourceReferences(filter, token));

        return suggestions;
    }

    /// <summary>
    /// Returns a collection of static values that we can suggest based on the input property/type to this argument
    /// observer wrapper. This will be either static boolean or enumeration values which are enumerable (a known set of).
    /// </summary>
    private IEnumerable<object> GetStaticSuggestions(string? filter, CancellationToken token)
    {
        if (token.IsCancellationRequested) return [];
        if (filter is not null && filter.StartsWith(Reference.KeyStart)) return [];

        var suggestions = new List<ValueObserver>();
        var input = _input.Invoke();

        if (input.Group == TypeGroup.Boolean)
            suggestions.AddRange([new ValueObserver(false), new ValueObserver(true)]);

        if (input.Group == TypeGroup.Enum && typeof(LogixEnum).IsAssignableFrom(input.Type))
            suggestions.AddRange(LogixEnum.Options(input.Type).Select(o => new ValueObserver(o)));

        if (string.IsNullOrEmpty(filter)) return suggestions;

        return suggestions
            .Where(x => x.Filter(filter))
            .OrderByDescending(x => x.Text.StartsWith(filter))
            .ThenBy(x => x.Text);
    }

    /// <summary>
    /// Sends request to get in memory source values to suggest based on the input filter and this argument observer input.
    /// </summary>
    private async Task<IEnumerable<object>> GetSourceSuggestions(string? filter, CancellationToken token)
    {
        if (filter is not null && filter.StartsWith(Reference.KeyStart)) return [];

        //Avoid duplicating any static value we have already suggested.
        //This method should only return values for numbers, strings, dates, or other complex objects.
        var group = _input().Group;
        if (group == TypeGroup.Boolean || group == TypeGroup.Enum) return [];

        var message = Messenger.Send(new SuggestionRequest(this));
        var suggestions = await message.GetResponsesAsync(token);

        if (string.IsNullOrEmpty(filter)) return suggestions;

        return suggestions
            .Where(x => x.Filter(filter))
            .OrderByDescending(x => x.Text.StartsWith(filter))
            .ThenBy(x => x.Text);
    }

    /// <summary>
    /// Returns the static special reference suggestion values if the provided input text contains the correct
    /// starting reference text.
    /// </summary>
    private static IEnumerable<object> GetSpecialReferences(string? filter, CancellationToken token)
    {
        if (token.IsCancellationRequested) return [];
        if (string.IsNullOrEmpty(filter) || !filter.StartsWith(Reference.KeyStart)) return [];
        if (filter.Count(c => c == Reference.KeyStart) == filter.Count(c => c == Reference.KeyEnd)) return [];
        return [];

        /*return Reference.THis
            .Select(r => new ValueObserver(r))
            .Where(x => x.Filter(filter))
            .OrderByDescending(x => x.Text.StartsWith(filter))
            .ThenBy(x => x.Text);*/
    }

    /// <summary>
    /// Sends requests to get source scope references from the database based on the provided input text.
    /// This will return any source scope reference that matches/contains the input text.
    /// </summary>
    private async Task<IEnumerable<object>> GetSourceReferences(string? filter, CancellationToken token)
    {
        if (token.IsCancellationRequested) return [];
        if (string.IsNullOrEmpty(filter) || !filter.StartsWith(Reference.KeyStart)) return [];

        if (filter.Count(c => c == Reference.KeyStart) == filter.Count(c => c == Reference.KeyEnd))
            return GetPropertySuggestions(filter, token);

        var key = filter.TrimStart(Reference.KeyStart);

        try
        {
            /*var references = await Mediator.Send(new ListReferences(key), token);

            return references
                .Select(r => new ValueObserver(r))
                .Where(x => x.Filter(filter))
                .OrderByDescending(x => x.Text.StartsWith(filter))
                .ThenBy(x => x.Text);*/
            return [];
        }
        catch (Exception)
        {
            // Ignored because this is just optional.
            // It's only to suggest possible values based on a known source content.
            return [];
        }
    }

    /// <summary>
    /// Handles getting suggestible properties for a reference input argument based on the current text input.
    /// This will parse the input, determine the origin type from the reference, and then use that type to return
    /// a list of possible properties that we know statically and filter them to that of the current text.
    /// This will not handle nested tagnames like property input. We will get nested tag names from the reference scope/key.
    /// </summary>
    private IEnumerable<object> GetPropertySuggestions(string filter, CancellationToken token)
    {
        return [];
        /*if (token.IsCancellationRequested || !filter.Contains("}.")) return [];

        //Get/parse the base reference.
        var close = filter.IndexOf(Reference.KeyEnd) + 1;
        var key = filter[..close].TrimStart(Reference.KeyStart).TrimEnd(Reference.KeyEnd);
        var reference = new Reference(key);

        try
        {
            //Determine the origin type.
            //If not detectable we should return the default property.
            //$this is special case and points to the current argument input origin type.
            var origin = reference.IsSource
                ? Element.FromScope(reference.Scope).This
                : reference is { IsSpecial: true, Key: "$this" }
                    ? Property.This(_input().Origin)
                    : Property.Default;

            //Parse the property path and create suggestions based on the origin type.
            var remaining = filter[close..].TrimStart('.');
            var index = remaining.LastIndexOf('.');
            var path = index > 0 ? remaining[..index] : string.Empty;
            var member = index > 0 ? remaining[(index + 1)..] : remaining;
            var current = origin.GetProperty(path);

            return current.Properties
                .Where(p => p.Name.Satisfies(member))
                .OrderByDescending(p => p.Name.StartsWith(member))
                .ThenBy(p => p.Name);
        }
        catch (Exception)
        {
            // Ignored because this is just optional.
            // It's only to suggest possible values based on a known source content.
            return [];
        }*/
    }

    #endregion
}