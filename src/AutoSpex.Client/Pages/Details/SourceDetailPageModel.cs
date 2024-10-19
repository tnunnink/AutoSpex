using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using FluentResults;

namespace AutoSpex.Client.Pages;

public class SourceDetailPageModel : DetailPageModel
{
    /// <inheritdoc/>
    public SourceDetailPageModel(SourceObserver source) : base(source.Name)
    {
        Source = source;
        Track(Source);
    }

    public override string Route => $"{nameof(Source)}/{Source.Id}";
    public override string Icon => nameof(Source);
    public SourceObserver Source { get; private init; }

    public override async Task<Result> Save()
    {
        var result = await Mediator.Send(new SaveSource(Source));

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
        await Navigator.Navigate(() => new ContentPageModel(Source));
        await Navigator.Navigate(() => new OverridesPageModel(Source));
    }
}