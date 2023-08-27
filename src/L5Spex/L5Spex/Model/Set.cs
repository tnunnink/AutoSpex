using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace L5Spex.Model;

public partial class Set : ObservableObject
{
    public Set(string name)
    {
        Id = 0;
        Name = name;
        Specifications = new ObservableCollection<Specification>();
    }
    
    public int Id { get; set; }
    
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private ObservableCollection<Specification> _specifications;
}