using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;

namespace AutoSpex.Client.Pages;

public partial class SourceSettingsPageModel : PageViewModel
{
    private readonly SourceObserver _source;

    /// <inheritdoc/>
    public SourceSettingsPageModel(SourceObserver source) : base("Settings")
    {
        _source = source;

        /*Suppressions = new ObserverCollection<Suppression, SuppressionObserver>(
            refresh: () => _source.Model.Suppressions.Select(s => new SuppressionObserver(s)).ToList(),
            add: (_, s) => _source.Model.AddSuppression(s),
            remove: (_, s) => _source.Model.RemoveSuppression(s),
            count: () => _source.Model.Suppressions.Count()
        );

        Track(Suppressions);*/
    }

    public override string Route => $"Source/{_source.Id}/{Title}";
    public override string Icon => "IconLineSliders";
}