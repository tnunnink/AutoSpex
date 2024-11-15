using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Observers;

/// <summary>
/// A generic observer that wraps another object and adds some type and formatted UI friendly text for the value.
/// This observer also supportes inner/nested complex types like criterion or lists of objects.
/// </summary>
public partial class ValueObserver : Observer<object?>
{
    /// <summary>
    /// The function that retrieves the underlying value this observer wraps.
    /// </summary>
    private readonly Func<object?> _getter;

    /// <summary>
    /// A function that can parse or transform an input value to the value of the underlying observer.
    /// </summary>
    private readonly Func<object?, object?> _parser;

    /// <summary>
    /// The action that updates the underlying value that this observer wraps.
    /// This is optional. If not provided, we default to an action that does nothing (read only).
    /// </summary>
    private readonly Action<object?> _setter;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parser"></param>
    /// <param name="suggestions"></param>
    public ValueObserver(object? value,
        Func<object?, object?>? parser = default,
        Func<string?, CancellationToken, Task<IEnumerable<object>>>? suggestions = default) : base(value)
    {
        _getter = () => value;
        _parser = parser ?? (x => x);
        _setter = x => value = x;
        Suggestions = suggestions ?? ((_, _) => Task.FromResult(Enumerable.Empty<object>()));

        RefreshValue();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="getter"></param>
    /// <param name="setter"></param>
    /// <param name="parser"></param>
    /// <param name="suggestions"></param>
    public ValueObserver(Func<object?> getter,
        Func<object?, object?>? parser = default,
        Action<object?>? setter = default,
        Func<string?, CancellationToken, Task<IEnumerable<object>>>? suggestions = default) : base(getter())
    {
        _getter = getter ?? throw new ArgumentNullException(nameof(getter), "Value observer required a getter.");
        _parser = parser ?? (x => x);
        _setter = setter ?? (_ => { });
        Suggestions = suggestions ?? ((_, _) => Task.FromResult(Enumerable.Empty<object>()));

        RefreshValue();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Text))]
    [NotifyPropertyChangedFor(nameof(Type))]
    [NotifyPropertyChangedFor(nameof(Group))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private object? _value;

    /// <summary>
    /// The text display for the value.
    /// This should either be the literal vlaue or some texttual representation of a complex value.
    /// Foir this we are using a custom extension to return the text based on the object type.
    /// </summary>
    public string Text => Value.ToText();

    /// <summary>
    /// The user-friendly type name of the value.
    /// </summary>
    public string Type => Value?.GetType().DisplayName() ?? "unknown";

    /// <summary>
    /// The <see cref="TypeGroup"/> to which this value belongs. 
    /// </summary>
    public TypeGroup Group => TypeGroup.FromType(Value?.GetType());

    /// <summary>
    /// Indicates that the value is "empty" or has no value (null or empty text).
    /// </summary>
    public bool IsEmpty => Value is null || Value is string text && string.IsNullOrEmpty(text);

    /// <summary>
    /// The function that can retrieve potential values that fit the requirements of the object this observer wraps.
    /// This is optional. If not provided, we default it to return an empty collection.  
    /// </summary>
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> Suggestions { get; }

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        FilterText = filter;
        return Text.Satisfies(filter);
    }

    /// <summary>
    /// Updates the  based on the received value type from the entry field.
    /// If user enters simple text we would like to parse it as the strong type to let our data templates work.
    /// If user enters a value observer object, then it was selected from the suggestions, and we can just use that.
    /// Anything else just wrap in a value observer and set accordingly.
    /// </summary>
    [RelayCommand]
    private void UpdateValue(object? value)
    {
        var parsed = _parser(value);
        _setter.Invoke(parsed);
        RefreshValue();
    }

    /// <summary>
    /// Command to add an argument value to this arguments value collection. This is only supported for the In operation,
    /// since we need to allow the user to add an somwhat unbounded list of arguments.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddValue))]
    private void AddValue(object? value)
    {
        if (Value is not ObserverCollection<object?, ValueObserver> collection) return;

        switch (value)
        {
            case ValueObserver observer:
                collection.Add(new ValueObserver(observer.Value, _parser, Suggestions));
                break;
            case string text:
                collection.Add(new ValueObserver(_parser(text), _parser, Suggestions));
                break;
            default:
                collection.Add(new ValueObserver(value, _parser, Suggestions));
                break;
        }

        OnPropertyChanged(nameof(Text));
    }

    /// <summary>
    /// Determine if this value observer is a nested observer that would allow values to potentially be added.
    /// </summary>
    private bool CanAddValue() => Value is ObserverCollection<object?, ValueObserver>;

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

    /// <inheritdoc />
    public override string ToString() => Value?.ToString() ?? string.Empty;

    /// <summary>
    /// Refreshes the value of the observer by invoking the provided getter function to update the value.
    /// If the new value is trackable, it will be tracked; if the previous value was trackable, it will be forgotten.
    /// </summary>
    /// <remarks>
    /// The RefreshValue method should be called whenever the underlying value needs to be updated.
    /// </remarks>
    private void RefreshValue()
    {
        if (Value is ITrackable forgettable) Forget(forgettable);
        Value = _getter();
        if (Value is ITrackable trackable) Track(trackable);
        Track(nameof(Value));
    }
}