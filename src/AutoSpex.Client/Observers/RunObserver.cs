using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

public class RunObserver : Observer<Run>
{
    /// <inheritdoc/>
    public RunObserver(Run model) : base(model)
    {
    }
    
    public ResultState Result => Model.Result;
}