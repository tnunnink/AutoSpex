using System;
using System.Collections.Generic;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using L5Sharp.Core;
using Argument = AutoSpex.Engine.Argument;

// ReSharper disable TailRecursiveCall

namespace AutoSpex.Client.Observers;

/// <summary>
/// A observer wrapper over the <see cref="Engine.Argument"/> class for a parent criterion object.
/// </summary>
public class ArgumentObserver : Observer<Argument>
{
    /// <summary>
    /// Creates a new <see cref="ArgumentObserver"/> wrapping the provided <see cref="Argument"/>.
    /// </summary>
    /// <param name="model">The <see cref="Argument"/> object to wrap.</param>
    /// <param name="owner">The owning <see cref="CriterionObserver"/> of the argument.</param>
    public ArgumentObserver(Argument model, CriterionObserver owner) : base(model)
    {
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        Track(nameof(Value));
    }

    /// <summary>
    /// The value of the argument which represents the data in which the criterion will evaluate against. This
    /// is ultimately the persisted value.
    /// </summary>
    public object Value
    {
        get => GetValue();
        set => SetProperty(Model.Value, value, Model, SetValue);
    }

    /// <summary>
    /// The owning <see cref="CriterionObserver"/> of the argument. This is needed to pass into nested criterion
    /// so that we can know which child properties are valid. This is not a underlying model property.
    /// </summary>
    public CriterionObserver Owner { get; }

    /// <summary>
    /// The <see cref="Type"/> for the argument.
    /// </summary>
    public Type Type => Model.Type;

    /// <summary>
    /// The friendly type name for the underlying argument value.
    /// </summary>
    public string Identifier => Model.Identifier;

    /// <summary>
    /// The <see cref="TypeGroup"/> for the argument.
    /// </summary>
    public TypeGroup Group => Model.Group;

    /// <summary>
    /// The collection of available suggestions arguments which are wrapping several potential values ...
    /// </summary>
    public IEnumerable<Argument> Suggestions => Messenger.Send(new GetSuggestions(this)).Response;

    /// <inheritdoc />
    public override string? ToString() => Value.ToString();

    /// <summary>
    /// Creates a new <see cref="ArgumentObserver"/> with an empty string as the initial value.
    /// </summary>
    /// <param name="owner">The <see cref="CriterionObserver"/> which owns or has reference to this argument.</param>
    /// <returns>A new <see cref="ArgumentObserver"/> object.</returns>
    public static ArgumentObserver Empty(CriterionObserver owner) => new(Argument.Empty, owner);

    /// <summary>
    /// Creates a new <see cref="ArgumentObserver"/> with an empty <see cref="Criterion"/> as the initial value.
    /// </summary>
    /// <param name="owner">The <see cref="CriterionObserver"/> which owns or has reference to this argument.</param>
    /// <returns>A new <see cref="ArgumentObserver"/> object.</returns>
    public static ArgumentObserver Criterion(CriterionObserver owner) => new(new Criterion(), owner);

    /// <summary>
    /// Implicit conversion operator from an <see cref="ArgumentObserver"/> instance to an <see cref="Argument"/> instance.
    /// This allows an <see cref="ArgumentObserver"/> to be used wherever an <see cref="Argument"/> is expected.
    /// </summary>
    /// <param name="observer">An instance of the <see cref="ArgumentObserver"/> class.</param>
    /// <returns>An instance of the <see cref="Argument"/> class that represents the same data as the observer.</returns>
    public static implicit operator Argument(ArgumentObserver observer) => observer.Model;

    /// <summary>
    /// We need some special wrapping of the child values for Argument. it can be a nested criterion or variable, and
    /// if so we need to wrap those in observers. If not we can just return the simple value. For Criterion we pass on
    /// the owning criterion property as the origin/parent property for the criterion since it only exists on the observer
    /// as meta data for which to present possible child property options.
    /// </summary>
    private object GetValue()
    {
        return Model.Value switch
        {
            Criterion criterion => new CriterionObserver(criterion, Owner.Property?.Type),
            Variable variable => new VariableObserver(variable),
            _ => Model.Value
        };
    }

    /// <summary>
    /// When we set argument with a criterion or variable observer we need to pass in the actual model object since that
    /// is what Argument expects. Anything else can be the plain value (text, number, enum, bool, etc.)
    /// </summary>
    private static void SetValue(Argument argument, object value)
    {
        switch (value)
        {
            case Argument other:
                SetValue(argument, other.Value);
                break;
            case CriterionObserver criterion:
                argument.SetValue(criterion.Model);
                break;
            case Criterion criterion:
                argument.SetValue(criterion);
                break;
            case VariableObserver variable:
                argument.SetValue(variable.Model);
                break;
            case Variable variable:
                argument.SetValue(variable);
                break;
            case LogixEnum enumeration:
                argument.SetValue(enumeration.Name);
                break;
            case ILogixSerializable serializable:
                argument.SetValue(serializable.Serialize());
                break;
            default:
                argument.SetValue(value.ToString());
                break;
        }
    }

    /// <summary>
    /// A request message to send for retrieval of argument suggestions for this particular <see cref="ArgumentObserver"/>.
    /// </summary>
    /// <param name="argument">The argument for which to retrieve suggestions.</param>
    /// <remarks>
    /// The suggestions will represent possible option values for the type (enums/bools only), possible
    /// variables defined in the scope of the node this argument exists, and possible source values that were indexed
    /// from all loaded source files. Since this will all be a single list of args which will require external query/data
    /// retrieval, we are using this message the means for obtaining the information, since the observer itself does
    /// not have access to the Mediator instance.
    /// </remarks>
    public class GetSuggestions(ArgumentObserver argument) : RequestMessage<IEnumerable<Argument>>
    {
        public ArgumentObserver Argument { get; } = argument;
    }
}