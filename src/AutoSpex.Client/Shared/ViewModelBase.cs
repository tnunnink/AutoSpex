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
/// <remarks>
/// We will also supply our services directly using the app container to resolve dependencies.
/// This means unit tests will need to initialize the app, but I will not do too
/// much unit testing in the client layer, but rather rely more on integration testing.
/// </remarks>
[ObservableRecipient]
public abstract partial class ViewModelBase : ObservableValidator
{
    //Instead of injecting I'm resolving dependencies for the models directly.
    //I am choosing to do this to simplify the construction and since I am never going to do mocking,
    //but rather using the real implementations to preform integration testing for my application pages.
    //I think this is (should be) a small enough app where DI is overkill.
    //These services are singleton so memory allocation should not be an issue.
    protected readonly Shell Shell = Registrar.Resolve<Shell>();
    protected readonly IMediator Mediator = Registrar.Resolve<IMediator>();
    protected readonly Navigator Navigator = Registrar.Resolve<Navigator>();
    protected readonly Notifier Notifier = Registrar.Resolve<Notifier>();
    protected readonly Prompter Prompter = Registrar.Resolve<Prompter>();
    protected readonly Settings Settings = Registrar.Resolve<Settings>();

    /// <summary>
    /// A command to request navigation of the view model into the view of the application.
    /// </summary>
    /// <returns>The <see cref="Task"/> representing the async navigation request.</returns>
    /// <remarks>Deriving classes will implement this using the internal <see cref="Navigator"/> service.</remarks>
    [RelayCommand]
    protected abstract Task Navigate();
}