using System;

namespace AutoSpex.Client.Shared;

public abstract class DetailViewModel : ViewModelBase
{
    public abstract Guid Id { get; }
    public abstract string Label { get; }
    public abstract string Icon { get; }
}