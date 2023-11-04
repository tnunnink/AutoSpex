using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace L5Spex.Client.Observers;

public partial class SourceObserver : ObservableObject
{
    public SourceObserver(dynamic record)
    {
        SourceId = record.SourceId;
        Path = record.Path.ToString();
        
        {
            
        }
        //create source file object with path
    }
    
    public Guid SourceId { get; }
    
    [ObservableProperty] private string _path;

    [ObservableProperty] private string _name;
    
    [ObservableProperty] private bool _pinned;
}