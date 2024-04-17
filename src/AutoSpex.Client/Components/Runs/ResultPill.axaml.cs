using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class ResultPill : TemplatedControl
{
    public static readonly DirectProperty<ResultPill, ResultState> ResultProperty =
        AvaloniaProperty.RegisterDirect<ResultPill, ResultState>(
            nameof(Result), o => o.Result, (o, v) => o.Result = v);

    private ResultState _result;

    public ResultState Result
    {
        get => _result;
        set => SetAndRaise(ResultProperty, ref _result, value);
    }
}