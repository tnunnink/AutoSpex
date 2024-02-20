using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class CriterionEntry : TemplatedControl
{
    #region Properties

    public static readonly DirectProperty<CriterionEntry, CriterionObserver> CriterionProperty =
        AvaloniaProperty.RegisterDirect<CriterionEntry, CriterionObserver>(
            nameof(Criterion), o => o.Criterion, (o, v) => o.Criterion = v);

    #endregion

    private CriterionObserver _criterion = new(new Criterion());

    public CriterionObserver Criterion
    {
        get => _criterion;
        set => SetAndRaise(CriterionProperty, ref _criterion, value);
    }
}