using CommunityToolkit.Mvvm.ComponentModel;
using MediatR;

namespace L5Spex.Client.Features.Sources.ViewModels;

public partial class AddSourceViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    public AddSourceViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    /*[RelayCommand]
    private Task AddSource() => _mediator.Send(new AddSource.Request());*/
}