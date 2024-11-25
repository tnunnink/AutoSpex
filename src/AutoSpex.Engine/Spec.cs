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
    /// The internal collection intermediate of steps that define how to execute this spec.
    /// </summary>
    private readonly List<Step> _steps = [new Query(Element.Default), new Verify()];

    /// <summary>
    /// The constructor used by internal methods and the JsonSerializer to instantiate instance from serialized configs.
    /// </summary>
    /// <param name="schemaVersion">The current schema version that defines the spec.</param>
    /// <param name="specId">The unique spec id identifying this spec relative to others.</param>
    /// <param name="steps">The collection of steps that define the spec.</param>
    [JsonConstructor]
    private Spec(int schemaVersion, Guid specId, IEnumerable<Step> steps) : this()
    {
        SchemaVersion = schemaVersion;
        SpecId = specId;
        _steps = steps.ToList();
    }

    /// <summary>
    /// Creates a new spec with the provided element type.
    /// </summary>
    /// <param name="element">The <see cref="Element"/> type the spec represents.</param>
    public Spec(Element element) : this()
    {
        _steps = [new Query(element), new Verify()];
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
    /// The collection of <see cref="Step"/> that define the spec.
    /// </summary>
    /// <remarks>
    /// Each step is run in sequence to produce the result. All specs are by default initialized with a <see cref="Query"/>
    /// and <see cref="Verify"/> step which can't be removed. All specs at least need to run these steps to get data
    /// from and L5X and verify it based on some criteria. The user can also add intermediate <see cref="Filter"/> and
    /// <see cref="Select"/> steps to find specific candidates and proeprties for verifcation.
    /// </remarks>
    [JsonInclude]
    public IEnumerable<Step> Steps => _steps;

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
    /// Adds the provided <see cref="Step"/> to the list of <see cref="Steps"/> to process when this spec is run.
    /// </summary>
    /// <param name="step">The <see cref="Step"/> to add to this spec.</param>
    /// <remarks>
    /// Since the Query and Verify steps are preconfigured, the user is only allowed to add Filter and Select steps.
    /// These steps will be inserted before the verify step.
    /// </remarks>
    public void AddStep(Step step)
    {
        if (step is Query or Verify)
            throw new InvalidOperationException(
                "Can only add Filter or Select steps to a spec. Query and Verify are predefined.");

        //Always add the new filter or select step as the second to last step (before verify).
        _steps.Insert(_steps.Count - 1, step);
    }

    /// <summary>
    /// Removes the specified step from the list of steps in the spec.
    /// </summary>
    /// <param name="step">The step to be removed from the spec.</param>
    /// <exception cref="InvalidOperationException">Thrown when trying to remove a required Query or Verify step.</exception>
    public void RemoveStep(Step step)
    {
        if (step is Query or Verify)
            throw new InvalidOperationException("Can not remove the required query or verify step from a spec.");

        _steps.Remove(step);
    }

    /// <summary>
    /// Get the input property for the specified step.
    /// </summary>
    /// <param name="step">The step to get the input for.</param>
    /// <returns>The <see cref="Property"/> representing the input for the specified step.</returns>
    public Property InputTo(Step step)
    {
        if (!_steps.Contains(step))
            throw new ArgumentException("This spec does not contain the provided step.");

        var previous = _steps[.._steps.IndexOf(step)];

        return previous.Aggregate(Property.Default, (property, item) => item switch
        {
            Query query => query.Element.This,
            Select select => select.Property,
            _ => property
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="criterion"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Property InputTo(Criterion criterion)
    {
        var step = _steps.SingleOrDefault(s => s.Contains(criterion));

        if (step is null)
            throw new ArgumentException("This spec does not contain the provided criterion.");

        var previous = _steps[.._steps.IndexOf(step)];

        return previous.Aggregate(Property.Default, (property, item) => item switch
        {
            Query query => query.Element.This,
            Select select => select.Property,
            _ => property
        });
    }

    /// <summary>
    /// Configures the <see cref="Element"/> type to query when this specification is run.
    /// This is exlusivley a method to help easily configure a simple spec object for testing purposes.
    /// The application will primarily add and configure steps manually.
    /// </summary>
    /// <param name="element">The <see cref="Engine.Element"/> option to query.</param>
    /// <returns>The configured spec instance.</returns>
    public Spec Fetch(Element element)
    {
        if (_steps.FirstOrDefault() is not Query query)
            throw new InvalidOperationException("Spec does not contain a query step.");

        query.Element = element;
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
        if (_steps.All(s => s is not Filter))
            AddStep(new Filter());

        var filter = (Filter)_steps.First(x => x is Filter);
        filter.Add(new Criterion(property, operation, argument));
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    public Spec Choose(string property)
    {
        if (_steps.All(s => s is not Select))
            AddStep(new Select());

        var select = (Select)_steps.First(x => x is Select);
        var input = InputTo(select);
        select.Property = input.GetProperty(property);

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
    public Spec Confirm(string property, Operation operation, object? argument = default)
    {
        if (_steps.LastOrDefault() is not Verify verify)
            throw new InvalidOperationException("Spec does not contain a verify step.");

        verify.Add(new Criterion(property, operation, argument));
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
    public Spec Confirm(string property, Negation negation, Operation operation, object? argument = default)
    {
        if (_steps.LastOrDefault() is not Verify verify)
            throw new InvalidOperationException("Spec does not contain a verify step.");

        verify.Add(new Criterion(property, operation, argument) { Negation = negation });
        return this;
    }

    /// <summary>
    /// Creates a new <see cref="Spec"/> with the same configuration but different <see cref="SpecId"/>.
    /// </summary>
    /// <returns>A new <see cref="Spec"/> object.</returns>
    public Spec Duplicate()
    {
        var data = JsonSerializer.Serialize(this);
        var spec = JsonSerializer.Deserialize<Spec>(data);

        if (spec is null)
            throw new ArgumentException("Could not materialize new spec instance.");

        return new Spec(spec.SchemaVersion, Guid.NewGuid(), spec.Steps);
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
        return _steps.Any(s => s.Contains(criterion));
    }

    /// <summary>
    /// Returns a collection of all <see cref="Criterion"/> for any configured step that defined the spec.
    /// </summary>
    /// <returns>A collection of <see cref="Criterion"/> objects representing the criteria for the spec.</returns>
    public IEnumerable<Criterion> Criteria() => _steps.SelectMany(s => s.Criteria);

    /// <summary>
    /// Runs the configured specification against the provided L5X content and returns a verification result.
    /// </summary>
    /// <param name="content">The L5X content to run this specification against.</param>
    /// <returns>The <see cref="Verification"/> containing the specification results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is null.</exception>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="step"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public IEnumerable<object> RunTo(Step step, L5X content)
    {
        return RunSpecTo(step, content);
    }

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
            var data = new List<object> { content }.AsEnumerable();

            var stopwatch = Stopwatch.StartNew();
            var evaluations = _steps.Aggregate(data, (input, step) => step.Process(input)).Cast<Evaluation>().ToList();
            stopwatch.Stop();

            return Verification.For(evaluations, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception e)
        {
            //If anything fails just return a single failed verification with the exception message.
            return Verification.For(Evaluation.Errored(e));
        }
    }

    /// <summary>
    /// Runs all steps of the spec excluding the last verify step. This essentially gets all the objects that will be
    /// returned by the configured spec.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private IEnumerable<object> RunSpecTo(Step target, L5X content)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(target);

        if (!_steps.Contains(target))
            throw new ArgumentException("This spec does not contain the provided step.");

        var data = new List<object> { content }.AsEnumerable();
        var index = _steps.IndexOf(target) + 1;
        var steps = _steps[..index];

        try
        {
            return steps.Aggregate(data, (input, step) => step.Process(input));
        }
        catch (Exception)
        {
            return [];
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