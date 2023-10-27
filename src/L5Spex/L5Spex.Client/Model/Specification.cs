using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace L5Spex.Model;

public partial class Specification : ObservableObject
{
    public Specification(Set set, string name, SpecType type)
    {
        Id = Guid.NewGuid();
        Name = name;
        Type = type;
        Description = string.Empty;
        Created = DateTime.Now;
        Modified = DateTime.Now;
    }

    public Guid Id { get; }
    public Guid SetId { get; }
    
    public virtual Set Set { get; }

    [ObservableProperty] private SpecType _type;

    [ObservableProperty] private string _name;

    [ObservableProperty] private string _description;

    [ObservableProperty] private DateTime _created;

    [ObservableProperty] private DateTime _modified;

    [ObservableProperty] private SpecOptions _options;

    [ObservableProperty] private ObservableCollection<Specification> _dependencies;
}