using System.Linq;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentResults;

namespace AutoSpex.Client.Pages;

public partial class SpecPageModel(NodeObserver node) : NodePageModel(node)
{
    [ObservableProperty] private SpecObserver? _spec;

    /// <inheritdoc />
    public override async Task Load()
    {
        await base.Load();

        var result = await Mediator.Send(new GetSpec(Node.Id));
        if (result.IsFailed) return;

        Spec = new SpecObserver(result.Value);
        Track(Spec);

        SaveCommand.NotifyCanExecuteChanged();
    }

    protected override async Task<Result> Save()
    {
        if (Spec is null) return Result.Fail("Can not save uninitialized spec object.");

        var baseResult = await base.Save();
        var specResult = await Mediator.Send(new SaveSpec(Spec));
        var result = Result.Merge(baseResult, specResult);

        if (result.IsSuccess)
        {
            NotifySaveSuccess();
            AcceptChanges();
        }

        return result;
    }
    
    protected override bool CanSave() => base.CanSave() && Spec is not null;

    protected override async Task Run(SourceObserver? source)
    {
        source ??= Sources.Single(s => s.IsSelected);
        var run = new Run(Node.Id, source.Id);
        var page = await Navigator.Navigate(() => new RunPageModel(run));
        var specs = Node.CheckedSpecs;
        await page.ExecuteCommand.ExecuteAsync(null);
    }
}