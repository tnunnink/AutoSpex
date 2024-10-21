using System.Linq.Expressions;
using System.Reflection;

namespace AutoSpex.Engine;

/// <summary>
/// A representation of a class property which can be navigated to from our logix or built in .NET types. This class
/// contains all the functionality we need for our object graph navigation as well as type information and getter functions
/// which will be used in criteria for getting the values to evaluate filters and verifications.
/// </summary>
public class Property : IEquatable<Property>
{
    private const char MemberSeparator = '.';
    private const char IndexOpenSeparator = '[';
    private const char IndexCloseSeparator = ']';
    private static readonly char[] Separators = [MemberSeparator, IndexOpenSeparator];

    //These are properties that I don't want to show up for the user because they are not really useful and are confusing.
    private static readonly List<string> PropertyExclusions = ["L5X", "L5XType", "Length"];

    /// <summary>
    /// 
    /// </summary>
    private static readonly Lazy<Dictionary<Type, List<Property>>> PropertyCache = new();

    /// <summary>
    /// Holds compiled property getter functions, so we don't have to recreate them each time we need to get a property.
    /// This will improve the overall performance when we go to run many criterion for many specifications.
    /// These are cached as they are accessed. We can't be greedy and create them ahead of time because of the recursive nature
    /// of the type graph and these being static types, we could cause overflow exceptions. 
    /// </summary>
    private static readonly Dictionary<string, Func<object?, object?>> GetterCache = new();

    /// <summary>
    /// A custom getter function for this property which will be used instead of trying to create a getter expression
    /// using the property type information. This allows for pseudo creation of properties with custom getters which
    /// I am using in the client to get collection items for a type graph. The order of operation is custom, cache,
    /// generate expression.
    /// </summary>
    private readonly Func<object?, object?>? _getter;

    /// <summary>
    /// Creates a new <see cref="Property"/> given the name, type, and optional parent and custom getter.
    /// </summary>
    /// <param name="name">The name to the property.</param>
    /// <param name="type">The type of the property.</param>
    /// <param name="parent">The parent property of this property. If null this property should represent the root property.</param>
    /// <param name="getter">An optional custom getter that tells us how to get the value for this property given an instance of the parent object.</param>
    /// <exception cref="ArgumentException"><paramref name="name"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
    public Property(string name, Type type, Property? parent = default, Func<object?, object?>? getter = default)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name can not be null or empty to initialize a property.");

