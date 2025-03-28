using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    /// Creates a new spec with the data from another spec.
    /// </summary>
    private Spec(Spec spec) : this()
    {
        Query = spec.Query;
        Verify = spec.Verify;
    }

    /// <summary>
    /// Creates a new spec with the provided element type.
    /// </summary>
    /// <param name="element">The <see cref="Element"/> type the spec represents.</param>
    public Spec(Element element) : this()
    {
        Query = new Query(element);
    }

    /// <summary>
    /// The version that identifies the JSON schema of the current object.
    /// </summary>
    [JsonInclude]
    private int SchemaVersion { get; } = 2;

    /// <summary>
    /// The unique id that indietifies this spec aprart from others.
    /// </summary>
    [JsonInclude]
    public Guid SpecId { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// The id of the <see cref="Node"/> this spec belongs to.
    /// </summary>
    [JsonIgnore]
    public Guid NodeId { get; private init; } = Guid.Empty;

    /// <summary>
    /// The <see cref="Engine.Query"/> that defines what data to retrieve from the source.
    /// </summary>
    [JsonInclude]
    public Query Query { get; private init; } = new(Element.Default);

    /// <summary>
    /// The collection of <see cref="Criterion"/> that define what this specification is verifying.
    /// </summary>
    [JsonInclude]
    public Verify Verify { get; private init; } = new();

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
    /// This is exlusivley a method to help easily configure a simple spec object for testing purposes.
    /// The application will primarily add and configure steps manually.
    /// </summary>
    /// <param name="element">The <see cref="Engine.Element"/> option to query.</param>
    /// <returns>The configured spec instance.</returns>
    public Spec Get(Element element)
    {
        Query.Element = element;
        return this;
    }

    /// <summary>
    /// Adds a <see cref="Criterion"/> as a filter to this spec. If no filter step exists, it will be created. Otherwise,
    /// this criterion is added to the first found filter step.
    /// This is exlusivley a method to help easily configure a simple spec object for testing purposes.
    /// The application will primarily add and configure steps manually.
    /// </summary>
    /// <param name="property">The property name to select for the criterion.</param>
    /// <param name="operation">The <see cref="Operation"/> the criterion will perform.</param>
    /// <param name="argument">The argument to supply to the criterion operation.</param>
    /// <returns>The current configured <see cref="Spec"/> instance.</returns>
    public Spec Where(string property, Operation operation, object? argument = default)
    {
        if (Query.Steps.All(s => s is not Filter))
            Query.Steps.Add(new Filter());

        var filter = (Filter)Query.Steps.First(x => x is Filter);
        filter.Criteria.Add(new Criterion(property, operation, argument));
        return this;
    }

    /// <summary>
    /// Adds a new selection to the query's selection list based on the specified property.
    /// </summary>
    /// <param name="property">The property to select.</param>
    /// <returns>The updated Spec instance with the new selection added.</returns>
    public Spec Select(string property)
    {
        var select = new Select(property);
        Query.Steps.Add(select);
        return this;
    }

    /// <summary>
    /// Adds a <see cref="Criterion"/> as a verification to this spec. All specs are initialized with a single verify step,
    /// so this criterion will just be added to that step.
    /// This is exlusivley a method to help easily configure a simple spec object for testing purposes.
    /// The application will primarily add and configure steps manually.
    /// </summary>
    /// <param name="property">The property name to select for the criterion.</param>
    /// <param name="operation">The <see cref="Operation"/> the criterion will perform.</param>
    /// <param name="argument">The argument to supply to the criterion operation.</param>
    /// <returns>The current configured <see cref="Spec"/> instance.</returns>
    public Spec Validate(string property, Operation operation, object? argument = default)
    {
        Verify.Criteria.Add(new Criterion(property, operation, argument));
        return this;
    }

    /// <summary>
    /// Adds a <see cref="Criterion"/> as a verification to this spec. All specs are initialized with a single verify step,
    /// so this criterion will just be added to that step.
    /// This is exlusivley a method to help easily configure a simple spec object for testing purposes.
    /// The application will primarily add and configure steps manually.
    /// </summary>
    /// <param name="property">The property name to select for the criterion.</param>
    /// <param name="negation">The negation option to use for the criterion.</param>
    /// <param name="operation">The <see cref="Operation"/> the criterion will perform.</param>
    /// <param name="argument">The argument to supply to the criterion operation.</param>
    /// <returns>The current configured <see cref="Spec"/> instance.</returns>
    public Spec Validate(string property, Negation negation, Operation operation, object? argument = default)
    {
        Verify.Criteria.Add(new Criterion(property, negation, operation, argument));
        return this;
    }

    /// <summary>
    /// Creates a new <see cref="Spec"/> with the same configuration but different <see cref="SpecId"/>.
    /// </summary>
    /// <returns>A new <see cref="Spec"/> object.</returns>
    public Spec Duplicate()
    {
        var data = JsonSerializer.Serialize(this);
        var spec = JsonSerializer.Deserialize<Spec>(data)!;
        return new Spec(spec);
    }

    /// <summary>
    /// Retrieves all <see cref="Criterion"/> found in the spec in both verifications and filters.
    /// </summary>
    /// <returns>A flat collection of Criterion objects found in the spec.</returns>
    public IEnumerable<Criterion> GetAllCriteria()
    {
        var filters = Query.Steps.Where(s => s is Filter).Cast<Filter>().SelectMany(f => f.Criteria).ToList();
        return filters.Concat(Verify.Criteria);
    }

    /// <summary>
    /// Retrieves all <see cref="Reference"/> arguments from the criteria associated with the spec.
    /// </summary>
    /// <returns>A collection of Reference objects found in the criteria.</returns>
    public IEnumerable<Reference> GetAllReferences()
    {
        return GetAllCriteria().Select(c => c.Argument).Where(a => a is Reference).Cast<Reference>();
    }

    /// <summary>
    /// Runs the configured specification against the provided L5X content and returns a verification result.
    /// </summary>
    /// <param name="content">The L5X content to run this specification against.</param>
    /// <returns>The <see cref="Verification"/> containing the specification results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is null.</exception>
    public Evaluation[] Run(L5X content)
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
    public Task<Evaluation[]> RunAsync(L5X content, CancellationToken token = default)
    {
        return Task.Run(() => RunSpec(content), token);
    }

    /// <summary>
    /// Executes the configured specification against the provided source content.
    /// </summary>
    /// <param name="content">The L5X content to run this spec against.</param>
    /// <returns>A <see cref="Verification"/> indicating the result of the specification.</returns>
    private Evaluation[] RunSpec(L5X content)
    {
        ArgumentNullException.ThrowIfNull(content);

        try
        {
            var candidates = Query.Execute(content).ToList();
            var evaluations = Verify.Process(candidates).Cast<Evaluation>();
            return evaluations.ToArray();
        }
        catch (Exception e)
        {
            //If anything fails just return a single failed verification with the exception message.
            return [Evaluation.Errored(e)];
        }
    }

    public bool Equals(Spec? other)
    {
        if (ReferenceEquals(null, other)) return false;
        return ReferenceEquals(this, other) || SpecId.Equals(other.SpecId);
    }

    public override bool Equals(object? obj) => obj is Spec other && Equals(other);
    public override int GetHashCode() => SpecId.GetHashCode();
}