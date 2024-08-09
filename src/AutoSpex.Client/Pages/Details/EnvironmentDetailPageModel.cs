using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using FluentResults;

namespace AutoSpex.Client.Pages;

public class EnvironmentDetailPageModel : DetailPageModel
{
    /// <inheritdoc/>
    public EnvironmentDetailPageModel(EnvironmentObserver environment)
    {
        Environment = environment;
    }

    public override string Route => $"{nameof(Environment)}/{Environment.Id}";
    public override string Title => Environment.Name;
    public override string Icon => nameof(Environment);
    public EnvironmentObserver Environment { get; private set; }

    /// <inheritdoc />
    /// <remarks>
    /// We need to first fully load the environment config to represent this detail page, which includes sources and
    /// overrides. We do this prior to navigating tabs, so that they get the fully loaded instance.
    /// </remarks>
    public override async Task Load()
    {
        await LoadEnvironment();
        await base.Load();
    }

    public override async Task<Result> Save()
    {
        var result = await Mediator.Send(new SaveEnvironment(Environment));
        
        if (result.IsFailed)
        {
            NotifySaveFailed(result);
        }
        else
        {
            NotifySaveSuccess();
            AcceptChanges();
        }

        return result;
    }

    /// <inheritdoc />
    protected override async Task NavigateTabs()
    {
        await Navigator.Navigate(() => new SourcesPageModel(Environment));
        await Navigator.Navigate(() => new OverridesPageModel(Environment));
    }

    /// <summary>
    /// Loads the full environment object from the database, which includes the configured sources and overrides.
    /// </summary>
    private async Task LoadEnvironment()
    {
        var result = await Mediator.Send(new LoadEnvironment(Environment.Id));
        if (result.IsFailed) return;
        Environment = new EnvironmentObserver(result.Value);
        Track(Environment);
    }
}