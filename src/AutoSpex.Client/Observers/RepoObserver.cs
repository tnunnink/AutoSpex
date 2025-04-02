using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

public class RepoObserver(Repo model) : Observer<Repo>(model)
{
    public override Guid Id => Model.RepoId;
    public string Location => Model.Location;

    public override string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v, true);
    }

    /// <inheritdoc />
    public override bool Filter(string? filter)
    {
        return base.Filter(filter) || Location.Satisfies(filter);
    }
}