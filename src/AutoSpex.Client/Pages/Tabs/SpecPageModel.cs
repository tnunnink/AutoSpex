using System.Threading.Tasks;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;
using L5Sharp.Core;

namespace AutoSpex.Client.Pages;

public partial class SpecPageModel(NodeObserver node) : PageViewModel("Spec")
{
    public override string Route => $"{node.Type}/{node.Id}/{Title}";
    public override string Icon => "IconFilledClipboard";

    [ObservableProperty] private SpecObserver? _spec;

    [ObservableProperty] private bool _showDrawer;

    [ObservableProperty] private ResultDrawerPageModel? _resultDrawer;

    /// <inheritdoc />
    public override async Task Load()
    {
        if (node.IsVirtual)
        {
            Spec = node.Model.Spec;
        }
        else
        {
            var result = await Mediator.Send(new LoadSpec(node.Id));
            if (Notifier.ShowIfFailed(result)) return;
            Spec = new SpecObserver(result.Value);
        }

        Track(Spec);

        ResultDrawer = new ResultDrawerPageModel();
        RegisterDisposable(ResultDrawer);
    }

    /// <inheritdoc />
    public override async Task<Result> Save(Result? result = default)
    {
        if (Spec is null)
            return Result.Fail($"Spec configuration was not correctly loaded for {node.Name}.");

        var payload = node.Model.Copy();
        payload.Configure(Spec.Model);
        return await Mediator.Send(new SaveSpec(payload));
    }

    /// <summary>
    /// Uses the current test runner page to execeute this specification.
    /// Opens the runner drawer if not alread open.
    /// </summary>
    [RelayCommand]
    private async Task TestSpec()
    {
        if (Spec is null || ResultDrawer is null) return;
        if (!TryGetContent(out var content)) return;

        ShowDrawer = true;

        //Resolve any configured references to external sources or data.
        var resolved = await Mediator.Send(new ResolveReferences([Spec]));
        if (Notifier.ShowIfFailed(resolved)) return;

        var result = await Spec.Model.RunAsync(content);
        var outcome = new Outcome();
        outcome.Apply(result);

        ResultDrawer.Outcome = new OutcomeObserver(outcome);
    }

    /// <summary>
    /// Attempts to get the L5X content from the targeted source file. If not source is targeted/exists, or if the target
    /// does not have loaded content, we show and error to the user informing them to load and/or target a valid source.
    /// </summary>
    private bool TryGetContent(out L5X content)
    {
        //This local run always uses the target source content.
        //And since that content is already memory we don't want to reload it.
        var message = new Observer.Get<SourceObserver>(s => s is { IsTarget: true, Model.Content: not null });
        Messenger.Send(message);

        //If no source is targetd show an error to the user.
        if (!message.HasReceivedResponse)
        {
            Notifier.ShowError("No source is targeted", "Select a source before running this spec.");
            content = null!;
            return false;
        }

        content = message.Response.Model.Content!;
        return true;
    }
}