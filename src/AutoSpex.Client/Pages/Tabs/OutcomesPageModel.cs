using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class OutcomesPageModel : PageViewModel, IRecipient<OutcomeObserver.Show>
{
    /// <inheritdoc/>
    public OutcomesPageModel(RunObserver run) : base("Outcomes")
    {
        Run = run;
    }

    public override string Route => $"{nameof(Run)}/{Run.Id}/{Title}";
    public RunObserver Run { get; }

    [ObservableProperty] private bool _showDrawer;

    [ObservableProperty] private ResultDrawerPageModel? _resultDrawer;

    public override Task Load()
    {
        ResultDrawer = new ResultDrawerPageModel();
        RegisterDisposable(ResultDrawer);
        return base.Load();
    }

    public void Receive(OutcomeObserver.Show message)
    {
        if (ResultDrawer is null) return;
        if (!Run.Outcomes.Has(message.Outcome)) return;
        ResultDrawer.Outcome = message.Outcome;
        ShowDrawer = true;
    }
}