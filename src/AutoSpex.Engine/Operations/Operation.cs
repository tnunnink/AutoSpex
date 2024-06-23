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
    /// Performs the operation on the input and provided values and returns the result.
    /// The actual implementation of the operation (how it's evaluated) is defined
    /// in derived operation classes.
    /// </summary>
    /// <param name="input">The value that will be compared/evaluated by the operation.</param>
    /// <param name="values">Additional parameters needed for some operations.</param>
    /// <returns>The result of the predicate operation. This is typically a boolean
    /// value indicating whether the input value meets the criteria defined by the operation.</returns>
    /// <remarks>
    /// As an abstract method, this must be implemented by each specific Operation subclass.
    /// Be aware that this function may throw exceptions if input data is not of the expected 
    /// type or proper format for the executing operation.
    /// </remarks>
    public abstract bool Execute(object? input, params object[] values);

    public static IEnumerable<Operation> Supporting(Property property) => List.Where(x => x.Supports(property.Group));

    public static IEnumerable<Operation> Supporting(TypeGroup group) => List.Where(x => x.Supports(group));

    protected virtual bool Supports(TypeGroup group) => true;

    /// <summary>
    /// Represents an equality operation. The operation checks if the input value 
    /// is equal to the comparison value. Comparison is done based on the underlying
    /// type of the input value.
    /// </summary>
    public static readonly Operation None = new NoneOperation();

    /// <summary>
    /// Represents an equality operation. The operation checks if the input value 
    /// is equal to the comparison value. Comparison is done based on the underlying
    /// type of the input value.
    /// </summary>
    public static readonly Operation Equal = new EqualOperation();

    /// <summary>
    /// Represents an operation that checks whether two values are equivalent.
    /// </summary>
    public static readonly Operation IsEquivalent = new IsEquivalentOperation();

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
    public static readonly Operation GreaterThanOrEqual = new GreaterThanOrEqualOperation();

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
    public static readonly Operation LessThanOrEqual = new LessThanOrEqualOperation();

    /// <summary>
    /// Returns the an <see cref="Operation"/> which evaluates whether an input value is <c>true</c>. 
    /// </summary>
    public static readonly Operation IsTrue = new IsTrueOperation();

    /// <summary>
    /// Returns the an <see cref="Operation"/> which evaluates whether an input value is <c>false</c>. 
    /// </summary>
    public static readonly Operation IsFalse = new IsFalseOperation();

    /// <summary>
    /// Represents an operation that checks for null values. It returns true 
    /// if the input value is null and false otherwise.
    /// </summary>
    public static readonly Operation IsNull = new IsNullOperation();

    /// <summary>
    /// The IsEmpty operation represents a condition that checks if a value or a collection of values is empty.
    /// </summary>
    public static readonly Operation IsEmpty = new IsEmptyOperation();

    /// <summary>
    /// Represents an operation that checks if a string is null or empty.
    /// </summary>
    public static readonly Operation IsNullOrEmpty = new IsNullOrEmptyOperation();

    /// <summary>
    /// Represents an operation that determines whether a string is null, empty, or consists only of white-space characters.
    /// This operation is typically used for input validation or cleansing.
    /// </summary>
    public static readonly Operation IsNullOrWhiteSpace = new IsNullOrWhiteSpaceOperation();

    /// <summary>
    /// Represents an operation that checks for a match between the input value and a pattern or set of values.
    /// </summary>
    public static readonly Operation IsMatch = new IsMatchOperation();

    /// <summary>
    /// Represents an operation that checks whether the input string contains a specific substring.
    /// </summary>
    public static readonly Operation Contains = new ContainsOperation();

    /// <summary>
    /// The StartsWith operation is used to check if a string starts with a specified value.
    /// </summary>
    public static readonly Operation StartsWith = new StartsWithOperation();

    /// <summary>
    /// Represents an operation that checks if a string ends with a specific value.
    /// </summary>
    public static readonly Operation EndsWith = new EndsWithOperation();

    /// <summary>
    /// Represents an "in" operation. The operation checks whether the input value 
    /// matches any value within a supplied list of values.
    /// </summary>
    public static readonly Operation In = new InOperation();


    public static readonly Operation Like = new LikeOperation();

    /// <summary>
    /// Represents a "between" operation. This operation checks if the input value falls between 
    /// two specified bounds (inclusive or exclusive based on the operation's implementation).
    /// </summary>
    public static readonly Operation Between = new BetweenOperation();


    public static readonly Operation All = new AllOperation();

    public static readonly Operation Any = new AnyOperation();

    public static readonly Operation Count = new CountOperation();
}