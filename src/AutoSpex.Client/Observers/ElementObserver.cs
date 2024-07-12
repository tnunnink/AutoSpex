using System;
using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using L5Sharp.Core;

namespace AutoSpex.Client.Observers;

public partial class ElementObserver(LogixElement model) : Observer<LogixElement>(model)
{
    public Guid SourceId => DetermineSourceId();
    public override string Name => DetermineName();
    public string? Description => DetermineDescription();
    public string? Container => DetermineContainer();
    public Element Element => Element.FromName(Model.GetType().Name);
    public IEnumerable<PropertyObserver> Properties => GetProperties();


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
    public override bool Filter(string? filter)
    {
        return base.Filter(filter) || Container.Satisfies(filter);
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

    private string? DetermineContainer()
    {
        return Model is LogixObject element ? element.Container : default;
    }

    private IEnumerable<PropertyObserver> GetProperties()
    {
        return Element.This.Properties.Select(p => new PropertyObserver(p, this));
    }
}