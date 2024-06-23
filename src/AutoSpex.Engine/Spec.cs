using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
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
        SpecId = node.NodeId;
        Name = node.Name;
    }

    /// <summary>
    /// A <see cref="Guid"/> that uniquely identifies this object.
    /// </summary>
    /// <remarks>
    /// This is actually the same as the owning node's NodeId since any given spec should belong to a node.
    /// If this objects is created as an orphaned spec then this id will be <see cref="Guid.Empty"/>.
    /// </remarks>
    public Guid SpecId { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// The name of the spec. 
    /// </summary>
    /// <remarks>
    /// This is actually the same as the owning node's Name since any given spec should belong to a node.
    /// If this objects is created as an orphaned spec then this id will be <see cref="String.Empty"/>.
    /// </remarks>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// The target <see cref="Engine.Element"/> this spec represents. This is the Logix type that we are validating.
    /// </summary>
    public Element Element { get; set; } = Element.Default;

    /// <summary>
    /// The <see cref="SpecSettings"/> defining how the spec should evaluate pass/failure.
    /// </summary>
    public SpecSettings Settings { get; set; } = SpecSettings.Default;

    /// <summary>
    /// The collection of <see cref="Criterion"/> that define how to filter elements to return candidates for verification.
    /// </summary>
    public List<Criterion> Filters { get; private set; } = [];

    /// <summary>
    /// The collection of <see cref="Criterion"/> that define the checks to perform for each candidate element.
    /// </summary>
    public List<Criterion> Verifications { get; private set; } = [];

    /// <summary>
    /// A collection of <see cref="Variable"/> configured for this spec.
    /// </summary>
    /// <remarks>
    /// These are objects that are assigned to the argument value
    /// for the <see cref="Filters"/> and <see cref="Verifications"/> criterion for this spec. We need to be able
    /// to retrieve these in order to assign/refresh their values prior to running the spec.
    /// </remarks>
    public IEnumerable<Variable> Variables => GetVariables();

    /// <summary>
    /// Deserializes the provided specification string into a Spec object.
    /// </summary>
    /// <param name="spec">The specification string to deserialize.</param>
    /// <returns>A Spec object representing the deserialized specification string.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided data cannot be deserialized into a valid specification.</exception>
    public static Spec Deserialize(string spec)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonSpecConverter());
        options.Converters.Add(new JsonCriterionConverter());
        options.Converters.Add(new JsonArgumentConverter());
        options.Converters.Add(new JsonVariableConverter());
        options.Converters.Add(new JsonTypeConverter());
        return JsonSerializer.Deserialize<Spec>(spec, options)
               ?? throw new ArgumentException("Not able to deserialize provided data into a valid specification");
    }

    /// <summary>
    /// Serializes the Spec object to JSON using custom converters.
    /// </summary>
    /// <returns>A JSON string representation of the Spec object.</returns>
    public string Serialize()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonSpecConverter());
        options.Converters.Add(new JsonCriterionConverter());
        options.Converters.Add(new JsonArgumentConverter());
        options.Converters.Add(new JsonVariableConverter());
        options.Converters.Add(new JsonTypeConverter());
        return JsonSerializer.Serialize(this, options);
    }

    /// <summary>
    /// Runs the Spec on the given L5X content and returns the outcome.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="token"></param>
    /// <returns>The outcome of the Spec run.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the content parameter is null.</exception>
    public async Task<Outcome> Run(Source source, CancellationToken token = default)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        var content = source.L5X;

        var stopwatch = Stopwatch.StartNew();
        var verifications = await RunSpec(content, token);
        stopwatch.Stop();

        return new Outcome(this, source, verifications, stopwatch.ElapsedMilliseconds);
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
    public Spec Update(Spec config)
    {
        ArgumentNullException.ThrowIfNull(config);
        Element = config.Element;
        Settings = config.Settings;
        Filters = [..config.Filters];
        Verifications = [..config.Verifications];
        return this;
    }

    /// <summary>
    /// Configures the <see cref="Element"/> for which this specification will query.
    /// </summary>
    /// <param name="element">The <see cref="Engine.Element"/> option to query.</param>
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
    public Spec Verify(Property? property, Operation operation, params Argument[] args)
    {
        Verifications.Add(new Criterion(property, operation, args));
        return this;
    }

    /// <summary>
    /// Determines if the provided criterion is one contained somewhere within this spec, either in filters,
    /// verifications, or nested within as a child criterion. 
    /// </summary>
    /// <param name="criterion">The criterion to search for in the object graph.</param>
    /// <returns><c>true</c> if this spec contains the provided criterion. Otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This allows us to determine if this spec "owns" a criterion which is important from the interface and
    /// data retrieval perspective.
    /// </remarks>
    public bool Contains(Criterion criterion)
    {
        return Filters.Any(c => c.CriterionId == criterion.CriterionId || c.Contains(criterion)) ||
               Verifications.Any(c => c.CriterionId == criterion.CriterionId || c.Contains(criterion));
    }

    /// <summary>
    /// Executes the configured specification against the provided L5X content.
    /// </summary>
    /// <param name="content">The L5X representing the content to verify.</param>
    /// <param name="token">A token for canceling the run.</param>
    /// <returns>A collection of <see cref="Verification"/> objects indicating the result data.</returns>
    private Task<ReadOnlyCollection<Verification>> RunSpec(L5X content, CancellationToken token = default)
    {
        return Task.Run(() =>
        {
            var verifications = new List<Verification>();

            //1.Query content
            var elements = Element.Query(content);

            token.ThrowIfCancellationRequested();

            //2.Filter content
            var filtered = elements.Where(Filter).ToList();

            token.ThrowIfCancellationRequested();

            //3.Evaluate count (if configured)
            if (Settings.VerifyCount)
                verifications.Add(VerifyCount(filtered));

            token.ThrowIfCancellationRequested();

            //4.Verify candidates
            verifications.AddRange(filtered.Select(Verify));

            return verifications.AsReadOnly();
        }, token);
    }

    /// <summary>
    /// Given a target object, runs the configured <see cref="Filters"/> to evaluate whether this object should be
    /// considered a candidate for verification of this spec.
    /// </summary>
    /// <param name="target">An object for which to filter.</param>
    /// <returns><c>true</c> if this target object passes the specified criterion filters. The target can pass any or
    /// all filters as defined by the <see cref="Settings"/> of the spec.</returns>
    private bool Filter(object target)
    {
        var evaluations = Filters.Select(f => f.Evaluate(target)).ToList();

        return Settings.FilterInclusion == Inclusion.All
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
    private Verification Verify(object? candidate)
    {
        var evaluations = Verifications.Select(v => v.Evaluate(candidate)).ToArray();

        return Settings.VerificationInclusion == Inclusion.All
            ? Verification.All(evaluations)
            : Verification.Any(evaluations);
    }

    /// <summary>
    /// Given the filtered candidate collection, verify the configured range criterion stored in the spec
    /// <see cref="Settings"/> object. 
    /// </summary>
    /// <param name="collection">The collection of candidate object that passed the filter step.</param>
    /// <returns>A <see cref="Verification"/> representing the result of the range criterion.</returns>
    private Verification VerifyCount(ICollection<LogixElement> collection)
    {
        var property = Property.This(typeof(ICollection<LogixElement>)).GetProperty("Count");
        var criterion = new Criterion(property, Settings.CountOperation, Settings.CountValue);
        var evaluation = criterion.Evaluate(collection);
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
}