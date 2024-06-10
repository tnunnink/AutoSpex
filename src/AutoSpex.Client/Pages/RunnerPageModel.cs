using System.Threading;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Pages;

public partial class RunnerPageModel : PageViewModel
{
    private CancellationTokenSource? _cancellation;
    public override string Title => "Runner";
    public override string Icon => "Runner";

    [ObservableProperty] private RunObserver? _selectedRun;
    public ObserverCollection<Outcome, OutcomeObserver> Outcomes { get; } = [];

    [RelayCommand]
    private void ClosePage()
    {
        Navigator.Send(new NavigationRequest(this, NavigationAction.Close));
    }

    /// <summary>
    /// Command to start execution of the provided run.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task Start()
    {
        if (SelectedRun is null) return;
        _cancellation = new CancellationTokenSource();
        await SelectedRun.Execute(_cancellation.Token).ConfigureAwait(true);
    }

    private bool CanStart() => SelectedRun is not null;

    /// <summary>
    /// Command to cancel execution of the current run.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop() => _cancellation?.Cancel();

    private bool CanStop() => SelectedRun?.Result == ResultState.Pending;
}