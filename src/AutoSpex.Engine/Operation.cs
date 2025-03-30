using Ardalis.SmartEnum;

namespace AutoSpex.Engine;

/// <summary>
/// Represents an operation that can be performed on input values. Operations are 
/// important components of filtering and querying data because they define the criteria 
/// for what data should be fetched.
/// </summary>
public abstract class Operation(string name, string value) : SmartEnum<Operation, string>(name, value)
{
    protected Operation(string name) : this(name, name.Replace(" ", string.Empty))
    {
    }

    /// <summary>
    /// Performs the operation on the input and provided value and returns the result.
    /// The actual implementation of the operation (how it's evaluated) is defined
    /// in derived operation classes.
    /// </summary>
    /// <param name="input">The value that will be compared/evaluated by the operation.</param>
    /// <param name="value">Additional argument needed for some operations.</param>
    /// <returns>The result of the predicate operation. This is a boolean
    /// value indicating whether the input value meets the criteria defined by the operation.</returns>
    /// <remarks>
    /// As an abstract method, this must be implemented by each specific Operation subclass.
    /// Be aware that this function may throw exceptions if input data is not of the expected 
    /// type or proper format for the executing operation.
    /// </remarks>
    public abstract bool Execute(object? input, object? value = null);

    /// <summary>
    /// Returns a collection of operations that support the specified property group.
    /// </summary>
    /// <param name="property">The property for which supporting operations are to be retrieved.</param>
    /// <returns>A collection of operations that support the specified property group.</returns>
    public static IEnumerable<Operation> Supporting(Property property) => List.Where(x => x.Supports(property.Group));

    /// <summary>
    /// Determines whether the given property is supported by the current operation type.
    /// </summary>
    /// <param name="property">The property to check.</param>
    /// <returns>True if the Operation supports the Property; otherwise, false.</returns>
    public bool Supports(Property property) => Supports(property.Group);

    /// <summary>
    /// Determines whether the given TypeGroup is supported by the Operation.
    /// </summary>
    /// <param name="group">The TypeGroup to check.</param>
    /// <returns>True if the Operation supports the TypeGroup; otherwise, false.</returns>
    protected virtual bool Supports(TypeGroup group) => true;

    /// <summary>
    /// Represents an operation that signifies no specific operation.
    /// Used as a placeholder or default value.
    /// Executing this operation always returns false.
    /// </summary>
    public static readonly Operation None = new NoneOperation();

    /// <summary>
    /// Represents an equality operation. The operation checks if the input value 
    /// is equal to the comparison value. Comparison is done based on the underlying
    /// type of the input value.
    /// </summary>
    public static readonly Operation EqualTo = new EqualToOperation();

    /// <summary>
    /// Represents a greater than operation. The operation checks if the input value 
    /// is greater than the comparison value. This operation is only valid for 
    /// input types that implement IComparable interface.
    /// </summary>
    public static readonly Operation GreaterThan = new GreaterThanOperation();

    /// <summary>
    /// Represents an operation that checks whether a value is greater than or equal to another value.
    /// This operation is only valid for input types that implement IComparable interface.
    /// </summary>
    public static readonly Operation GreaterThanOrEqualTo = new GreaterThanOrEqualToOperation();

    /// <summary>
    /// Represents the less than comparison operation. The operation checks if the input value 
    /// is less than the comparison value. This operation is only valid for 
    /// input types that implement IComparable interface.
    /// </summary>
    public static readonly Operation LessThan = new LessThanOperation();

    /// <summary>
    /// Represents the less than or equal to operation. The operation checks if the input value 
    /// is less than or equal to the comparison value. This operation is only valid for 
    /// input types that implement IComparable interface.
    /// </summary>
    public static readonly Operation LessThanOrEqualTo = new LessThanOrEqualToOperation();

    /// <summary>
    /// Represents an operation that checks for null values. It returns true 
    /// if the input value is null and false otherwise.
    /// </summary>
    public static readonly Operation Null = new NullOperation();

    /// <summary>
    /// The Empty operation represents a condition that checks if a value or a collection of values is empty.
    /// </summary>
    public static readonly Operation Empty = new EmptyOperation();

    /// <summary>
    /// Represents an operation that determines whether a string is null, empty, or consists only of white-space characters.
    /// This operation is typically used for input validation or cleansing.
    /// </summary>
    public static readonly Operation Void = new VoidOperation();

    /// <summary>
    /// Represents an operation that checks whether the input string contains a specific substring.
    /// </summary>
    public static readonly Operation Containing = new ContainingOperation();

    /// <summary>
    /// The StartsWith operation is used to check if a string starts with a specified value.
    /// </summary>
    public static readonly Operation StartingWith = new StartingWithOperation();

    /// <summary>
    /// Represents an operation that checks if a string ends with a specific value.
    /// </summary>
    public static readonly Operation EndingWith = new EndingWithOperation();

    /// <summary>
    /// Represents an "in" operation. The operation checks whether the input value
    /// matches any value within a supplied list of values.
    /// </summary>
    public static readonly Operation In = new InOperation();

    /// <summary>
    /// Represents a like operation. The operation checks if the input value
    /// is similar to the comparison value based on a pattern matching algorithm.
    /// </summary>
    public static readonly Operation Like = new LikeOperation();

    /// <summary>
    /// Represents an operation that checks for a match between the input value and a pattern or set of values.
    /// </summary>
    public static readonly Operation Match = new MatchOperation();

    /// <summary>
    /// This operation checks if the input value falls between two specified bounds
    /// </summary>
    public static readonly Operation Between = new BetweenOperation();

    /// <summary>
    /// Represents an operation that checks if all elements of a collection meet a specific condition.
    /// </summary>
    public static readonly Operation All = new AllOperation();

    /// <summary>
    /// Represents an operation that checks if any elements of a collection meet a specific condition.
    /// </summary>
    public static readonly Operation Any = new AnyOperation();
}