using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using FluentResults;

namespace AutoSpex.Client.Pages;

public partial class SourceDetailPageModel : DetailPageModel
{
    /// <inheritdoc/>
    public SourceDetailPageModel(SourceObserver source) : base(source.Name)
    {
        Source = source;
        RegisterDisposable(Source);
    }

    public override string Route => $"{nameof(Source)}/{Source.Id}";
    public override string Icon => nameof(Source);
    public SourceObserver Source { get; }

    public override async Task<Result> Save(Result? result = default)
    {
        result = await Mediator.Send(new SaveSource(Source));
        return await base.Save(result);
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task Run()
    {
        var page = Navigator.Open<QueryPageModel>($"Source/{Source.Id}/Query");
        await page.ExecuteCommand.ExecuteAsync(null);
    }

    private bool CanRun() => Source.Model.Content is not null;

    /// <inheritdoc />
    protected override async Task NavigatePages()
    {
        await Navigator.Navigate(() => new QueryPageModel(Source));
        await Navigator.Navigate(() => new ActionsPageModel(Source));
        await Navigator.Navigate(() => new HistoryPageModel(Source));
        /*await Navigator.Navigate(() => new CommentsPageModel(Source));*/
        await Navigator.Navigate(() => new InfoPageModel(Source));
    }
}