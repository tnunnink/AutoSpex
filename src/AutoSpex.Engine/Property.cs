using System.Linq.Expressions;

namespace AutoSpex.Engine;

/// <summary>
/// A representation of a class property which can be navigated to from our logix or build in .NET types. This class
/// contains all the functionality we need for our object graph navigation as well as type information and getter functions
/// which will be used in criteria for getting the values to evaluate filters and verifications.
/// </summary>
public class Property : IEquatable<Property>
{
    private const char Separator = '.';

    /// <summary>
    /// Holds compiled property getter functions so we don't have to recreate them each time we need to get a property.
    /// This will improve the overall performance when we go to run many criterion for many specifications.
    /// These are cached as the are accessed. We can't be greedy and create them ahead of time because of the recursive nature
    /// of the type graph and these being static types, we could cause overflow exceptions. 
    /// </summary>
    private static readonly Dictionary<Property, Func<object?, object?>> Cache = new();

    /// <summary>
    /// Creates a new <see cref="Property"/> given the originating type, path, return type, and optional custom getter.
    /// </summary>
    /// <param name="origin">The type from which this property originates.</param>
    /// <param name="path">The path name to this immediate or nested child property.</param>
    /// <param name="type">The return type of the property.</param>
    /// <param name="getter">An optional custom getter that tells us how to get the value for the property given and
    /// instance of the origin object.</param>
    /// <exception cref="ArgumentException"><paramref name="path"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="origin"/> or <paramref name="type"/> is null.</exception>
    public Property(Type origin, string path, Type type, Func<object?, object?>? getter = default)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Name can not be null or empty to initialize a property.");

        Origin = origin ?? throw new ArgumentNullException(nameof(origin));
        Path = path;
        Type = type ?? throw new ArgumentNullException(nameof(type));

        if (getter is not null)
        {
            Cache.TryAdd(this, getter);
        }
    }

    /// <summary>
    /// The root or originating type to which this property belongs.
    /// </summary>
    public Type Origin { get; }

    /// <summary>
    /// The full dot down path to this property from the origin type.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// The return type of this property.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// The member name of this property.
    /// </summary>
    public string Name => Path[(Path.LastIndexOf(Separator) + 1)..];

    /// <summary>
    /// A friendly type identifier for the property.
    /// </summary>
    public string Identifier => Type.TypeIdentifier();

    /// <summary>
    /// The <see cref="TypeGroup"/> to which this property's return type belongs.
    /// </summary>
    public TypeGroup Group => TypeGroup.FromType(Type);
    
    /// <summary>
    /// The set of object values that represent the options (enum/bool values) for the property type.
    /// </summary>
    public IEnumerable<object> Options => Type.GetOptions();
    
    /// <summary>
    /// The set of child properties which can be navigated to from this property's return type.
    /// </summary>
    public IEnumerable<Property> Properties => Type.Properties(this);

    /// <summary>
    /// Finds a child property of this property given the property name.
    /// </summary>
    /// <param name="path">The path to the property.</param>
    /// <returns>A property representing the provided property path if found, otherwise null.</returns>
    /// <remarks>This simply forwards the call to the property return type Properties extension method.</remarks>
    public Property? FindProperty(string path) => Type.Property(path);

    /// <summary>
    /// Returns the value getter for the property which can be used to retrieve the property value from an instance of the
    /// origin type.
    /// </summary>
    /// <returns>A <see cref="Func{TResult}"/> taking an instance object and returning this property value.</returns>
    public Func<object?, object?> Getter() => Getter(Origin, Path);

    public override bool Equals(object? obj) => obj is Property other && other.Origin == Origin && other.Path == Path;
    public override int GetHashCode() => HashCode.Combine(Origin, Path);
    public override string ToString() => Path;

    public bool Equals(Property? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Origin == other.Origin && Path == other.Path;
    }

    /// <summary>
    /// Creates the getter expression for this property and caches the result for the next call.
    /// This will help improve performance as we run many evaluations (getters) for many specs.
    /// </summary>
    private Func<object?, object?> Getter(Type origin, string path)
    {
        if (Cache.TryGetValue(this, out var cached)) return cached;

        var parameter = Expression.Parameter(typeof(object), "x");
        var converted = Expression.TypeAs(parameter, origin);
        var member = GetMember(converted, path);

        var getter = Expression.Lambda<Func<object?, object?>>(member, parameter).Compile();

        Cache.Add(this, getter);
        return getter;
    }

    /// <summary>
    /// Recursively gets the member expression for the provided property name. This will also add null checks for
    /// each nested member to prevent null reference exceptions, and to allow null to be propagated through the
    /// member expression and returns to the operation's evaluation method.
    /// </summary>
    /// <param name="parameter">The current member access expression for the type.</param>
    /// <param name="path">The current property name to create member access to.</param>
    /// <returns>An <see cref="Expression{TDelegate}"/> that represents member access to a immediate or nested/complex
    /// member property or field, with corresponding conditional null checks for each member level.</returns>
    private static Expression GetMember(Expression parameter, string path)
    {
        if (!path.Contains(Separator))
            return Expression.TypeAs(Expression.PropertyOrField(parameter, path), typeof(object));

        var index = path.IndexOf(Separator);
        var member = Expression.PropertyOrField(parameter, path[..index]);
        var notNull = Expression.NotEqual(member, Expression.Constant(null));
        return Expression.Condition(notNull, GetMember(member, path[(index + 1)..]), Expression.Constant(null),
            typeof(object));
    }
}