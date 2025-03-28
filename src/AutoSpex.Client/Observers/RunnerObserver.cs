using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using JetBrains.Annotations;

namespace AutoSpex.Client.Observers;

[UsedImplicitly]
public class RunnerObserver(RunConfig model) : Observer<RunConfig>(model)
{
    public override Guid Id => Model.ConfigId;
    public override string Name => Model.Name;

    /// <summary>
    /// A message that is sent to start execution of the provided runner instance.
    /// </summary>
    public record Run(RunnerObserver Runner);
}