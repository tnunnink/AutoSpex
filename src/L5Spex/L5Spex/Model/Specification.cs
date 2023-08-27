using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace L5Spex.Model;

public partial class Specification : ObservableObject
{
    public Specification(string name, string type)
    {
        Id = Guid.NewGuid();
        _name = name;
        _type = type;
        _description = string.Empty;
    }

    public Guid Id { get; }

    [ObservableProperty]
    private string _type;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _description;
}