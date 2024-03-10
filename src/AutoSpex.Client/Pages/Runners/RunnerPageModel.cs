using AutoSpex.Client.Observers;
using AutoSpex.Client.Pages.Projects;

namespace AutoSpex.Client.Pages;

public partial class RunnerPageModel(RunnerObserver runner) : DetailsPageModel
{
    public override string Route => $"Runner/{Runner.Id}";
    public override string Title => Runner.Name;
    public override string Icon => "Runner";
    public RunnerObserver Runner { get; } = runner;
}