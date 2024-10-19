using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Pages;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;
using L5Sharp.Core;

namespace AutoSpex.Client.Observers;

public partial class PropertyObserver(Property model, ElementObserver element) : Observer<Property>(model)
{
    private const int MaxFilterDepth = 5;

    /// <summary>
    /// The root <see cref="ElementObserver"/> object representing the staring point or origin type. As we navigate
    /// down the type hierarchy, this will get passed down, so we can get the value of the current property using the
    /// element's <see cref="LogixElement"/> instance. When we get to a collection property, these reset to the inner
    /// collection element type, so we can continue to navigate down the type hierarchy.
    /// </summary>
    private readonly ElementObserver _element = element ?? throw new ArgumentNullException(nameof(element));

    /// <summary>
    /// The property name.
    /// </summary>
    public override string Name => Model.Name;

    /// <summary>
    /// The UI friendly type name of the current property.
    /// </summary>
    public string Type => $"{{{Model.DisplayName}}}";

    /// <summary>
    /// The value of the property retrieved from the instance help within <see cref="_element"/>. 
    /// </summary>
    public ValueObserver Value => new(Model.GetValue(_element.Model));

    /// <summary>
    /// The collection of child <see cref="PropertyObserver"/> that are navigable from this property instance given the
    /// current root <see cref="ElementObserver"/>.
    /// </summary>
    public IEnumerable<PropertyObserver> Properties => GetProperties();

    [RelayCommand]
    private async Task CopyValue()
    {
        try
        {
            var clipboard = Shell.Clipboard;
            if (clipboard is null) return;
            await clipboard.SetTextAsync(Value.ToString());
        }
        catch (Exception e)
        {
            Notifier.ShowError("Command failed", e.Message);
        }
    }

    [RelayCommand]
    private async Task CreateVariable()
    {
        try
        {
            var variable = await Prompter.Show<VariableObserver?>(() => new NewVariablePageModel(Value.Model));
            if (variable is null) return;
            Notifier.ShowSuccess("Variable created",
                $"{variable.Name} successfully created for in {variable.Node?.Name}");
        }
        catch (Exception e)
        {
            Notifier.ShowError("Command failed", e.Message);
        }
    }

    [RelayCommand]
    private void ViewValue()
    {
        try
        {
        }
        catch (Exception e)
        {
            Notifier.ShowError("Command failed", e.Message);
        }
    }

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        if (Model.TypeGraph.Length > MaxFilterDepth) return false;

        var passes = base.Filter(filter) || Type.Satisfies(filter) || Value.Filter(filter);
        var children = Properties.Count(x => x.Filter(filter));

        IsVisible = passes || children > 0;
        IsExpanded = !string.IsNullOrEmpty(filter) && children > 0;

        return IsVisible;
    }

    /// <summary>
    /// Get child <see cref="PropertyObserver"/> objects based on what this property represents
    /// (collection, child element, simple child property). 
    /// </summary>
    private IEnumerable<PropertyObserver> GetProperties()
    {
        //For collections, we need to create pseudo properties since they can not be retrieved statically from the type
        //information. We need access to the actual instance object in order to get the items of the collection, but will
        //want to present those as "properties" in the UI from which the user can continue to drill down from.
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (Model.Group == TypeGroup.Collection)
        {
            return GetCollectionProperties();
        }

        //If this is a normal property then just return the sub properties wrapped in the observer and pass
        //down the root Element.
        return Model.Properties
            .Where(p => p.Type != _element.Type.Type &&
                        p.Name != "This") //No self referencing types please. Just confusing to look at.
            .Select(p => new PropertyObserver(p, _element));
    }

    /// <summary>
    /// Creates a collection of child pseudo properties for the current property collection type.
    /// </summary>
    private List<PropertyObserver> GetCollectionProperties()
    {
        //Get the collection instance and if not an IEnumerable return empty properties.
        var value = Model.GetValue(_element.Model);
        if (value is not IEnumerable enumerable) return [];

        var properties = new List<PropertyObserver>();

        //For each child create a new pseudo collection item property.
        var index = 0;
        foreach (var item in enumerable)
        {
            var name = $"[{index}]";
            var property = new Property(name, item.GetType(), Model);
            var observer = new PropertyObserver(property, _element);
            properties.Add(observer);
            index++;
        }

        return properties;
    }
}