﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using L5Sharp.Core;

namespace AutoSpex.Client.Observers;

public partial class PropertyObserver(Property model, ElementObserver element) : Observer<Property>(model)
{
    /// <summary>
    /// The root <see cref="ElementObserver"/> object representing the staring point or origin type. As we navigate
    /// down the type hierarchy, this will get passed down so we can get the value of the current property using the
    /// element's <see cref="LogixElement"/> instance. When we get to a collection property, these reset to the inner
    /// collection element type so we can continue to navigate down the type hierarchy.
    /// </summary>
    private readonly ElementObserver _element = element ?? throw new ArgumentNullException(nameof(element));

    /// <summary>
    /// The property name.
    /// </summary>
    public string Name => Model.Name;

    /// <summary>
    /// The UI friendly type name of the current property.
    /// </summary>
    public string Type => $"{{{Model.Identifier}}}";

    /// <summary>
    /// The <see cref="TypeGroup"/> the property value belongs to.
    /// </summary>
    public TypeGroup Group => Model.Group;

    /// <summary>
    /// The value of the property retrieved from the instance help within <see cref="_element"/>. 
    /// </summary>
    public object? Value => Model.Getter()(_element.Model);

    /// <summary>
    /// The value of the property retrieved from the instance help within <see cref="_element"/>. 
    /// </summary>
    public string FormattedValue => FormatValue();

    private string FormatValue()
    {
        var value = Model.Getter()(_element.Model);
        if (value is null) return "null";
        if (Model.Group == TypeGroup.Text) return $"\"{value}\"";
        if (Model.Group == TypeGroup.Collection && value is IEnumerable enumerable)
            return $"Count =";
        if (Model.Group == TypeGroup.Enum && value is LogixEnum logixEnum) return $"{logixEnum.Name}";
        return value.ToString() ?? "null";
    }

    /// <summary>
    /// indicates that the observer is visible in the control in which it is being presented.
    /// </summary>
    [ObservableProperty] private bool _isVisible = true;

    /// <summary>
    /// Indicates that the observer is expanded within the tree view
    /// </summary>
    [ObservableProperty] private bool _isExpanded;

    [ObservableProperty] private bool _isSelected;

    /// <summary>
    /// The collection of child <see cref="PropertyObserver"/> that are navigable from this property instance given the
    /// current root <see cref="ElementObserver"/>.
    /// </summary>
    public IEnumerable<PropertyObserver> Properties => GetProperties();

    /// <summary>
    /// Get child <see cref="PropertyObserver"/> objects based on what this property represents
    /// (collection, child element, simple child property). 
    /// </summary>
    private IEnumerable<PropertyObserver> GetProperties()
    {
        //For collections we need to create pseudo properties since they can not be retrieved statically from the type
        //information. We need access to the actual instance object in order to get the items of the collection, but will
        //want to present those as "properties" in the UI from which the user can continue to drill down from.
        if (Model.Group == TypeGroup.Collection)
        {
            return GetCollectionProperties();
        }

        //If we "started over" in a collection so this property represents a new nested LogixElement type, forward this
        //call to the element instance's Properties which will allow us to get nested properties correctly and use the
        //corresponding getter expressions to retrieve the values.
        if (Model.Origin == Model.Type)
        {
            return _element.Properties;
        }

        //If this is a normal property then just return the sub properties wrapped in the observer and pass
        //down the root Element.
        return Model.Properties.Select(p => new PropertyObserver(p, _element));
    }

    /// <summary>
    /// Creates a collection of child pseudo properties for the current property collection type.
    /// </summary>
    private IEnumerable<PropertyObserver> GetCollectionProperties()
    {
        //Get the collection instance and if not an IEnumerable return empty properties.
        var value = Model.Getter()(_element.Model);
        if (value is not IEnumerable enumerable) return Enumerable.Empty<PropertyObserver>();

        var properties = new List<PropertyObserver>();

        //For each child create a new pseudo property.
        var index = 0;
        foreach (var item in enumerable)
        {
            //The path is the current property path plus the array index.
            var path = $"{Model.Path}[{index}]";
            //If the collection item is an inner child element, pass that in as the new element for the property
            //so from the child pseudo property we can continue to navigate down the tree.
            var element = item is LogixElement child ? new ElementObserver(child) : _element;
            //Pass in the custom getter which is just pointer to the item of the collection.
            var property = new Property(element.Element.Type, path, item.GetType(), _ => item);
            var observer = new PropertyObserver(property, element);

            properties.Add(observer);
            index++;
        }

        return properties;
    }
}