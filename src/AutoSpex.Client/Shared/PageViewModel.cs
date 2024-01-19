using System.ComponentModel;
using AutoSpex.Client.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;

namespace AutoSpex.Client.Shared;

[ObservableRecipient]
public abstract partial class PageViewModel : ObservableValidator, IChangeTracking
{
    //Instead of DI I'm resolving dependencies for the pages directly.
    //I am choosing to do this to simplify the construction and since I am never going to do mocking,
    //but rather using the real implementations to preform integration testing for my application pages.
    protected readonly Shell Shell = Container.Resolve<Shell>();
    protected readonly IMediator Mediator = Container.Resolve<IMediator>();
    protected readonly Navigator Navigator = Container.Resolve<Navigator>();

    public virtual string Route => GetType().Name;

    public virtual string Title => "Title";

    public virtual string Icon => "None";

    [ObservableProperty] private bool _loading;

    public virtual bool IsChanged => false;

    public virtual void AcceptChanges()
    {
    }

    [RelayCommand]
    protected virtual Task Load() => Task.CompletedTask;

    [RelayCommand(CanExecute = nameof(CanSave))]
    protected virtual Task Save() => Task.CompletedTask;
    protected virtual bool CanSave() => true;
}