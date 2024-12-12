using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentResults;

namespace AutoSpex.Client.Pages;

public partial class SourceDetailPageModel : DetailPageModel
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

    [ObservableProperty] private bool _showDrawer;

    public override async Task<Result> Save(Result? result = default)
    {
        result = await Mediator.Send(new SaveSource(Source));
        return await base.Save(result);
    }

    /// <inheritdoc />
    protected override async Task NavigateContent()
    {
        await Navigator.Navigate(() => new ContentPageModel(Source));
        await Navigator.Navigate(() => new SuppressionsPageModel(Source));
        await Navigator.Navigate(() => new OverridesPageModel(Source));
    }
}