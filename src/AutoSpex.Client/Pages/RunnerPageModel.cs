using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class RunnerPageModel : PageViewModel,
    IRecipient<RunObserver.OpenRun>,
    IRecipient<RunObserver.CloseRun>
{
    public override string Title => "Runner";
    public override string Icon => "Runner";
    public ObservableCollection<RunObserver> Runs { get; } = [];

    [ObservableProperty] private RunObserver? _selectedRun;

    [RelayCommand]
    private void ClosePage()
    {
        Navigator.Send(new NavigationRequest(this, NavigationAction.Close));
    }

    [RelayCommand(CanExecute = nameof(CanSaveRun))]
    private async Task SaveRun()
    {
        if (SelectedRun is null) return;
        //todo need to open prompt to allow user to select container?

        var result = await Mediator.Send(new SaveRun(SelectedRun));
        if (result.IsFailed) return;

        //todo Once saved, should we open the node? should we just notify?
    }

    private bool CanSaveRun() => SelectedRun is not null && SelectedRun.IsChanged && !SelectedRun.IsErrored;

    public void Receive(RunObserver.OpenRun message)
    {
        if (!Runs.Contains(message.Run))
        {
            Runs.Add(message.Run);
        }

        SelectedRun = message.Run;


        //todo execute the run if it can be
    }

    public void Receive(RunObserver.CloseRun message)
    {
        Runs.Remove(message.Run);

        if (SelectedRun is not null && SelectedRun == message.Run)
        {
            SelectedRun = Runs.FirstOrDefault();
        }
    }

    /// <summary>
    /// If the selected run is set from the control by way of the user dropping in new nodes, we need to respond by
    /// adding it to the <see cref="Runs"/> collection to display as a tab.
    /// </summary>
    partial void OnSelectedRunChanged(RunObserver? value)
    {
        SaveRunCommand.NotifyCanExecuteChanged();

        if (value is null || Runs.Contains(value)) return;
        Runs.Add(value);
    }
}