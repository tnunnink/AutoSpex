using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine;

/// <summary>
/// The configuration that defines the specification to run against a given source. A spec is basically a definition
/// for how to get data from an L5X file and check the values of the returned data to verify its content. So this
/// is the primary means though which we set up and verify L5X content. A spec will query a specific element type,
/// filter those resulting element, and verify the candidate element all using the configured <see cref="Criterion"/>
/// objects. This object is persisted to the database to be reloaded and executed against source files as needed.
/// </summary>
public class Spec()
{
    //The internal list of steps that define the specification. Each step will process some input data and produce some
    //output data to be consumed by the next step. Internally, a spec should always end with a Verify step.
    //If none is configured, then we return the default result configured in the settings.
    
    //private readonly List<Step> _steps = [];

    /// <summary>
    /// Creates a new spec with the data from another spec.
    /// </summary>
    private Spec(Spec spec) : this()
    {
        Element = spec.Element;
        Steps = spec.Steps.ToList();
    }

    /// <summary>
    /// Creates a new spec with the provided element type.
    /// </summary>
    /// <param name="element">The <see cref="Element"/> type the spec represents.</param>
    public Spec(Element element) : this()
    {
        Element = element ?? throw new ArgumentNullException(nameof(element));
    }

    /// <summary>
    /// Creates a new <see cref="Spec"/> initialized with the provided element type and steps.
    /// </summary>
    [JsonConstructor]
    public Spec(Element element, List<Step> steps) : this()
    {
        Element = element ?? throw new ArgumentNullException(nameof(element));
        Steps = steps;
    }

    /// <summary>
    /// The version that identifies the JSON schema of the current object.
    /// </summary>
    [JsonInclude]
    private int SchemaVersion { get; } = 3;

