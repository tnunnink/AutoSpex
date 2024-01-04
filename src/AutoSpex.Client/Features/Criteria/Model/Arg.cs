using System;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Features.Criteria;

public partial class Arg : ObservableObject
{
    public Arg(Property property, object? value = default)
    {
        Property = property ?? throw new ArgumentNullException(nameof(property));
        Value = value;
    }
    
    [ObservableProperty] private Property _property;

    [ObservableProperty] private object? _value;
}