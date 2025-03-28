using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;

namespace AutoSpex.Client.Pages;

[UsedImplicitly]
public partial class SourceSelectorPageModel : PageViewModel
{
    public override bool Reload => true;
    public ObserverCollection<Source, SourceObserver> Sources { get; } = [];

    protected override void FilterChanged(string? filter)
    {
        Sources.Filter(filter);
    }
}