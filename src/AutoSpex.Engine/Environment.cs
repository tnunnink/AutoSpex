using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

/// <summary>
/// A configuration that contains the list of source L5X files to run a set of specifications against. This allows us to
/// set up a single thing that we can press run and get results from. This class also contains "overrides" which allow the
/// user to change the input data to variables that are referenced on any node in the project. Therefore, this represents
/// sort of the local runtime environment for the user to set up based on what they are testing. 
/// </summary>
public class Environment
{
    /// <summary>
    /// The <see cref="Guid"/> identifing this environment.
    /// </summary>
    [JsonInclude]
    public Guid EnvironmentId { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// The name of the environment. 
    /// </summary>
    public string Name { get; set; } = "Environment";

    /// <summary>
    /// A comment that describes the purpose or details of this environment. 
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Indicates that this is the target environment for which to run specifications against.
    /// </summary>
    public bool IsTarget { get; set; }

    /// <summary>
    /// The sources configured to be run by this <see cref="Environment"/>
    /// </summary>
    public List<Source> Sources { get; init; } = [];

    /// <summary>
    /// A default/empty environment instance.
    /// </summary>
    public static Environment Default => new()
    {
        Name = "Default",
        Comment = "A default environment config.",
        IsTarget = true
    };

    /// <summary>
    /// Deserializes a JSON string representation into an Environment object.
    /// </summary>
    /// <param name="configuration">The JSON string representation of the Environment object.</param>
    /// <returns>The deserialized Environment object.</returns>
    public static Environment Deserialize(string configuration)
    {
        return JsonSerializer.Deserialize<Environment>(configuration) ??
               throw new ArgumentException("Could not deserialize string as Environment object.");
    }

    /// <summary>
    /// Serializes the current Environment instance to a JSON string representation.
    /// </summary>
    /// <returns>A JSON string representing the serialized Environment.</returns>
    public string Serialize() => JsonSerializer.Serialize(this);

    /// <summary>
    /// Adds a new source to the environment.
    /// </summary>
    /// <param name="uri">The URI of the source to be added.</param>
    public Source Add(Uri uri)
    {
        var source = new Source(uri);
        Sources.Add(source);
        return source;
    }
}