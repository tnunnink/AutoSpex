﻿using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public class SpecListPageModel(NodeObserver node) : DetailPageModel
{
    public override string Route => $"Node/{Node.Id}/{Title}";
    public override string Title => "Specs";
    public NodeObserver Node { get; } = node;
}