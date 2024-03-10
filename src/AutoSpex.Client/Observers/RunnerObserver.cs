using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

public class RunnerObserver(Runner model) : Observer<Runner>(model)
{
    public override Guid Id => Model.RunnerId;

    public string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (m, s) => m.Name = s, true);
    }
}