        Name = name;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Parent = parent;
        _getter = getter;
    }

    /// <summary>
    /// Creates a new nested property instance given an existing property definition and a new parent property.
    /// </summary>
    /// <param name="property">The property to recreate with a new parent.</param>
    /// <param name="parent">The new parent to the property being creates.</param>
    private Property(Property property, Property parent)
    {
        ArgumentNullException.ThrowIfNull(property);
        ArgumentNullException.ThrowIfNull(parent);

        Name = property.Name;
        Type = property.Type;
        Parent = parent;
        _getter = property._getter;
    }

    /// <summary>
    /// The string that uniquely identifies this property using the origin type and property path.
    /// </summary>
    public string Key => string.Concat(Origin, MemberSeparator, Path).TrimEnd(MemberSeparator);

    /// <summary>
    /// The root or originating type to which this property belongs.
    /// </summary>
    public Type Origin => GetOriginType();

    /// <summary>
    /// The parent property to which this property belongs. If null then this is the "root" of the type graph.
    /// </summary>
    public Property? Parent { get; }

    /// <summary>
    /// The full dot down path to this property from the origin.
    /// </summary>
    public string Path => GetPath();

    /// <summary>
    /// The return type of this property.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// The member name of this property.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// A user-friendly type name for the property. This can be used to display in the client UI.
    /// </summary>
    public string DisplayName => Type.CommonName();

    /// <summary>
    /// The <see cref="TypeGroup"/> in which this property's return type belongs.
    /// </summary>
    public TypeGroup Group => TypeGroup.FromType(Type);

    /// <summary>
    /// The set of object values that represent the options (enum/bool values) for the property type.
    /// </summary>
    public IEnumerable<object> Options => Type.GetOptions();

    /// <summary>
    /// The set of child properties which can be navigated to from this property's type.
    /// </summary>
    public IEnumerable<Property> Properties => GetProperties();

    /// <summary>
    /// The collection of types that lead up to the current property starting from the origin type.
    /// </summary>
    public Type[] TypeGraph => GetTypeGraph().ToArray();

    /// <summary>
    /// If this property type is a generic type with a single generic parameter argument, this will return the inner
    /// generic parameter type. Otherwise, it will return the same type as <see cref="Type"/>. This is useful for collections
    /// where we want to know what the types of the items in the collection.
    /// </summary>
    public Type InnerType => Type.SelfOrInnerType();

    /// <summary>
    /// Represents a default or property that is just a reference to <see cref="object"/>. We can use this in place
    /// of a null property instance.
    /// </summary>
    public static Property Default => This(typeof(object));

    /// <summary>
    /// Creates a default self-referential property called "This" with a null parent, which can be used as a root
    /// property for all sub properties of the provided type. This is need with how <see cref="Property"/> is designed
    /// to get values, since it needs some root pseudo property as the origin of the type graph.
    /// </summary>
    /// <param name="type">The <see cref="System.Type"/> of the property.</param>
    /// <returns>A <see cref="Property"/> with the provided type named "This" and a null parent.</returns>
    public static Property This(Type type) => new(nameof(This), type, null, x => x);

    /// <summary>
    /// Gets a specified nested property using the provided property path name.
    /// </summary>
    /// <param name="path">The path of the property from the current type.</param>
    /// <returns>The <see cref="Property"/> object representing the child property if found, Otherwise null.</returns>
    /// <remarks>
    /// This is the primary extension fot getting a single child or nested property from a given type. Both
    /// <see cref="Element"/> and <see cref="Property"/> make use of this extension to retrieve child property objects.
    /// This extension will check if the type is an Element type and if so also search the defined custom properties.
    /// </remarks>
    public Property? GetProperty(string? path)
    {
        if (path is null) return default;

        var property = this;
        var properties = Properties.ToList();

        while (!string.IsNullOrEmpty(path))
        {
            int index;
            string member;

            //Collection indexers are special case where we can return a "pseudo" property instance.
            if (path.StartsWith(IndexOpenSeparator) && path.Contains(IndexCloseSeparator))
            {
                index = path.IndexOf(IndexCloseSeparator) + 1;
                member = index >= 0 ? path[..index] : path;
                property = new Property(member, Type.SelfOrInnerType(), property);
            }
            else
            {
                index = path.IndexOfAny(Separators);
                member = index >= 0 ? path[..index] : path;
                var defined = properties.SingleOrDefault(p => p.Name == member);
                if (defined is null) return default;
                property = new Property(defined, property);
            }

            properties = property.Properties.ToList();
            path = index >= 0 ? path[index..].TrimStart(MemberSeparator) : string.Empty;
        }

        return property;
    }

    /// <summary>
    /// Gets the value of this <see cref="Property"/> given an instance representing the origin type this
    /// property belongs to.
    /// </summary>
    /// <param name="origin">The origin object instance for which to get the property value of.</param>
    /// <returns>A object representing the value of this property relative to the provided origin.</returns>
    /// <remarks>
    /// This is primary means though how we will get values from our element objet and use those to execute
    /// criterion for filtering and verification.
    /// </remarks>
    public object? GetValue(object? origin)
    {
        if (origin is null) return null;

        if (Origin != origin.GetType())
            throw new ArgumentException(
                $"Input object of type '{origin.GetType()}' does not match the property origin type '{Origin}'");

        //If we are at the root either use a custom getter or just return the origin object.
        //This marks the start of the traversal back down the tree.
        if (Parent is null) return _getter is not null ? _getter(origin) : origin;

        //Will recurse up the tree and back down calling each getter on the way until we have our parent object.
        var parent = Parent.GetValue(origin);

        //Use our local getter function (custom or expression) and the parent object to get the value of this property.
        var getter = GetGetter();
        return getter(parent);
    }

    /// <summary>
    /// Determines if two <see cref="Property"/> objects are equal. We are using the <see cref="Origin"/> and <see cref="Path"/>
    /// to indicate if one property is the "same" as another, since properties on different type could have same name or path.
    /// </summary>
    /// <param name="other">The other <see cref="Property"/> to compare.</param>
    /// <returns><c>true</c> if the properties are equal, otherwise, <c>false</c>.</returns>
    public bool Equals(Property? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Key == other.Key;
    }

    /// <summary>
    /// Determines if two <see cref="Property"/> objects are equal. We are using the <see cref="Origin"/> and <see cref="Path"/>
    /// to indicate if one property is the "same" as another, since properties on different type could have same name or path.
    /// </summary>
    public override bool Equals(object? obj) => obj is Property other && string.Equals(Key, other.Key);

    /// <inheritdoc />
    public override int GetHashCode() => Key.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Path;

    public static bool operator ==(Property left, Property right) => Equals(left, right);
    public static bool operator !=(Property left, Property right) => !Equals(left, right);

    /// <summary>
    /// Gets all child properties of the current property type.
    /// </summary>
    /// <returns>A collection of <see cref="Property"/> objects defining the child properties of the type.</returns>
    /// <remarks>
    /// This is the primary means through which we get properties for a given logix or .NET type. This will
    /// check if the type is an <see cref="Element"/> type and if so also append the defined custom properties for that type.
    /// </remarks>
    private IEnumerable<Property> GetProperties()
    {
        var children = new List<Property>();

        //Always first check if this type's properties we already cached to exit early and avoid reflection usage.
        if (PropertyCache.Value.TryGetValue(Type, out var cached))
            return cached.Select(c => new Property(c, this));

        //Get all non indexer properties first.
        var properties = Type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0 && !PropertyExclusions.Contains(p.Name));

        foreach (var prop in properties)
        {
            var property = new Property(prop.Name, prop.PropertyType, this);
            children.Add(property);
        }

        /*//Only for element types (like Tag) support indexers.
        if (Group == TypeGroup.Element)
        {
            //Handle indexer properties next. Only supporting single parameter indexers since that is all we really have anyway.
            //These are treated separately to change the display name of the property. 
            var indexers = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 1 && !PropertyExclusions.Contains(p.Name));

            foreach (var indexer in indexers)
            {
                var name = string.Join(',', indexer.GetIndexParameters().Select(x => x.ParameterType.CommonName()));
                var property = new Property($"[{name}]", indexer.PropertyType, this);
                children.Add(property);
            }
        }*/

        //Add custom properties defined on elements.
        if (!Element.TryFromName(Type.Name, out var element)) return children;
        children.AddRange(element.CustomProperties);

        //Cache all properties for this type, they should not change at runtime, and we can avoid reusing reflection. 
        PropertyCache.Value.TryAdd(Type, children);

        return children;
    }

    /// <summary>
    /// Retrieves the getter function for this property, which will either be the custom getter, cached expression,
    /// or the expression function we built using the info of this class. This getter function will return the value
    /// of this property provided the parent object instance. As getters are created we are caching them so that we can
    /// reuse them and not have to recreate each time we ask for a property value (which will be a lot as each spec is run)
    /// </summary>
    private Func<object?, object?> GetGetter()
    {
        //Always defer to the provided custom getter as the override to building an expression tree.
        if (_getter is not null) return _getter;

        //If we have a cached getter function for this property then return that instead of creating it again.
        if (GetterCache.TryGetValue(Key, out var cached)) return cached;

        //Build the property expression and compile it.
        var type = Parent is not null ? Parent.Type : Type;
        var getter = BuildGetterExpression(type, Name);

        //Cache this getter for future or static calls to this property for other instance objects to improve performance.
        GetterCache.Add(Key, getter);
        return getter;
    }

    /// <summary>
    /// Builds a getter expression and returns the compiled function which can be executed to get the value of this
    /// property given its parent object instance. This expression uses a null check condition to prevent null reference
    /// exceptions when accessing the child property.
    /// </summary>
    private static Func<object?, object?> BuildGetterExpression(Type parent, string name)
    {
        var parameter = Expression.Parameter(typeof(object), "x");
        var converted = Expression.TypeAs(parameter, parent);
        var member = Expression.TypeAs(BuildPropertyExpression(converted, name), typeof(object));
        var notNull = Expression.NotEqual(converted, Expression.Constant(null));
        var condition = Expression.Condition(notNull, member, Expression.Constant(null), typeof(object));
        var getter = Expression.Lambda<Func<object?, object?>>(condition, parameter).Compile();
        return getter;
    }

    /// <summary>
    /// Builds a property getter expression for the provided name and parameter.
    /// This supports collection index properties as well as normal named properties.
    /// If the <paramref name="name"/> starts and ends with the bracket notation we assume it's an indexer property and
    /// that name is the parameter to retrieve the indexed item.
    /// </summary>
    private static Expression BuildPropertyExpression(Expression parameter, string name)
    {
        if (!(name.StartsWith('[') && name.EndsWith(']')))
            return Expression.PropertyOrField(parameter, name);

        //Strip off the array brackets
        var key = name[1..^1];

        //If "name" is just a number we need to parse it becuase there is no built-in coersion for string to int.
        //In this case we can just use the propety getter for Item (built in indexer name).
        if (int.TryParse(key, out var index))
            return Expression.Property(parameter, "Item", Expression.Constant(index));

        //Otherwise, for now we are going to assume this is some string based indexer.
        //We will find the property and use its parameter type to form the expression.
        var indexer = parameter.Type.GetProperties().FirstOrDefault(x =>
            x.GetIndexParameters().Length == 1 &&
            TypeGroup.FromType(x.GetIndexParameters()[0].ParameterType) == TypeGroup.Text
        );

        //If this type has no indexer that match the supported criteria, then just return this as a property accessor.
        //This will probably fail when executed, but we want that rather than failing when attempting to generate the getter,
        //since we don't normally handle exceptions here.
        if (indexer is null)
        {
            return Expression.PropertyOrField(parameter, name);
        }

        //Convert the provided text to the type of the parameter (my Tag indexer accepts TagName which has built in coersion).
        var parameterType = indexer.GetIndexParameters()[0].ParameterType;
        var paramaterValue = Expression.Convert(Expression.Constant(key), parameterType);
        return Expression.Property(parameter, indexer, paramaterValue);
    }

    /// <summary>
    /// Gets the originating type of this property instance by traversing the object hierarchy until it reaches the root
    /// property (property with no parent).
    /// </summary>
    private Type GetOriginType()
    {
        var current = this;

        while (current.Parent is not null)
        {
            current = current.Parent;
        }

        return current.Type;
    }

    /// <summary>
    /// Retrieves the path of the current <see cref="Property"/> object by combining the names of all the properties
    /// in the hierarchy leading up to it.
    /// </summary>
    private string GetPath()
    {
        var path = string.Empty;

        var current = this;

        while (current.Parent is not null)
        {
            path = CombineMembers(current.Name, path);
            current = current.Parent;
        }

        return path;
    }

    /// <summary>
    /// Returns the type graph of the Property. The type graph represents the hierarchy of types starting from
    /// the origin parent property to the current property.
    /// </summary>
    private List<Type> GetTypeGraph()
    {
        if (Parent is null) return [Type];
        var types = new List<Type>();
        types.AddRange(Parent.GetTypeGraph());
        types.Add(Type);
        return types;
    }

    /// <summary>
    /// Combines two string values representing members to form a fully qualified path.
    /// </summary>
    /// <param name="left">The left member to combine.</param>
    /// <param name="right">The right member to combine.</param>
    /// <returns>A string representing the combined path of the left and right members.</returns>
    private static string CombineMembers(string left, string right)
    {
        return right.StartsWith('[') ? $"{left}{right}" : $"{left}.{right}".Trim(MemberSeparator);
    }
}