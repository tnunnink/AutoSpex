using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

[PseudoClasses(":none", ":running", ":passed", ":failed", ":error")]
public class RunSummary : TemplatedControl
{
    public static readonly DirectProperty<RunSummary, bool> RunningProperty =
        AvaloniaProperty.RegisterDirect<RunSummary, bool>(
            nameof(Running), o => o.Running, (o, v) => o.Running = v);

    public static readonly DirectProperty<RunSummary, ResultState> ResultProperty =
        AvaloniaProperty.RegisterDirect<RunSummary, ResultState>(
            nameof(Result), o => o.Result, (o, v) => o.Result = v);

    public static readonly DirectProperty<RunSummary, string> SourceNameProperty =
        AvaloniaProperty.RegisterDirect<RunSummary, string>(
            nameof(SourceName), o => o.SourceName, (o, v) => o.SourceName = v);

    public static readonly DirectProperty<RunSummary, int> DurationProperty =
        AvaloniaProperty.RegisterDirect<RunSummary, int>(
            nameof(Duration), o => o.Duration, (o, v) => o.Duration = v);

    public static readonly DirectProperty<RunSummary, int> AverageProperty =
        AvaloniaProperty.RegisterDirect<RunSummary, int>(
            nameof(Average), o => o.Average, (o, v) => o.Average = v);

    public static readonly DirectProperty<RunSummary, int> TotalProperty =
        AvaloniaProperty.RegisterDirect<RunSummary, int>(
            nameof(Total), o => o.Total, (o, v) => o.Total = v);

    public static readonly DirectProperty<RunSummary, int> RanProperty =
        AvaloniaProperty.RegisterDirect<RunSummary, int>(
            nameof(Ran), o => o.Ran, (o, v) => o.Ran = v);

    public static readonly DirectProperty<RunSummary, int> PassedProperty =
        AvaloniaProperty.RegisterDirect<RunSummary, int>(
            nameof(Passed), o => o.Passed, (o, v) => o.Passed = v);

    public static readonly DirectProperty<RunSummary, int> FailedProperty =
        AvaloniaProperty.RegisterDirect<RunSummary, int>(
            nameof(Failed), o => o.Failed, (o, v) => o.Failed = v);

    public static readonly DirectProperty<RunSummary, int> ErroredProperty =
        AvaloniaProperty.RegisterDirect<RunSummary, int>(
            nameof(Errored), o => o.Errored, (o, v) => o.Errored = v);


    private bool _running;
    private ResultState _result = ResultState.None;
    private string _sourceName = "Source File";
    private int _duration;
    private int _average;
    private int _total;
    private int _ran;
    private int _passed;
    private int _failed;
    private int _errored;

    public bool Running
    {
        get => _running;
        set => SetAndRaise(RunningProperty, ref _running, value);
    }

    public ResultState Result
    {
        get => _result;
        set => SetAndRaise(ResultProperty, ref _result, value);
    }

    public string SourceName
    {
        get => _sourceName;
        set => SetAndRaise(SourceNameProperty, ref _sourceName, value);
    }

    public int Duration
    {
        get => _duration;
        set => SetAndRaise(DurationProperty, ref _duration, value);
    }

    public int Average
    {
        get => _average;
        set => SetAndRaise(AverageProperty, ref _average, value);
    }

    public int Total
    {
        get => _total;
        set => SetAndRaise(TotalProperty, ref _total, value);
    }

    public int Ran
    {
        get => _ran;
        set => SetAndRaise(RanProperty, ref _ran, value);
    }

    public int Passed
    {
        get => _passed;
        set => SetAndRaise(PassedProperty, ref _passed, value);
    }

    public int Failed
    {
        get => _failed;
        set => SetAndRaise(FailedProperty, ref _failed, value);
    }

    public int Errored
    {
        get => _errored;
        set => SetAndRaise(ErroredProperty, ref _errored, value);
    }
}