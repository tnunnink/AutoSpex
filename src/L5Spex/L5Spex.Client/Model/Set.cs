using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace L5Spex.Model;

public partial class Set : ObservableValidator
{
    public Set()
    {
        Id = Guid.NewGuid();
        Specifications = new ObservableCollection<Specification>();
    }

    public Set(string name)
    {
        Name = name;
    }
    
    public Guid Id { get; private set; }
    
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    private string _name;

    [ObservableProperty]
    private ObservableCollection<Specification> _specifications;
}