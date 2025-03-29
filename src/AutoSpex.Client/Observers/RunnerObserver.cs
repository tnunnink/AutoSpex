using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using JetBrains.Annotations;

namespace AutoSpex.Client.Observers;

[UsedImplicitly]
public class RunnerObserver(RunContext model) : Observer<RunContext>(model)
{
    public override Guid Id => Model.RunId;
    public override string Name => Model.Name;

    /// <summary>
    /// A message that is sent to start execution of the provided runner instance.
    /// </summary>
    public record Run(RunnerObserver Runner);
}