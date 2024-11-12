using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Pages;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Input;
using L5Sharp.Core;

namespace AutoSpex.Client.Observers;

public class ElementObserver(LogixElement model) : Observer<LogixElement>(model)
{
    public override string Name => DetermineName();
    public string? Scope => Model is LogixScoped scoped ? scoped.Scope.Path : default;
    public Element Type => Element.FromName(Model.GetType().Name);
    public Task<PropertyTreePageModel> ProeprtyTree => Navigator.Navigate(() => new PropertyTreePageModel(this));

    #region Commands

    /// <inheritdoc />
    protected override async Task Copy()
    {
        try
        {
            var clipboard = Shell.Clipboard;
            if (clipboard is null) return;
            await clipboard.SetTextAsync(Model.Serialize().ToString());
        }
        catch (Exception e)
        {
            Notifier.ShowError("Command failed", e.Message);
        }
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerable<PropertyObserver> GetProperties()
    {
        return Type.This.Properties.Select(p => new PropertyObserver(p, this));
    }

    /// <summary>
    /// Filters this element observer using the provide filter text. This filter will check if this element has any
    /// property values that contain the provided text using an ordinal cas insensitive comparer.
    /// </summary>
    /// <param name="filter">The filter text to apply.</param>
    /// <returns><c>true</c> if this observer passes the filter, otherwise, <c>false</c></returns>
    public override bool Filter(string? filter)
    {
        FilterText = filter;
        return Name.Satisfies(filter) || Scope.Satisfies(filter);
    }

    public override string ToString() => DetermineName();

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateMenuItems()
    {
        yield return new MenuActionItem
        {
            Header = "Copy",
            Icon = Resource.Find("IconFilledCopy"),
            Command = CopyCommand,
            Gesture = new KeyGesture(Key.C, KeyModifiers.Control)
        };
    }

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateContextItems()
    {
        yield return new MenuActionItem
        {
            Header = "Copy",
            Icon = Resource.Find("IconFilledCopy"),
            Command = CopyCommand,
            DetermineVisibility = () => HasSingleSelection,
            Gesture = new KeyGesture(Key.C, KeyModifiers.Control)
        };
    }


    private string DetermineName()
    {
        return Model switch
        {
            Tag tag => tag.TagName,
            LogixCode code => $"{code.Scope.Type} {code.Scope.Name}",
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

    public static implicit operator LogixElement(ElementObserver observer) => observer.Model;
    public static implicit operator ElementObserver(LogixElement element) => new(element);
}