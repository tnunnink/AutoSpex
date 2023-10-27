﻿using CommunityToolkit.Mvvm.ComponentModel;
using MediatR;

namespace L5Spex.ViewModels;

public partial class HeaderViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    public HeaderViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }
}