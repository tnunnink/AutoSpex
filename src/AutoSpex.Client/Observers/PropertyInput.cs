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

public partial class PropertyInput : Observer
{
    private readonly Observer _observer;

    public PropertyInput(Observer observer)
    {
        _observer = observer;
        Track(nameof(Value));
    }

    /// <summary>
    /// The origin <see cref="Engine.Property"/> type to this property input.
    /// This is based what observer owns this object and where it exists withint a query or spec.
    /// </summary>
    private Property Origin => Messenger.Send(new GetInputTo(_observer)).Response;

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
    /// The actualy poperty return <see cref="System.Type"/>.
    /// </summary>
    public Type Type => Value.Type;

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
        var result = value switch
        {
            Property property => property.Path,
            string path => path,
            TagName tagName => tagName.ToString(),
            _ => string.Empty
        };

        SetProperty(result);
        OnPropertyChanged(nameof(Value));
    }

    /// <summary>
    /// Retrieve the property based on the observer type. If the observer is a CriterionObserver,
    /// the property comes from the criterion model. If the observer is a SelectObserver,
    /// the property comes from the select model. Otherwise, an empty string is returned.
    /// </summary>
    /// <returns>The property based on the observer type.</returns>
    private Property GetProperty()
    {
        var path = _observer switch
        {
            CriterionObserver criterion => criterion.Model.Property,
            SelectObserver select => select.Model.Property,
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(path)) return Property.Default;
        return Origin.GetProperty(path);
    }

    /// <summary>
    /// Sets the specified property value for the associated observer based on the input result.
    /// </summary>
    /// <param name="result">The value to set as the property.</param>
    private void SetProperty(string result)
    {
        switch (_observer)
        {
            case CriterionObserver criterion:
                criterion.Model.Property = result;
                break;
            case SelectObserver select:
                select.Model.Property = result;
                break;
        }
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

        //If no input text is entered, just return the properties of the origin property.
        if (string.IsNullOrEmpty(filter)) return Origin.Properties;

        //Based on the input text, parse the path and the current member. Use this to find the current property.
        var index = GetIndex(filter);
        var path = index > 0 ? filter[..index] : string.Empty;
        var member = index > 0 ? filter[(index + 1)..] : filter;
        var current = Origin.GetProperty(path);

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
        var data = await Messenger.Send(new GetDataTo(_observer)).Response;

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
    /// A message that is sent from this wrapper observer to other observsers to determine what the input or origin
    /// property is for this property input. This is handled b various observers, but should only ever return a single
    /// response.
    /// </summary>
    public class GetInputTo(Observer observer) : RequestMessage<Property>
    {
        public Observer Observer { get; } = observer;
    }

    /// <summary>
    /// A message that is sent from this wrapper observer to other observers to get a list of potential tag names that
    /// can be used for tag indexer property entries.
    /// </summary>
    public class GetDataTo(Observer observer) : AsyncRequestMessage<IEnumerable<object?>>
    {
        public Observer Observer { get; } = observer;
    }
}