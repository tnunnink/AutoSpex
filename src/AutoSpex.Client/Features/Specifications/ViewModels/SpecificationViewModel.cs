using System;
using System.Collections.ObjectModel;
using AutoSpex.Client.Features.Nodes;
using AutoSpex.Engine;
using AutoSpex.Engine.Operations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;

namespace AutoSpex.Client.Features.Specifications;

public partial class SpecificationViewModel : NodeDetailViewModel
{
    private readonly IMediator _mediator;
    
    public SpecificationViewModel(Node node) : base(node, new SpecificationView())
    {
        _mediator = App.Container.GetInstance<IMediator>();
    }

    protected override Task Save()
    {
        throw new NotImplementedException();
    }

    [ObservableProperty] private Element? _element;

    [ObservableProperty] private ObservableCollection<CriterionViewModel> _criterion = new();

    private async Task LoadSpec()
    {
        var result = await _mediator.Send(new LoadSpecRequest(this));
    }

    [RelayCommand]
    private void AddFilter(CriterionViewModel criterion)
    {
        throw new NotImplementedException();
    }
}