using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using L5Sharp.Core;

namespace AutoSpex.Client.Observers;

public abstract partial class InputObserver<T>(
    Func<T> getter,
    Action<T>? setter = default,
    Func<Type>? input = default) : Observer
{
    /// <summary>
    /// The function that retrieves the underlying value this input observer wraps.
    /// </summary>
    protected readonly Func<T> Getter = getter ?? throw new ArgumentNullException(nameof(getter));

    /// <summary>
    /// The action that updates the underlying value that this input observer wraps.
    /// This is optional. If not provided, we default to an action that does nothing (read only).
    /// </summary>
    protected readonly Action<T> Setter = setter ?? (_ => { });

    /// <summary>
    /// The function the gets the input type the underlying value expects.
    /// </summary>
    protected readonly Func<Type> Input = input ?? (() => typeof(object));


    /// <summary>
    /// A function that takes some input text and returns a set of suggesstable values that can match the
    /// current context of this input observer.
    /// </summary>
    public abstract Func<string?, CancellationToken, Task<IEnumerable<object>>> Suggestions { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    [RelayCommand]
    protected void Update(object? value)
    {
        if (value is T typed)
        {
            Setter.Invoke(typed);
        }

        Refresh();
    }
}

public partial class PropertyInput : Observer
{
    /// <summary>
    /// The function the gets the input or origin property to this property input.
    /// </summary>
    private readonly Func<Property> _input;

    /// <summary>
    /// The function that retrieves the underlying value this observer wraps.
    /// </summary>
    private readonly Func<string> _getter;

    /// <summary>
    /// The action that updates the underlying value that this observer wraps.
    /// This is optional. If not provided, we default to an action that does nothing (read only).
    /// </summary>
    private readonly Action<string> _setter;

    /// <summary>
    /// Instantiates a new <see cref="ArgumentInput"/> observer with the provided functions for getting and setting the
    /// object value this argument wraps.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="getter">The funtion that retrieves the underlying argument value.</param>
    /// <param name="setter">The function that sets teh underlying argument value. If not provided we default to nothing (read-only).</param>
    public PropertyInput(Func<string> getter, Action<string>? setter = default, Func<Property>? input = default)
    {
        _getter = getter ?? throw new ArgumentNullException(nameof(getter));
        _setter = setter ?? (_ => { });
        _input = input ?? (() => Property.Default);

        Track(nameof(Value));
    }

    /// <summary>
    /// The <see cref="Engine.Property"/> instance that this criterion is configured with. This will use the current
    /// input type and configured property string to redner the strongly typed property.
    /// </summary>
    public Property Value => GetProperty();

    /// <summary>
    /// The string path of the underlying property value.
    /// </summary>
    public string Path => Value.Path;

    /// <summary>
    /// Gets a value indicating whether the PropertyInput is empty.
    /// Returns true if the Value property is equal to the default Property instance.
    /// </summary>
    public bool IsEmpty => Value == Property.Default;

    /// <summary>
    /// A function that takes some input filter and returns a set of suggesstable property values that can match the
    /// current context of this input observer.
    /// </summary>
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> Suggestions => GetProperties;

    /// <summary>
    /// Command to update the underlying property path for this observer instance given the input object.
    /// This input can be text that the user types or a selected property from the suggestion popup.
    /// </summary>
    [RelayCommand]
    private void Update(object? value)
    {
        var path = value switch
        {
            Property property => property.Path,
            string text => text,
            TagName tagName => tagName.ToString(),
            _ => string.Empty
        };

        _setter(path);
        OnPropertyChanged(nameof(Value));
        OnPropertyChanged(nameof(Path));
        OnPropertyChanged(nameof(IsEmpty));
    }

    /// <summary>
    /// Retrieve the property by invoking the provided getter and using the known input property.
    /// </summary>
    /// <returns>The property based on the observer type.</returns>
    private Property GetProperty()
    {
        var path = _getter();
        return !string.IsNullOrEmpty(path) ? _input().GetProperty(path) : Property.Default;
    }

    /// <summary>
    /// Retrieves possible properties based on the origin and entered property path for this observer.
    /// For the most part we can return these data statically using the built-in functions of <see cref="Property"/> via reflection.
    /// One bonus feature is tags, where we want to also suggest potential tag name that would make sense in the current context.
    /// This will be handled via async request message.
    /// </summary>
    private async Task<IEnumerable<object>> GetProperties(string? filter, CancellationToken token)
    {
        if (token.IsCancellationRequested) return [];

        var input = _input.Invoke();

        //If no input text is entered, just return the properties of the origin property.
        if (string.IsNullOrEmpty(filter)) return input.Properties;

        //Based on the input text, parse the path and the current member. Use this to find the current property.
        var index = GetIndex(filter);
        var path = index > 0 ? filter[..index] : string.Empty;
        var member = index > 0 ? filter[(index + 1)..] : filter;
        var current = input.GetProperty(path);

        //While we are inside an tag property indexer, we want to suggest possible tag names from a source.
        if (current.Type == typeof(Tag) && member.Count(x => x == '[') != member.Count(x => x == ']'))
        {
            return await GetTagNames(member.TrimStart('['), token);
        }

        //Otherwise return the static properties that match the current member text.
        return current.Properties
            .Where(p => p.Name.Satisfies(member))
            .OrderByDescending(p => p.Name.StartsWith(member))
            .ThenBy(p => p.Name);

        int GetIndex(string text)
        {
            var result = 0;
            var isOpen = false;

            for (var i = 0; i < text.Length; i++)
            {
                var characther = text[i];

                isOpen = characther switch
                {
                    '[' => true,
                    ']' => false,
                    _ => isOpen
                };

                if (Property.Separators.Contains(characther) && !isOpen) result = i;
            }

            return result;
        }
    }

    /// <summary>
    /// Retrieves a list of tag names based on the input filter string and the observer instance.
    /// </summary>
    /// <param name="member">The filter string to search for in the tag names.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The filtered and sorted list of tag names as TagName objects.</returns>
    private async Task<IEnumerable<TagName>> GetTagNames(string member, CancellationToken token)
    {
        if (token.IsCancellationRequested) return [];

        var tagNames = new List<TagName>();

        //Sends request to essentially run the query or spec up to the point of this observer to get context data.
        var data = await Messenger.Send(new GetDataTo(this)).GetResponsesAsync(token);

        foreach (var item in data)
        {
            switch (item)
            {
                case Tag tag:
                    tagNames.AddRange(tag.TagNames());
                    break;
                case Module module:
                    tagNames.AddRange(module.Tags.SelectMany(t => t.TagNames()));
                    break;
            }
        }

        return tagNames
            .Select(t => t.Path)
            .Where(t => !string.IsNullOrEmpty(t) && t.Satisfies(member))
            .Distinct()
            .OrderByDescending(t => t.StartsWith(member))
            .ThenBy(t => t)
            .Select(t => new TagName($"[{t}]"));
    }

    /// <summary>
    /// A message that is sent from this wrapper observer to other observers to get a list of potential tag names that
    /// can be used for tag indexer property entries.
    /// </summary>
    public class GetDataTo(Observer observer) : AsyncCollectionRequestMessage<object?>
    {
        public Observer Observer { get; } = observer;
    }
}