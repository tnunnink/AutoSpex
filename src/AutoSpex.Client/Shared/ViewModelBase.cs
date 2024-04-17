using AutoSpex.Client.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;

namespace AutoSpex.Client.Shared;

/// <summary>
/// This base view model class combines the Community Toolkit <see cref="ObservableValidator"/> and
/// <see cref="ObservableRecipient"/> to provide a fully featured view model capable of notification, validation,
/// and messaging (including activation and deactivation).
/// </summary>
[ObservableRecipient]
public abstract partial class ViewModelBase : ObservableValidator
{
    //Instead of injecting I'm resolving dependencies for the models directly.
    //I am choosing to do this to simplify the construction and since I am never going to do mocking,
    //but rather using the real implementations to preform integration testing for my application pages.
    protected readonly Shell Shell = Container.Resolve<Shell>();
    protected readonly IMediator Mediator = Container.Resolve<IMediator>();
    protected readonly Navigator Navigator = Container.Resolve<Navigator>();
    protected readonly Notifier Notifier = Container.Resolve<Notifier>();
    protected readonly Prompter Prompter = Container.Resolve<Prompter>();
    protected readonly Manager Manager = Container.Resolve<Manager>();

    /// <summary>
    /// A command to request navigation of the this view model into the view of the application.
    /// </summary>
    /// <returns>The <see cref="Task"/> representing the async navigation request.</returns>
    /// <remarks>Deriving classes will implement this using the internal <see cref="Navigator"/> service.</remarks>
    [RelayCommand]
    protected abstract Task Navigate();
}