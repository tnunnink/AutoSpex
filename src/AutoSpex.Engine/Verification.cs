using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;

namespace AutoSpex.Engine;

public class Verification
{
    [JsonConstructor]
    private Verification(ResultState result, IEnumerable<Evaluation>? evaluations = default, long duration = 0)
    {
        Result = result ?? throw new ArgumentNullException(nameof(result));
        Evaluations = evaluations ?? [];
        Duration = duration;
    }

    /// <summary>
    /// The total result for all evaluations of this verification, indicating whether is Passed/Failed/Errored.
    /// </summary>
    [JsonInclude]
    [JsonConverter(typeof(SmartEnumNameConverter<ResultState, int>))]
    public ResultState Result { get; }

    /// <summary>
    /// The duration or time it took for all evaluations of this verification to process.
    /// </summary>
    [JsonInclude]
    public long Duration { get; }

    /// <summary>
    /// The collection of <see cref="Evaluation"/> that belong to the verification.
    /// These represent the checks that were made and grouped together to form this single verification.
    /// Verification can also be combined to further flatten and group sets of evaluations.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<Evaluation> Evaluations { get; }

    /// <summary>
    /// Represents a verification object with no evaluations and a result state of None.
    /// </summary>
    /// <remarks>
    /// The None property is a predefined instance of the <see cref="Verification"/> class.
    /// It is useful when you need to initialize a verification object with no evaluations.
    /// </remarks>
    public static Verification None => new(ResultState.None);

    /// <summary>
    /// Represents a verification object with no evaluations and a result state of Suppressed.
    /// </summary>
    public static Verification Suppressed => new(ResultState.Suppressed);

    /// <summary>
    /// Creates a new <see cref="Verification"/> for a single evaluation, using the result as the total result for the
    /// verification. 
    /// </summary>
    /// <param name="evaluation">The <see cref="Evaluation"/> the verification contains</param>
    /// <returns>A <see cref="Verification"/> with the result state of the provided evaluation.</returns>
    public static Verification For(Evaluation evaluation)
    {
        return new Verification(evaluation.Result, [evaluation]);
    }

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
        return new Verification(result, evaluations, duration);
    }

    /// <summary>
    /// Combines multiple verifications into a single <see cref="Verification"/>, using the maximum result
    /// from the provided instances.
    /// </summary>
    /// <param name="verifications">The collection of Verification instances.</param>
    /// <param name="duration">The optional duration in which it took to process the provided collection of evaluations.</param>
    /// <returns>A <see cref="Verification"/> with the max result state of the provided verification.</returns>
    public static Verification Merge(ICollection<Verification> verifications, long duration = 0)
    {
        var result = ResultState.MaxOrDefault(verifications.Select(e => e.Result).ToList());
        duration = duration > 0 ? duration : verifications.Sum(x => x.Duration);
        return new Verification(result, verifications.SelectMany(v => v.Evaluations).ToList(), duration);
    }
}