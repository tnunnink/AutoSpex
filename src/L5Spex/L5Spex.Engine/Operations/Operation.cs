using System.Reflection;
using L5Spex.Engine.Contracts;

namespace L5Spex.Engine.Operations;

public abstract class Operation : IEquatable<Operation>, IOperation
{
    protected Operation(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string Name { get; }
    
    public static Operation EqualTo => new EqualToOperation();
    
    public static Operation GreaterThan => new GreaterThanOperation();
    
    public static Operation GreaterThanOrEqualTo => new GreaterThanOrEqualToOperation();
    
    public static Operation LessThan => new LessThanOperation();
    
    public static Operation LessThanOrEqualTo => new LessThanOrEqualToOperation();
    
    public static Operation IsNull => new IsNullOperation();
    
    public static Operation IsEmpty => new IsEmptyOperation();
    
    public static Operation IsNullOrEmpty => new IsNullOrEmptyOperation();
    
    public static Operation IsNullOrWhiteSpace => new IsNullOrWhiteSpaceOperation();
    
    public static Operation IsMatch => new IsMatchOperation();
    
    public static Operation Contains => new ContainsOperation();
    
    public static Operation StartsWith => new StartsWithOperation();
    
    public static Operation EndsWith => new StartsWithOperation();
    
    public static Operation In => new InOperation();
    
    public static Operation Between => new BetweenOperation();
    
    /// <summary>
    /// Performs the operation on the input and provided values and returns the result.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="values"></param>
    /// <returns>The result of the predicate operation.</returns>
    public abstract bool Evaluate(object? input, params object[] values);

    public bool Equals(Operation? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Operation) obj);
    }

    public override int GetHashCode() => Name.GetHashCode();

    public override string ToString() => Name;
    
    public static IEnumerable<Operation> All() => Operations.Value.Select(o => o.Value);
    
    public static Operation Named(string name)
    {
        if (Operations.Value.TryGetValue(name, out var operation)) 
            return operation;
            
        throw new KeyNotFoundException($"Operation with name '{name}' was not found.");
    }
    
    private static readonly Lazy<Dictionary<string, Operation>> Operations = new(() =>
        Introspect().ToDictionary(o => o.Name));
    
    private static IEnumerable<Operation> Introspect()
    {
        var types = typeof(Operation).Assembly.GetTypes().Where(IsOperationDerivative);

        foreach (var type in types)
        {
            yield return Activator.CreateInstance(type) as Operation ??
                         throw new InvalidOperationException("Unable to create operation instance.");
        }
    }

    private static bool IsOperationDerivative(Type type)
    {
        return typeof(Operation).IsAssignableFrom(type) &&
               type is {IsAbstract: false, IsPublic: true} &&
               type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes) is not null;
    }
}