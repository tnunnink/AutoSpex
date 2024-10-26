using System;
using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using L5Sharp.Core;

namespace AutoSpex.Client.Observers;

public partial class ElementObserver : Observer<LogixElement>
{
    /// <inheritdoc/>
    public ElementObserver(LogixElement model) : base(model)
    {
        Description = DetermineDescription();
        Container = DetermineContainer();
        Type = Element.FromName(Model.GetType().Name);
    }

    public override string Name => DetermineName();
    public string? Description { get; }
    public string? Container { get; }
    public Element Type { get; }
    public IEnumerable<PropertyObserver> Properties => GetProperties();

    #region Commands

    [RelayCommand]
    private async Task CopyElement()
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

    [RelayCommand]
    private async Task CopyName()
    {
        try
        {
            var clipboard = Shell.Clipboard;
            if (clipboard is null) return;
            await clipboard.SetTextAsync(Name);
        }
        catch (Exception e)
        {
            Notifier.ShowError("Command failed", e.Message);
        }
    }

    [RelayCommand]
    private void ViewProperties()
    {
        try
        {
            Messenger.Send(new ShowProperties(this));
        }
        catch (Exception e)
        {
            Notifier.ShowError("Command failed", e.Message);
        }
    }

    #endregion

    #region Messages

    /// <summary>
    /// A message that can be sent to a parent page to signal the opening of the properties for this element.
    /// </summary>
    /// <param name="Element">The element instance for which to view properties.</param>
    public record ShowProperties(ElementObserver Element);

    #endregion

    /// <summary>
    /// Filters this element observer using the provide filter text. This filter will check if this element has any
    /// property values that contain the provided text using an ordinal cas insensitive comparer.
    /// </summary>
    /// <param name="filter">The filter text to apply.</param>
    /// <returns><c>true</c> if this observer passes the filter, otherwise, <c>false</c></returns>
    public override bool Filter(string? filter)
    {
        FilterText = filter;
        return Name.Satisfies(filter) || Container.Satisfies(filter) || Description.Satisfies(filter);
    }

    public override string ToString() => DetermineName();

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateMenuItems()
    {
        yield return new MenuActionItem
        {
            Header = "Copy Element",
            Icon = Resource.Find("IconFilledClone"),
            Command = CopyElementCommand
        };

        yield return new MenuActionItem
        {
            Header = "Copy Name",
            Icon = Resource.Find("IconFilledClone"),
            Command = CopyNameCommand
        };

        yield return new MenuActionItem
        {
            Header = "View Properties",
            Icon = Resource.Find("IconFilledBinoculars"),
            Command = ViewPropertiesCommand
        };
    }

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateContextItems()
    {
        yield return new MenuActionItem
        {
            Header = "Copy Element",
            Icon = Resource.Find("IconFilledClone"),
            Command = CopyElementCommand,
            DetermineVisibility = () => HasSingleSelection
        };

        yield return new MenuActionItem
        {
            Header = "Copy Name",
            Icon = Resource.Find("IconFilledClone"),
            Command = CopyNameCommand,
            DetermineVisibility = () => HasSingleSelection
        };

        yield return new MenuActionItem
        {
            Header = "View Properties",
            Icon = Resource.Find("IconFilledBinoculars"),
            Command = ViewPropertiesCommand,
            DetermineVisibility = () => HasSingleSelection
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

    private string? DetermineContainer()
    {
        return Model is LogixScoped scoped
            ? $"{scoped.Scope.Controller}/{scoped.Scope.Program}/{scoped.Scope.Routine}".Trim('/')
            : default;
    }

    private IEnumerable<PropertyObserver> GetProperties()
    {
        return Type.This.Properties.Select(p => new PropertyObserver(p, this));
    }

    public static implicit operator LogixElement(ElementObserver observer) => observer.Model;
    public static implicit operator ElementObserver(LogixElement element) => new(element);
}