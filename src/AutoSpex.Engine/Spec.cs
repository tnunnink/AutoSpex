using System.Diagnostics;
using System.Text.Json;
using AutoSpex.Engine.Converters;
using JetBrains.Annotations;
using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine;

[PublicAPI]
public class Spec()
{
    private Node? _node;

    public Spec(Node node) : this()
    {
        _node = node;
        SpecId = _node.NodeId;
    }

    /// <summary>
    /// A <see cref="Guid"/> that uniquely identifies this object.
    /// </summary>
    /// <remarks>
    /// This is actually the same as the owning node NodeId since any given spec should belong to a node.
    /// If this objects is created as a orphaned spec then this id will be <see cref="Guid.Empty"/>.
    /// </remarks>
    public Guid SpecId { get; private set; }

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
    public List<Criterion> Filters { get; } = [];

    /// <summary>
    /// The collection of <see cref="Criterion"/> that define the checks to perform for each candidate element.
    /// </summary>
    public List<Criterion> Verifications { get; } = [];
    
    /// <summary>
    /// A collection of <see cref="Variables"/> that are in scope of this spec. In other words, the variables that this
    /// Spec has access to.
    /// </summary>
    public IEnumerable<Variable> Variables => _node?.ScopedVariables() ?? Enumerable.Empty<Variable>();

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
    /// <param name="content">The L5X content to run the Spec on.</param>
    /// <returns>The outcome of the Spec run.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the content parameter is null.</exception>
    public Task<Outcome> Run(L5X content)
    {
        _node ??= Node.NewSpec("Spec");

        if (content is null)
            throw new ArgumentNullException(nameof(content));

        //Uses the attached node if available to resolve any variables in it's scope.
        ApplyVariables();

        return Task.Run(() =>
        {
            var verifications = new List<Verification>();

            //Time the process for analysis
            var stopwatch = Stopwatch.StartNew();

            //1.Query content
            var elements = Element.Query(content);

            //2.Filter content
            var filtered = elements.Where(Filter).ToList();

            //3.Evaluate count (if configured)
            if (Settings.VerifyCount)
                verifications.Add(Verify(filtered));

            //4.Verify candidates
            verifications.AddRange(filtered.Select(Verify));

            stopwatch.Stop();

            return new Outcome(_node, stopwatch.ElapsedMilliseconds, verifications);
        });
    }

    /// <summary>
    /// Configures the <see cref="Element"/> for which this specification will query.
    /// </summary>
    /// <param name="element">The <see cref="Engine.Element"/> option to query.</param>
    public void For(Element element)
    {
        Element = element ?? throw new ArgumentNullException(nameof(element));
    }

    /// <summary>
    /// Simply adds a new <see cref="Criterion"/> to the <see cref="Filters"/> collection with the specified arguments.
    /// </summary>
    /// <param name="property">The property name to select for the filter criterion.</param>
    /// <param name="operation">The <see cref="Operation"/> the criterion will perform.</param>
    /// <param name="args">The collection of <see cref="Argument"/> to supply to the criterion operation.</param>
    public void Where(string property, Operation operation, params Argument[] args)
    {
        Filters.Add(new Criterion(property, operation, args));
    }

    /// <summary>
    /// Simply adds a new <see cref="Criterion"/> to the <see cref="Verifications"/> collection with the specified arguments.
    /// </summary>
    /// <param name="property">The property name to select for the filter criterion.</param>
    /// <param name="operation">The <see cref="Operation"/> the criterion will perform.</param>
    /// <param name="args">The collection of <see cref="Argument"/> to supply to the criterion operation.</param>
    public void Verify(string property, Operation operation, params Argument[] args)
    {
        Verifications.Add(new Criterion(property, operation, args));
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
            ? Verification.All(candidate, evaluations)
            : Verification.Any(candidate, evaluations);
    }

    /// <summary>
    /// Given the filtered candidate collection, verify the configured range criterion stored in the spec
    /// <see cref="Settings"/> object. 
    /// </summary>
    /// <param name="candidates">The collection of candidate object that passed the filter step.</param>
    /// <returns>A <see cref="Verification"/> representing the result of the range criterion.</returns>
    private Verification Verify(ICollection<object> candidates)
    {
        var criterion = new Criterion(Settings.CountOperation, Settings.CountValue);
        var evaluation = criterion.Evaluate(candidates.Count);
        return Verification.For(candidates, evaluation);
    }

    /// <summary>
    /// Applies variable values to all filter and verification criterion of the provided spec.
    /// </summary>
    private void ApplyVariables()
    {
        Filters.ForEach(ApplyVariables);
        Verifications.ForEach(ApplyVariables);
    }

    /// <summary>
    /// Recursively applies all current variable values to the provided criterion.
    /// </summary>
    /// <remarks>
    /// This is basically how we resolve variables. A user can specify a variable as a criterion argument.
    /// The variable has the value to use as the actual argument value. This will get used when the argument calls
    /// Resolve, so we just need to make sure they are set to the correct value as they are serialized and persisted.
    /// </remarks>
    private void ApplyVariables(Criterion criterion)
    {
        foreach (var argument in criterion.Arguments)
        {
            switch (argument.Value)
            {
                case Criterion child:
                    ApplyVariables(child);
                    break;
                case Variable variable:
                {
                    var match = _node?.FindVariable(variable.Name);
                    if (match is null) break;
                    variable.Value = match.Value;
                    break;
                }
            }
        }
    }
}