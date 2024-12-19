using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
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
    private Task Run()
    {
        throw new NotImplementedException();
    }

    private bool CanRun() => Source.Model.Content is not null;

    /// <inheritdoc />
    protected override async Task NavigatePages()
    {
        await Navigator.Navigate(() => new SourceQueryPageModel(Source));
        await Navigator.Navigate(() => new SourceSettingsPageModel(Source));
        await Navigator.Navigate(() => new VariablesPageModel(Source));
        await Navigator.Navigate(() => new HistoryPageModel(Source));
        await Navigator.Navigate(() => new CommentsPageModel(Source));
        await Navigator.Navigate(() => new InfoPageModel(Source));
    }
}