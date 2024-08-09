using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;

namespace AutoSpex.Engine;

/// <summary>
/// An object containing the resulting data from running a <see cref="Spec"/> against a given <see cref="Source"/>.
/// </summary>
public class Outcome
{
    public Outcome(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (node.Type != NodeType.Spec)
            throw new ArgumentException("Node type invalid for outcome object.");

        SpecId = node.NodeId;
        Name = node.Name;
        Result = ResultState.None;
        Verifications = [];
    }

    public Outcome(Spec spec, long duration, ICollection<Verification> verifications)
    {
        ArgumentNullException.ThrowIfNull(spec);
        SpecId = spec.SpecId;
        Name = spec.Name;
        Result = ResultState.MaxOrDefault(verifications.Select(x => x.Result).ToList());
        Duration = duration;
        Verifications = verifications;
    }

    /// <summary>
    /// The <see cref="Guid"/> which represents the Spec configuration this outcome was produced by.
    /// </summary>
    [JsonInclude]
    public Guid SpecId { get; private set; }

    /// <summary>
    /// The name of the spec that this outcome represents.
    /// </summary>
    [JsonInclude]
    public string Name { get; private set; }

    /// <summary>
    /// The result of the outcome. This represents if the spec passed, failed, errored, etc.
    /// </summary>
    [JsonConverter(typeof(SmartEnumNameConverter<ResultState, int>))]
    [JsonInclude]
    public ResultState Result { get; private set; }

    /// <summary>
    /// Represents the duration of running a spec against a given source or collection of sources.
    /// </summary>
    [JsonInclude]
    public long Duration { get; private set; }

    /// <summary>
    /// the collection of <see cref="Verification"/> that contain the detailed checks and corresponding result and source
    /// information. This data is not serialized and persisted to conserve space.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<Verification> Verifications { get; private set; }

    /// <summary>
    /// Updates the current Outcome object with the values from the provided Outcome object.
    /// </summary>
    /// <param name="result">The Outcome object containing the updated values.</param>
    public void Update(Outcome result)
    {
        ArgumentNullException.ThrowIfNull(result);

        Result = result.Result;
        Duration = result.Duration;
        Verifications = result.Verifications;
    }
}