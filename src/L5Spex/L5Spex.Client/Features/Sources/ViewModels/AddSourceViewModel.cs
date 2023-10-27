using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;

namespace L5Spex.Features.Sources.ViewModels;

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