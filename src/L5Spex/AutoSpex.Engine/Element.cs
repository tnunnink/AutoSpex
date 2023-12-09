using System.Linq.Expressions;
using System.Reflection;
using Ardalis.SmartEnum;
using L5Sharp.Core;
using Module = L5Sharp.Core.Module;
using Task = L5Sharp.Core.Task;

namespace AutoSpex.Engine;

public abstract class Element : SmartEnum<Element, string>
{
    private Element(Type type) : base(type.Name, type.Name)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Properties = FindProperties();
    }

    public Type Type { get; }

    public IEnumerable<Property> Properties { get; }

    public Property Property(string path)
    {
        throw new NotImplementedException();
    }

    public Expression<Func<object, object?>> Getter(string property)
    {
        var parameter = Expression.Parameter(typeof(object), "x");
        var converted = Expression.TypeAs(parameter, Type);
        
        if (CustomGetters().TryGetValue(property, out var custom))
        {
            return Expression.Lambda<Func<object, object?>>(custom, parameter);
        }
        
        var member = GetMember(converted, property);
        return Expression.Lambda<Func<object, object?>>(member, parameter);
    }

    public Func<L5X, IEnumerable<object>> Query => file => file.Query(Type);

    public static Element Controller => new ControllerElement();
    public static Element DataType => new DataTypeElement();
    public static Element AddOnInstruction => new AddOnInstructionElement();
    public static Element Module => new ModuleElement();
    public static Element Tag => new TagElement();
    public static Element Program => new ProgramElement();
    public static Element Routine => new RoutineElement();
    public static Element Task => new TaskElement();

    public static Element? FromType(Type type) => FindByType(type); 

    public static bool TryFromType(Type type, out Element? element)
    {
        var result = FindByType(type);
        element = result;
        return element is not null;
    }

    protected virtual IEnumerable<string> PropertyExclusions()
    {
        return new[] {"L5X", "L5XType", "IsAttached"};
    }

    protected virtual Dictionary<string, Expression<Func<object, object?>>> CustomGetters() => new();

    private IEnumerable<Property> FindProperties()
    {
        var properties = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => PropertyExclusions().All(e => e != p.Name));

        return properties.Select(p => new Property(p)).ToList();
    }

    /// <summary>
    /// Recursively gets the member expression for the provided property name. This will also add null checks for
    /// each nested member to prevent null reference exceptions, and to allow null to be propagated through the
    /// member expression and returns to the operation's evaluation method.
    /// </summary>
    /// <param name="parameter">The current member access expression for the type.</param>
    /// <param name="property">The current property name to create member access to.</param>
    /// <returns>An <see cref="Expression{TDelegate}"/> that represents member access to a immediate or nested/complex
    /// member property or field, with corresponding conditional null checks for each member level.</returns>
    private static Expression GetMember(Expression parameter, string property)
    {
        if (!property.Contains('.'))
            return Expression.TypeAs(Expression.PropertyOrField(parameter, property), typeof(object));

        var index = property.IndexOf('.');
        var member = Expression.PropertyOrField(parameter, property[..index]);
        var notNull = Expression.NotEqual(member, Expression.Constant(null));
        return Expression.Condition(notNull, GetMember(member, property[(index + 1)..]), Expression.Constant(null),
            typeof(object));
    }
    
    
    private static Element? FindByType(Type type) => List.FirstOrDefault(e => e.Type == type);

    private class ControllerElement : Element
    {
        public ControllerElement() : base(typeof(Controller))
        {
        }
    }

    private class DataTypeElement : Element
    {
        public DataTypeElement() : base(typeof(DataType))
        {
        }
    }

    private class AddOnInstructionElement : Element
    {
        public AddOnInstructionElement() : base(typeof(AddOnInstruction))
        {
        }
    }

    private class ModuleElement : Element
    {
        public ModuleElement() : base(typeof(Module))
        {
        }
    }

    private class TagElement : Element
    {
        public TagElement() : base(typeof(Tag))
        {
        }
    }

    private class ProgramElement : Element
    {
        public ProgramElement() : base(typeof(Program))
        {
        }
    }

    private class RoutineElement : Element
    {
        public RoutineElement() : base(typeof(Routine))
        {
        }
    }

    private class TaskElement : Element
    {
        public TaskElement() : base(typeof(Task))
        {
        }
    }
}