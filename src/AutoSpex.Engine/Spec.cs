using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
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
[PublicAPI]
public class Spec()
{
    /// <summary>
    /// Creates a new <see cref="Spec"/> with the provided node id.
    /// </summary>
    /// <param name="node">The <see cref="Node"/> that this spec is attached to.</param>
    public Spec(Node node) : this()
    {
        ArgumentNullException.ThrowIfNull(node);

        if (node.Type != NodeType.Spec)
            throw new ArgumentException("Can only create spec with a spec type node.");

        Node = node;
    }

    /// <summary>
    /// The unique identifier of the spec. This is the NodeId property of the associated Node object.
    /// </summary>
    [JsonIgnore]
    public Guid SpecId => Node.NodeId;

    /// <summary>
    /// The name of the spec. This is the same as the name of the provided/configured node.
    /// </summary>
    [JsonIgnore]
    public string Name => Node.Name;

    /// <summary>
    /// Represents a configuration that defines the specification to run against a given source.
    /// A spec is basically a definition for how to get data from an L5X file and check the values
    /// for the returned data to verify its content. This is the primary means through which we set up
    /// and verify L5X content.
    /// </summary>
    [JsonIgnore]
    public Node Node { get; private init; } = Node.New(Guid.NewGuid(), "New Spec", NodeType.Spec);

    /*/// <summary>
    /// The target <see cref="Engine.Element"/> this spec represents. This is the Logix type that we are validating.
    /// </summary>
    [JsonConverter(typeof(SmartEnumNameConverter<Element, string>))]
    public Element Element { get; set; } = Element.Default;*/

    /// <summary>
    /// The settings used to specify which component item to search using the element lookup function instead of
    /// querying all elements in the entire file. This speeds up specs for tags significantly since we can use the internal
    /// L5X index. This would only be used for specs that are targeting some specific tag or component.
    /// </summary>
    [JsonInclude]
    public Query Query { get; private set; } = new();

    /// <summary>
    /// The collection of <see cref="Criterion"/> that define how to filter elements to return candidates for verification.
    /// </summary>
    public List<Criterion> Filters { get; init; } = [];

    /// <summary>
    /// The collection of <see cref="Criterion"/> that define the checks to perform for each candidate element.
    /// </summary>
    public List<Criterion> Verifications { get; init; } = [];

    /// <summary>
    /// Contains the configuration for verifying the candidate range of the specification. Range is an optional check
    /// that allow the user to specify a specific number of objects that should be returned from the query.
    /// </summary>
    [JsonInclude]
    public Range Range { get; private set; } = new();

    /// <summary>
    /// The <see cref="Inclusion"/> specifying how to evaluate the filters of the spec (All/Any).
    /// </summary>
    public Inclusion FilterInclusion { get; set; } = Inclusion.All;

    /// <summary>
    /// The <see cref="Inclusion"/> specifying how to evaluate the verifications of the spec (All/Any). 
    /// </summary>
    public Inclusion VerificationInclusion { get; set; } = Inclusion.All;

    /// <summary>
    /// A collection of <see cref="Variable"/> configured for this spec.
    /// </summary>
    /// <remarks>
    /// These are objects that are assigned to the argument value
    /// for the <see cref="Filters"/> and <see cref="Verifications"/> criterion for this spec. We need to be able
    /// to retrieve these in order to assign/refresh their values prior to running the spec.
    /// </remarks>
    [JsonIgnore]
    public IEnumerable<Variable> Variables => GetVariables();

    /// <summary>
    /// Deserializes the provided specification string into a Spec object.
    /// </summary>
    /// <param name="spec">The specification string to deserialize.</param>
    /// <returns>A Spec object representing the deserialized specification string.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided data cannot be deserialized into a valid specification.</exception>
    public static Spec? Deserialize(string spec) => JsonSerializer.Deserialize<Spec>(spec);

    /// <summary>
    /// Serializes the Spec object to JSON using custom converters.
    /// </summary>
    /// <returns>A JSON string representation of the Spec object.</returns>
    public string Serialize() => JsonSerializer.Serialize(this);

    /// <summary>
    /// Creates a orphaned node instnace that represents the spec node for this <see cref="Spec"/> object.
    /// </summary>
    /// <returns>A new <see cref="Node"/> object configured with the correct id, name, and type.</returns>
    public Node ToNode() => Node.New(SpecId, Name, NodeType.Spec);

    /// <summary>
    /// Runs the Spec on the given L5X content and returns the outcome.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="token"></param>
    /// <returns>The outcome of the Spec run.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the content parameter is null.</exception>
    public async Task<Outcome> RunAsync(Source source, CancellationToken token = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var verifications = await RunSpecAsync(source, token);
        stopwatch.Stop();

        return new Outcome(this, stopwatch.ElapsedMilliseconds, verifications);
    }