    /// <summary>
    /// The unique id that indemnifies this spec apart from others.
    /// </summary>
    [JsonInclude]
    public Guid SpecId { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// The <see cref="Engine.Element"/> type to query when this query is executed.
    /// </summary>
    [JsonInclude]
    [JsonConverter(typeof(SmartEnumNameConverter<Element, string>))]
    public Element Element { get; set; } = Element.Default;

    /// <summary>
    /// The collection of <see cref="Step"/> that define how to process data for the spec.
    /// </summary>
    [JsonInclude]
    public List<Step> Steps { get; private init; } = [];

    /// <summary>
    /// 
    /// </summary>
    [JsonIgnore]
    public ResultState DefaultResult { get; set; } = ResultState.Failed;

    /// <summary>
    /// Gets the <see cref="Property"/> that identifies the type this spec returns.
    /// </summary>
    [JsonIgnore]
    public Property Returns => Steps.Aggregate(Element.This, (property, step) => step.Returns(property));

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

    public void AddStep(Step step)
    {
    }

    /// <summary>
    /// Configures the <see cref="Element"/> type to query when this specification is run.
    /// </summary>
    /// <param name="element">The <see cref="Engine.Element"/> option to query.</param>
    /// <returns>The configured spec instance.</returns>
    /// <remarks>
    /// This is exclusivity a method to help easily configure a simple spec object for testing purposes.
    /// The application will primarily add and configure steps.
    /// </remarks>
    public Spec Query(Element element)
    {
        Element = element ?? throw new ArgumentNullException(nameof(element));
        return this;
    }

    /// <summary>
    /// Adds a <see cref="Criterion"/> as a filter to this spec. If no filter step exists, it will be created.
    /// Otherwise, this criterion will be added to the first found filter step.
    /// </summary>
    /// <param name="property">The property name to select for the criterion.</param>
    /// <param name="operation">The <see cref="Operation"/> the criterion will perform.</param>
    /// <param name="argument">The argument to supply to the criterion operation.</param>
    /// <returns>The currently configured <see cref="Spec"/> instance.</returns>
    /// <remarks>
    /// This is exclusivity a method to help easily configure a simple spec object for testing purposes.
    /// The application will primarily add and configure steps manually.
    /// </remarks>
    public Spec Where(string property, Operation operation, object? argument = null)
    {
        if (Steps.All(s => s is not Filter))
            Steps.Add(new Filter());

        var filter = (Filter)Steps.First(x => x is Filter);
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
        Steps.Add(select);
        return this;
    }

    /// <summary>
    /// Adds a <see cref="Criterion"/> as verification to this spec. All specs are initialized with a single verify step,
    /// so this criterion will just be added to that step.
    /// This is exclusivity a method to help easily configure a simple spec object for testing purposes.
    /// The application will primarily add and configure steps manually.
    /// </summary>
    /// <param name="property">The property name to select for the criterion.</param>
    /// <param name="operation">The <see cref="Operation"/> the criterion will perform.</param>
    /// <param name="argument">The argument to supply to the criterion operation.</param>
    /// <returns>The current configured <see cref="Spec"/> instance.</returns>
    public Spec Verify(string property, Operation operation, object? argument = null)
    {
        if (Steps.All(s => s is not Engine.Verify))
            Steps.Add(new Verify());

        var step = (Verify)Steps.First(x => x is Verify);
        step.Criteria.Add(new Criterion(property, operation, argument));
        return this;
    }

    /// <summary>
    /// Adds a <see cref="Criterion"/> as verification to this spec. All specs are initialized with a single verified step,
    /// so this criterion will just be added to that step.
    /// This is exclusivity a method to help easily configure a simple spec object for testing purposes.
    /// The application will primarily add and configure steps manually.
    /// </summary>
    /// <param name="property">The property name to select for the criterion.</param>
    /// <param name="negation">The negation option to use for the criterion.</param>
    /// <param name="operation">The <see cref="Operation"/> the criterion will perform.</param>
    /// <param name="argument">The argument to supply to the criterion operation.</param>
    /// <returns>The current configured <see cref="Spec"/> instance.</returns>
    public Spec Verify(string property, Negation negation, Operation operation, object? argument = null)
    {
        if (Steps.All(s => s is not Engine.Verify))
            Steps.Add(new Verify());

        var step = (Verify)Steps.First(x => x is Verify);
        step.Criteria.Add(new Criterion(property, negation, operation, argument));
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
    /// Retrieves all <see cref="Criterion"/> found in the spec in both filter and verify steps.
    /// </summary>
    /// <returns>A flat collection of Criterion objects found in the spec.</returns>
    public IEnumerable<Criterion> GetAllCriteria()
    {
        var criteria = new List<Criterion>();

        foreach (var step in Steps)
        {
            switch (step)
            {
                case Filter filter:
                    criteria.AddRange(filter.Criteria);
                    break;
                case Verify verify:
                    criteria.AddRange(verify.Criteria);
                    break;
            }
        }

        return criteria;
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
    /// Determines the property to which the input will flow based on the provided step.
    /// </summary>
    /// <param name="step">The step used to determine the property that the input will flow to.</param>
    /// <returns>The property to which the input will flow based on the provided step.</returns>
    public Property GetInputTo(Step step)
    {
        var index = Steps.IndexOf(step);
        if (index == -1) return Property.Default;
        return index > 0 ? Steps[..index].Aggregate(Element.This, (p, s) => s.Returns(p)) : Element.This;
    }

    /// <summary>
    /// Runs the configured specification against the provided L5X content and returns a verification result.
    /// </summary>
    /// <param name="content">The L5X content to run this specification against.</param>
    /// <returns>The <see cref="Verification"/> containing the specification results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is null.</exception>
    public IEnumerable<Verification> Run(L5X content)
    {
        return ExecuteSpec(content);
    }

    /// <summary>
    /// Runs the configured spec against the provided L5X content and returns a verification result.
    /// </summary>
    /// <param name="content">The L5X content to run this specification against.</param>
    /// <param name="token">The optional cancellation token to stop the run.</param>
    /// <returns>The <see cref="Verification"/> containing the specification results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the content parameter is null.</exception>
    public Task<IEnumerable<Verification>> RunAsync(L5X content, CancellationToken token = default)
    {
        return Task.Run(() => ExecuteSpec(content), token);
    }

    /// <summary>
    /// Executes the Query and Processing steps on the provided L5X content.
    /// </summary>
    /// <param name="content">The L5X content to process.</param>
    /// <returns>The result of executing the specified Query and Processing steps on the content.</returns>
    private IEnumerable<Verification> ExecuteSpec(L5X content)
    {
        ArgumentNullException.ThrowIfNull(content);

        try
        {
            //Query all elements of the specified type.
            var elements = content.Query(Element.Type).Cast<object?>();

            //Run the resulting elements through all configured steps to process data.
            var results = Steps.Aggregate(elements, (data, step) => step.Process(data)).ToArray();

            //If we don't produce any results, then we resort to the default state.
            if (results.Length == 0)
            {
                return [new Verification(DefaultResult)];
            }


            //Returns the results of the processing.
            return results.Cast<Verification>();
        }
        catch (Exception e)
        {
            //If anything fails, just return a failed evaluation with the exception message.
            return [new Verification(null, [Evaluation.Errored(e)])];
        }
    }
}