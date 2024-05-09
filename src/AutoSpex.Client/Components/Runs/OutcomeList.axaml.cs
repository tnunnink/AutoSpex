using System.Collections.ObjectModel;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class OutcomeList : TemplatedControl
{
    public static readonly DirectProperty<OutcomeList, ObservableCollection<OutcomeObserver>> OutcomesProperty =
        AvaloniaProperty.RegisterDirect<OutcomeList, ObservableCollection<OutcomeObserver>>(
            nameof(Outcomes), o => o.Outcomes, (o, v) => o.Outcomes = v);

    private ObservableCollection<OutcomeObserver> _outcomes = [];

    public ObservableCollection<OutcomeObserver> Outcomes
    {
        get => _outcomes;
        set => SetAndRaise(OutcomesProperty, ref _outcomes, value);
    }

    public ObservableCollection<EvaluationObserver> Evaluations { get; } = [];
}