    /// <summary>
    /// Runs the Spec on the given L5X content and returns the outcome.
    /// </summary>
    /// <param name="sources"></param>
    /// <param name="token"></param>
    /// <returns>The outcome of the Spec run.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the content parameter is null.</exception>
    public async Task<Outcome> RunAllAsync(IEnumerable<Source> sources, CancellationToken token = default)
    {
        var verifications = new List<Verification>();
        var stopwatch = Stopwatch.StartNew();

        foreach (var source in sources)
        {
            token.ThrowIfCancellationRequested();
            var results = await RunSpecAsync(source, token);
            verifications.AddRange(results);
        }

        stopwatch.Stop();

        return new Outcome(this, stopwatch.ElapsedMilliseconds, verifications);
    }

    /// <summary>
    /// Runs the Spec on the given L5X content and returns the outcome.
    /// </summary>
    /// <param name="sources"></param>
    /// <returns>The outcome of the Spec run.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the content parameter is null.</exception>
    public Outcome RunAll(IEnumerable<Source> sources)
    {
        var verifications = new List<Verification>();
        var stopwatch = Stopwatch.StartNew();

        foreach (var source in sources)
        {
            var results = RunSpec(source);
            verifications.AddRange(results);
        }

        stopwatch.Stop();

        return new Outcome(this, stopwatch.ElapsedMilliseconds, verifications);
    }

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
    /// Configures this spec with the data from the provided spec object.
    /// </summary>
    /// <param name="config">The spec to apply to the this spec.</param>
    /// <returns>A <see cref="Spec"/> with the updated config.</returns>
    /// <remarks>This is mostly, so I can update the data returned from the database while maintaining the id.</remarks>
    public Spec Update(Spec? config)
    {
        if (config is null) return this;

        Query = config.Query;
        FilterInclusion = config.FilterInclusion;
        VerificationInclusion = config.VerificationInclusion;
        Filters.Clear();
        Filters.AddRange(config.Filters);
        Verifications.Clear();
        Verifications.AddRange(config.Verifications);
        Range = config.Range;

        return this;
    }

    /// <summary>
    /// Configures the <see cref="Query"/> for which this specification will find data.
    /// </summary>
    /// <param name="element">The <see cref="Engine.Element"/> option to query.</param>
    /// <param name="name">The optional name of the element to find to optimize the query and scope to a single component.</param>
    public Spec Search(Element element, string? name = default)
    {
        Query = new Query(element, name);
        return this;
    }

    /// <summary>
    /// Simply adds a new <see cref="Criterion"/> to the <see cref="Filters"/> collection with the specified arguments.
    /// </summary>
    /// <param name="property">The property name to select for the filter criterion.</param>
    /// <param name="operation">The <see cref="Operation"/> the criterion will perform.</param>
    /// <param name="args">The collection of <see cref="Argument"/> to supply to the criterion operation.</param>
    public Spec Where(Property? property, Operation operation, params Argument[] args)
    {
        Filters.Add(new Criterion(property, operation, args));
        return this;
    }

    /// <summary>
    /// Simply adds a new <see cref="Criterion"/> to the <see cref="Verifications"/> collection with the specified arguments.
    /// </summary>
    /// <param name="property">The property name to select for the filter criterion.</param>
    /// <param name="operation">The <see cref="Operation"/> the criterion will perform.</param>
    /// <param name="args">The collection of <see cref="Argument"/> to supply to the criterion operation.</param>
    public Spec ShouldHave(Property? property, Operation operation, params Argument[] args)
    {
        Verifications.Add(new Criterion(property, operation, args));
        return this;
    }

    /// <summary>
    /// Simply adds a new <see cref="Criterion"/> to the <see cref="Verifications"/> collection with the specified arguments.
    /// </summary>
    /// <param name="property">The property name to select for the filter criterion.</param>
    /// <param name="operation">The <see cref="Operation"/> the criterion will perform.</param>
    /// <param name="args">The collection of <see cref="Argument"/> to supply to the criterion operation.</param>
    public Spec ShouldNotHave(Property? property, Operation operation, params Argument[] args)
    {
        Verifications.Add(new Criterion(property, operation, args) { Invert = true });
        return this;
    }

    public Spec ShouldReturn(Operation operation, params Argument[] args)
    {
        Range.Enabled = true;
        Range.Criterion.Operation = operation;
        Range.Criterion.Arguments.Clear();
        Range.Criterion.Arguments.AddRange(args);
        return this;
    }

