using System;
using AutoSpex.Client.Pages;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Observers;

public partial class SourceObserver(Source source) : Observer<Source>(source)
{
    public Guid SourceId => Model.SourceId;

    public string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v, true);
    }
    
    public string Description
    {
        get => Model.Description;
        set => SetProperty(Model.Description, value, Model, (s, v) => s.Description = v, true);
    }
    
    public bool IsSelected
    {
        get => Model.IsSelected;
        set => SetProperty(Model.IsSelected, value, Model, (s, v) => s.IsSelected = v, true);
    }

    /*private Task Open() => Navigator.Navigate<DetailsPageModel>(() => new SourcePageModel(this));*/
}