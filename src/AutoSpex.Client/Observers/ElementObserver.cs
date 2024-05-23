using System;
using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using L5Sharp.Core;

namespace AutoSpex.Client.Observers;

public partial class ElementObserver(LogixElement model) : Observer<LogixElement>(model)
{
    public Guid SourceId => DetermineSourceId();
    public string Name => DetermineName();
    public string? Description => DetermineDescription();
    public Element Element => Element.FromName(Model.GetType().Name);

    public IEnumerable<PropertyObserver> Properties =>
        Element.Properties.Where(p => p.Name != "This").Select(p => new PropertyObserver(p, this));

    public ICollection<PropertyObserver> DisplayProperties => GetDisplayProperties();
    public int PropertyCount => DisplayProperties.Count + 2;


    [RelayCommand]
    private void ViewElement() => Messenger.Send(new View(this));

    [RelayCommand]
    private async Task CopyElement()
    {
        var clipboard = Shell.Clipboard;
        if (clipboard is null) return;

        var data = new DataObject();
        data.Set(nameof(ElementObserver), this);
        await clipboard.SetDataObjectAsync(data);
    }

    [RelayCommand]
    private Task CreateVariable()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Filters this element observer using the provide filter text. This filter will check if this element has any
    /// property values that contain the provided text using an ordinal cas insensitive comparer.
    /// </summary>
    /// <param name="filter">The filter text to apply.</param>
    /// <returns><c>true</c> if this observer passes the filter, otherwise, <c>false</c></returns>
    public bool Filter(string? filter)
    {
        return string.IsNullOrEmpty(filter) ||
               Model.Serialize().ToString().Contains(filter, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString() => DetermineName();
    public static implicit operator LogixElement(ElementObserver observer) => observer.Model;
    public static implicit operator ElementObserver(LogixElement element) => new(element);


    private Guid DetermineSourceId()
    {
        var element = Model.Serialize();
        var id = element.Ancestors(L5XName.RSLogix5000Content).SingleOrDefault()?.Attribute(nameof(SourceId))?.Value;
        return id is not null ? Guid.Parse(id) : Guid.Empty;
    }

    private string DetermineName()
    {
        return Model switch
        {
            DataTypeMember member => member.Name,
            Parameter parameter => parameter.Name,
            Tag tag => tag.TagName,
            LogixCode code => code.Location,
            LogixComponent component => component.Name,
            _ => Model.ToString() ?? Model.L5XType
        };
    }

    private string? DetermineDescription()
    {
        return Model switch
        {
            DataTypeMember member => member.Description,
            Parameter parameter => parameter.Description,
            Rung rung => rung.Comment,
            LogixComponent component => component.Description,
            _ => null
        };
    }

    private ICollection<PropertyObserver> GetDisplayProperties()
    {
        var properties = new List<PropertyObserver>();

        foreach (var name in Element.DisplayProperties)
        {
            var property = Properties.FirstOrDefault(p => p.Model.Path == name);
            if (property is null) continue;
            properties.Add(property);
        }

        return properties;
    }

    public record View(ElementObserver Element);
}