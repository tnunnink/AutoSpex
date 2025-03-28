using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using MediatR;

namespace AutoSpex.Client.Services;

[UsedImplicitly]
public class Runner(IMessenger messenger, IMediator mediator, Notifier notifier)
{
    private readonly List<ResultObserver> _results = [];

    public IEnumerable<ResultObserver> Results => _results;
    public ResultState Result { get; private set; } = ResultState.None;

    public async Task Run(RunConfig config, CancellationToken token)
    {
        if (Result == ResultState.Pending)
        {
            notifier.ShowWarning(
                "Run already processing",
                "Please stop or wait for the current run to complete before starting a new run."
            );
            return;
        }

        Result = ResultState.Pending;
        _results.Clear();

        try
        {
            Result = ResultState.MaxOrDefault(_results.Select(r => r.Result).ToArray());
        }
        catch (OperationCanceledException)
        {
            notifier.ShowWarning("Run canceled", "The current run was canceled prior to finishing execution.");
            Result = ResultState.Inconclusive;
        }
    }

    public record RunStarting(Node Node, Source Source);

    public record ResultChanged(Verification Verification);
}