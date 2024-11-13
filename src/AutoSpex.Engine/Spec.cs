using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine;

/// <summary>
/// The configuration that defines the specification to run against a given source. A spec is basically a definition
/// for how to get data from an L5X file and check the values of for the returned data to verify it's content. So this
/// is the primary means though which we set up and verify L5X content. A spec will query a specific element type,
/// filter those resulting element, and verify the candidate element all using the configured <see cref="Criterion"/>
/// objects. This object is persisted to the database to be reloaded and executed against source files as needed.
/// </summary>
public class Spec() : IEquatable<Spec>
{
    /// <summary>
    /// Creates a new spec with the provided element type.
    /// </summary>
    /// <param name="element">The <see cref="Element"/> type the spec represents.</param>
    public Spec(Element element) : this()
    {
        Element = element ?? throw new ArgumentNullException(nameof(element));
    }

    /// <summary>
    /// The unique id that indietifies this spec aprart from others.
    /// </summary>
    [JsonInclude]
    public Guid SpecId { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// The target <see cref="Engine.Element"/> this query will search for.
    /// </summary>
    [JsonConverter(typeof(SmartEnumNameConverter<Element, string>))]
    [JsonInclude]
    public Element Element { get; set; } = Element.Default;

    /// <summary>
    /// The collection of <see cref="Criterion"/> that define how to filter elements to return candidates for verification.
    /// </summary>
    [JsonInclude]
    public List<Criterion> Filters { get; private init; } = [];

    /// <summary>
    /// The collection of <see cref="Criterion"/> that define the checks to perform for each candidate element.
    /// </summary>
    [JsonInclude]
    public List<Criterion> Verifications { get; private init; } = [];

    /// <summary>
    /// Creates a new <see cref="Spec"/> with the provided configuration.
    /// </summary>
    /// <param name="config">The config to apply to the new spec.</param>
    /// <returns>A <see cref="Spec"/> with the provided config applied.</returns>
    public static Spec Configure(Action<Spec> config)
    {
        var spec = new Spec();
        config.Invoke(spec);
        return spec;
    }

    /// <summary>
    /// Configures the <see cref="Element"/> type to query when this specification is run.
    /// </summary>
    /// <param name="element">The <see cref="Engine.Element"/> option to query.</param>
    /// <returns>The configured spec instance.</returns>
    public Spec Query(Element element)
    {
        Element = element ?? throw new ArgumentNullException(nameof(element));
        return this;
    }

    /// <summary>
    /// Simply adds a new <see cref="Criterion"/> to the <see cref="Filters"/> collection with the specified arguments.
    /// </summary>
    /// <param name="property">The property name to select for the filter criterion.</param>
    /// <param name="operation">The <see cref="Operation"/> the criterion will perform.</param>
    /// <param name="args">The argument to supply to the criterion operation.</param>
    /// <returns>The configured spec instance.</returns>
    public Spec Filter(string? property, Operation operation, object? args = default)
    {
        Filters.Add(new Criterion(Element.Property(property), operation, args));
        return this;
    }

    /// <summary>
    /// Simply adds a new <see cref="Criterion"/> to the <see cref="Verifications"/> collection with the specified arguments.
    /// </summary>
    /// <param name="property">The property name to select for the filter criterion.</param>
    /// <param name="operation">The <see cref="Operation"/> the criterion will perform.</param>
    /// <param name="args">The argument to supply to the criterion operation.</param>
    /// <returns>The configured spec instance.</returns>
    public Spec Verify(string? property, Operation operation, object? args = default)
    {
        Verifications.Add(new Criterion(Element.Property(property), operation, args));
        return this;
    }

    /// <summary>
    /// Simply adds a new <see cref="Criterion"/> to the <see cref="Verifications"/> collection with the specified arguments.
    /// </summary>
    /// <param name="property">The property name to select for the filter criterion.</param>
    /// <param name="negation">The negation option to use for the vierifcation.</param>
    /// <param name="operation">The <see cref="Operation"/> the criterion will perform.</param>
    /// <param name="args">The argument to supply to the criterion operation.</param>
    /// <returns>The configured spec instance.</returns>
    public Spec Verify(string? property, Negation negation, Operation operation, object? args = default)
    {
        Verifications.Add(new Criterion(Element.Property(property), operation, args) { Negation = negation });
        return this;
    }

    /// <summary>
    /// Creates a new <see cref="Spec"/> with the same configuration but default id and node properties.
    /// </summary>
    /// <returns>A new <see cref="Spec"/> object.</returns>
    public Spec Duplicate()
    {
        return new Spec
        {
            Element = Element,
            Filters = Filters.Select(x => x.Duplicate()).ToList(),
            Verifications = Verifications.Select(x => x.Duplicate()).ToList()
        };
    }

    /// <summary>
    /// Creates a deep copy of this spec instance, returning the same configuration and same <see cref="SpecId"/>.
    /// </summary>
    /// <returns>The new copied spec instance.</returns>
    public Spec Copy()
    {
        var data = JsonSerializer.Serialize(this);
        var spec = JsonSerializer.Deserialize<Spec>(data);
        return spec ?? throw new ArgumentException("Could not materialize new spec instance.");
    }

    /// <summary>
    /// Checks if the Spec contains the given Criterion in its Filters or Verifications.
    /// </summary>
    /// <param name="criterion">The Criterion to check for within the Spec.</param>
    /// <returns>True if the Spec contains the Criterion, false otherwise.</returns>
    public bool Contains(Criterion criterion)
    {
        return Filters.Any(c => c.Contains(criterion)) || Verifications.Any(c => c.Contains(criterion));
    }

    /// <summary>
    /// Determines whether the Spec contains the given text in any of its Filters or Verifications.
    /// </summary>
    /// <param name="text">The text to search for within the Spec's Filters or Verifications.</param>
    /// <returns>True if the text is found in any of the Filters or Verifications of the Spec; otherwise, false.</returns>
    public bool Contains(string? text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        return Filters.Any(c => c.ToString().Satisfies(text)) || Verifications.Any(c => c.ToString().Satisfies(text));
    }

    /// <summary>
    /// Represents the criteria that define a Spec, including filters and verifications.
    /// </summary>
    /// <returns>An IEnumerable of Criterion objects representing the criteria.</returns>
    public IEnumerable<Criterion> Criteria() => Filters.Concat(Verifications);

    /// <summary>
    /// Runs the configured specification against the provided L5X content and returns a verification result.
    /// </summary>
    /// <param name="content">The L5X content to run this specification against.</param>
    /// <returns>The <see cref="Verification"/> containing the specification results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the content parameter is null.</exception>
    public Verification Run(L5X content)
    {
        return RunSpec(content);
    }

    /// <summary>
    /// Runs the configured spec against the provided L5X content and returns a verification result.
    /// </summary>
    /// <param name="content">The L5X content to run this specification against.</param>
    /// <param name="token">The optional cancellation token to stop the run.</param>
    /// <returns>The <see cref="Verification"/> containing the specification results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the content parameter is null.</exception>
    public Task<Verification> RunAsync(L5X content, CancellationToken token = default)
    {
        return Task.Run(() => RunSpec(content), token);
    }

    public bool Equals(Spec? other)
    {
        if (ReferenceEquals(null, other)) return false;
        return ReferenceEquals(this, other) || SpecId.Equals(other.SpecId);
    }

    public override bool Equals(object? obj) => obj is Spec other && Equals(other);

    public override int GetHashCode() => SpecId.GetHashCode();

    /// <summary>
    /// Executes the configured specification against the provided source content.
    /// </summary>
    /// <param name="content">The L5X content to run this spec against.</param>
    /// <returns>A <see cref="Verification"/> indicating the result of the specification.</returns>
    private Verification RunSpec(L5X content)
    {
        ArgumentNullException.ThrowIfNull(content);

        try
        {
            var stopwatch = Stopwatch.StartNew();

            //1. Execute the configured query.
            var elements = content.Query(Element.Type);

            //2. Filter the resulting elements.
            var candidates = elements.Where(e => Filters.All(f => f.Evaluate(e))).ToList();

            //3. Verify the candidates elements.
            var evaluations = candidates.SelectMany(c => Verifications.Select(v => v.Evaluate(c))).ToList();

            stopwatch.Stop();

            //Create the verification object that contains the total result and all evaluation information.
            return Verification.For(evaluations, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception e)
        {
            //If anything fails just return a single failed verification with the exception message.
            return Verification.For(Evaluation.Errored(e));
        }
    }
}