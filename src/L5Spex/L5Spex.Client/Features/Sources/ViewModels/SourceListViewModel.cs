using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MediatR;

namespace L5Spex.Client.Features.Sources.ViewModels;

public partial class SourceListViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private SourceWater _selectedSource;
    private readonly TaskNotifier _load;

    private Task Load
    {
        init => SetPropertyAndNotifyOnCompletion(ref _load, value);
    }

    public SourceListViewModel(IMediator mediator, IMessenger messenger)
    {
        _mediator = mediator;
        messenger.RegisterAll(this);
        //Load = LoadSources();
    }


    [ObservableProperty] private ObservableCollection<SourceWater> _sources;

    public SourceWater SelectedSource
    {
        get => _selectedSource;
        set
        {
            SetProperty(ref _selectedSource, value);
            OnSourceSelected(_selectedSource);
        }
    }

    private void OnSourceSelected(SourceWater value)
    {
        /*var request = new UpdateSelectedSource.Request(value);
        _mediator.Send(request);*/
        //Send the update to the mediator
    }

    /*[RelayCommand]
    private async Task AddSource()
    {
        var source = await _mediator.Send(new AddSource.Request());
    }
        

    [RelayCommand]
    private void LocateSource(SourceWater sourceWater) =>
        _mediator.Send(new LocateSource.Request(sourceWater.DirectoryName));

    private async Task LoadSources()
    {
        var request = new GetSources.Request();
        var response = await _mediator.Send(request);
        Sources = new ObservableCollection<SourceWater>(response.Select(r => new SourceWater(r)));
    }*/
}