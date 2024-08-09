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
    public override string Name => DetermineName();
    public string? Description => DetermineDescription();
    public string? Container => DetermineContainer();
    public Element Element => Element.FromName(Model.GetType().Name);
    public IEnumerable<PropertyObserver> Properties => GetProperties();

    #region Commands

    /// <inheritdoc />
    /// <remarks>
    /// </remarks>
    protected override Task Navigate()
    {
        Messenger.Send(new Open(this));
        return Task.CompletedTask;
    }

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
    private async Task CopyName()
    {
        var clipboard = Shell.Clipboard;
        if (clipboard is null) return;

        var data = new DataObject();
        data.Set(nameof(Name), Name);

        await clipboard.SetDataObjectAsync(data);
    }

    [RelayCommand]
    private Task CreateVariable()
    {
        throw new NotImplementedException();
    }

    #endregion

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

    public static implicit operator LogixElement(ElementObserver observer) => observer.Model;
    public static implicit operator ElementObserver(LogixElement element) => new(element);

    /// <summary>
    /// A message to be sent to a containing page to allow this element observer to be opened.
    /// Opening an element just involves viewing its property tree to see the data from the interface.
    /// </summary>
    /// <param name="Element">The element to open</param>
    public record Open(ElementObserver Element);
}