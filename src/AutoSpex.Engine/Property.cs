using System.Linq.Expressions;
using L5Sharp.Core;

namespace AutoSpex.Engine;

public class Property : IEquatable<Property>
{
    private const char Separator = '.';

    /// <summary>
    /// Holds compiled property getter functions so we don't have to recreate them each time we need to get a property.
    /// This will improve the overall performance when we go to run many criterion for many specifications.
    /// These are cached as the are accessed. We can't be greedy and create them ahead of time because of the recursive nature
    /// of the type structures and these being static type, we could cause overflow exceptions. 
    /// </summary>
    private static readonly Dictionary<Property, Func<object?, object?>> Cache = new();

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

    public Type Origin { get; }
    public string Path { get; }
    public Type Type { get; }
    public string Name => Path[(Path.LastIndexOf(Separator) + 1)..];
    public string Identifier => Type.TypeIdentifier();
    public TypeGroup Group => TypeGroup.FromType(Type);
    public IEnumerable<object> Options => Type.GetOptions();
    public IEnumerable<Property> Properties => GetProperties(Type);

    public Func<object?, object?> Getter() => Getter(Origin, Path);

    public override bool Equals(object? obj) => obj is Property other && other.Origin == Origin && other.Path == Path;

    public override int GetHashCode() => HashCode.Combine(Origin, Path);

    public override string ToString() => $"[{Identifier}] {Origin.Name}.{Path}";
    
    public bool Equals(Property? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Origin == other.Origin && Path == other.Path;
    }

    #region Internals

    private IEnumerable<Property> GetProperties(Type type)
    {
        if (!typeof(LogixElement).IsAssignableFrom(type))
        {
            return Type.Properties(this);
        }

        var element = Element.FromName(Type.Name);
        return element.Properties;
    }

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

    #endregion
}