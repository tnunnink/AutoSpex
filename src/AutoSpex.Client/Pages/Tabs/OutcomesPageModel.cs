using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages;

public partial class OutcomesPageModel : PageViewModel
{
    /// <inheritdoc/>
    public OutcomesPageModel(RunObserver run) : base("Outcomes")
    {
        Run = run;
    }

    public override string Route => $"{nameof(Run)}/{Run.Id}/{Title}";
    public RunObserver Run { get; }

    [ObservableProperty] private ResultDrawerPageModel? _resultDrawer;

    public override Task Load()
    {
        ResultDrawer = new ResultDrawerPageModel();
        RegisterDisposable(ResultDrawer);
        return base.Load();
    }
}