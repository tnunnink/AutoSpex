﻿using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

public class Verification
{
    [JsonConstructor]
    private Verification(ResultState result, long duration, double passRate)
    {
        Result = result ?? throw new ArgumentNullException(nameof(result));
        Duration = duration;
        PassRate = passRate;
    }

    private Verification(ResultState result, IEnumerable<Evaluation>? evaluations = null,
        long duration = 0,
        double passRate = 0.0)
    {
        Result = result ?? throw new ArgumentNullException(nameof(result));
        Evaluations = evaluations?.ToList() ?? [];
        Duration = duration;
        PassRate = passRate;
    }

    /// <summary>
    /// The total result for all evaluations of this verification, indicating whether is Passed/Failed/Errored.
    /// </summary>
    public ResultState Result { get; }

    /// <summary>
    /// The duration or time it took for all evaluations of this verification to process.
    /// </summary>
    public long Duration { get; }

    /// <summary>
    /// The pass rate of the verification, representing the percentage of evaluations that passed successfully.
    /// </summary>
    /// <remarks>
    /// This property is a float value indicating the ratio of successful evaluations to the total number of evaluations.
    /// </remarks>
    public double PassRate { get; }

    /// <summary>
    /// The collection of <see cref="Evaluation"/> that belong to the verification.
    /// These represent the checks that were made and grouped together to form this single verification.
    /// Verification can also be combined to further flatten and group sets of evaluations.
    /// </summary>
    public IReadOnlyCollection<Evaluation> Evaluations { get; } = [];

    /// <summary>
    /// Represents a verification object with no evaluations and a result state of None.
    /// </summary>
    /// <remarks>
    /// The None property is a predefined instance of the <see cref="Verification"/> class.
    /// It is useful when you need to initialize a verification object with no evaluations.
    /// </remarks>
    public static Verification None => new(ResultState.None);

    /// <summary>
    /// Creates a new <see cref="Verification"/> for a single evaluation, using the result as the total result for the
    /// verification. 
    /// </summary>
    /// <param name="evaluation">The <see cref="Evaluation"/> the verification contains</param>
    /// <returns>A <see cref="Verification"/> with the result state of the provided evaluation.</returns>
    public static Verification For(Evaluation evaluation) => new(evaluation.Result, [evaluation]);

    /// <summary>
    /// Creates a new <see cref="Verification"/> for the provided collection of evaluations by considering the maximum
    /// result state of the collection.
    /// </summary>
    /// <param name="evaluations">The collection of evaluation instances.</param>
    /// <param name="duration">The optional duration in which it took to process the provided collection of evaluations.</param>
    /// <returns>A <see cref="Verification"/> with the max result state of the provided evaluations.</returns>
    /// <remarks>
    /// In other words, the resulting verification will be Passed only if all provided evaluations Passed.
    /// If any one is marked Failed then the result for the verification will be Failed.
    /// If any one is marked Errored then the result for the verification will be Errored.
    /// </remarks>
    public static Verification For(ICollection<Evaluation> evaluations, long duration = 0)
    {
        var result = ResultState.MaxOrDefault(evaluations.Select(e => e.Result).ToList());
        var rate = evaluations.Percent(e => e.Result == ResultState.Passed);
        return new Verification(result, evaluations, duration, rate);
    }
}