    /// <summary>
    /// Executes the configured specification against the provided L5X content.
    /// </summary>
    /// <param name="source">The L5X representing the content to verify.</param>
    /// <param name="token">A token for canceling the run.</param>
    /// <returns>A collection of <see cref="Verification"/> objects indicating the result data.</returns>
    private Task<ICollection<Verification>> RunSpecAsync(Source source, CancellationToken token = default)
    {
        return Task.Run(() => RunSpec(source), token);
    }

    /// <summary>
    /// Executes the configured specification against the provided source content.
    /// </summary>
    /// <param name="source">The source file representing the content to verify.</param>
    /// <returns>A collection of <see cref="Verification"/> objects indicating the result data.</returns>
    private ICollection<Verification> RunSpec(Source source)
    {
        ArgumentNullException.ThrowIfNull(source);

        try
        {
            var verifications = new List<Verification>();

            //1. Load source file.
            var content = source.Load();

            //2. Query/get data from the source file.
            var elements = Query.Execute(content);

            //3. Filter this data based on configured criteria.
            var filtered = elements.Where(Filter).ToList();

            //4. If configured, verify the resulting candidate range.
            if (Range.Enabled)
                verifications.Add(VerifyRange(filtered));

            //5. Process all configured verification criteria and return the results.
            verifications.AddRange(filtered.Select(Verify));

            return verifications;
        }
        catch (Exception e)
        {
            //If anything fails just return a single failed evaluation with the exception.
            var verification = Verification.For(Evaluation.Errored(e));
            return [verification];
        }
    }

    /// <summary>
    /// Given a target object, runs the configured <see cref="Filters"/> to evaluate whether this object should be
    /// considered a candidate for verification of this spec.
    /// </summary>
    /// <param name="target">An object for which to filter.</param>
    /// <returns><c>true</c> if this target object passes the specified criterion filters. The target can pass any or
    /// all filters as defined by the <see cref="FilterInclusion"/> of the spec.</returns>
    private bool Filter(LogixElement target)
    {
        var evaluations = Filters.Select(f => f.Evaluate(target)).ToList();

        return FilterInclusion == Inclusion.All
            ? evaluations.All(e => e.Result == ResultState.Passed)
            : evaluations.Any(e => e.Result == ResultState.Passed);
    }

    /// <summary>
    /// Given a candidate object, runs the configured <see cref="Verifications"/> to evaluate whether this object
    /// passes or fails the defined specification.
    /// </summary>
    /// <param name="candidate">An object for which to verify.</param>
    /// <returns>A <see cref="Verification"/> which is a grouped set of <see cref="Evaluation"/> results for a single
    /// candidate object.</returns>
    private Verification Verify(LogixElement candidate)
    {
        var evaluations = Verifications.Select(v => v.Evaluate(candidate)).ToArray();

        return VerificationInclusion == Inclusion.All
            ? Verification.All(evaluations)
            : Verification.Any(evaluations);
    }

    /// <summary>
    /// Given the filtered candidate collection, verify the configured range criterion as defined by the
    /// <see cref="Range"/> settings. 
    /// </summary>
    /// <param name="collection">The collection of candidate object that passed the filter step.</param>
    /// <returns>A <see cref="Verification"/> representing the result of the range criterion.</returns>
    private Verification VerifyRange(List<LogixElement> collection)
    {
        var evaluation = Range.Criterion.Evaluate(collection);
        return Verification.For(evaluation);
    }

    /// <summary>
    /// Gets all variables defined in the Filters and Verifications collections for this spec.
    /// </summary>
    private IEnumerable<Variable> GetVariables()
    {
        var variables = new List<Variable>();
        variables.AddRange(Filters.SelectMany(GetVariables));
        variables.AddRange(Verifications.SelectMany(GetVariables));
        return variables;
    }

    /// <summary>
    /// Gets all variables, including nested criterion variables, in the provided criterion. 
    /// </summary>
    private static IEnumerable<Variable> GetVariables(Criterion criterion)
    {
        var variables = new List<Variable>();

        foreach (var argument in criterion.Arguments)
        {
            switch (argument.Value)
            {
                case Criterion child:
                    variables.AddRange(GetVariables(child));
                    break;
                case Variable variable:
                    variables.Add(variable);
                    break;
            }
        }

        return variables;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="variables"></param>
    private void ResolveVariables(ICollection<Variable> variables)
    {
        foreach (var variable in variables)
        {
            foreach (var filter in Filters) Resolve(filter, variable);
            foreach (var verification in Verifications) Resolve(verification, variable);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="criterion"></param>
    /// <param name="variable"></param>
    private static void Resolve(Criterion criterion, Variable variable)
    {
        foreach (var argument in criterion.Arguments)
        {
            switch (argument.Value)
            {
                case Criterion child:
                    Resolve(child, variable);
                    break;
                case Variable current when current.Name == variable.Name:
                    argument.Value = variable;
                    break;
            }
        }
    }
}