using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

public class RunObserver : Observer<RunResult>
{
    /// <inheritdoc/>
    public RunObserver(RunResult model) : base(model)
    {
    }
    
    public ResultState Result => Model.Result;
}