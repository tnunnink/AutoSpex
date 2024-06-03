using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using L5Sharp.Core;

namespace AutoSpex.Client.Observers;

public partial class VariableObserver : Observer<Variable>
{
    /// <summary>
    /// Default variable constructor taking loaded variable objects and a parent node (the node that loaded them).
    /// </summary>
    public VariableObserver(Variable model, NodeObserver? node = default) : base(model)
    {
        _group = Model.Group;
        _name = Model.Name;
        _value = Model.Value;
        _node = node;
        Track(nameof(Name));
        Track(nameof(Value));
    }

    /// <summary>
    /// This is intended to be used for creating new in memory variables where we don't want to immediately set
    /// name or value. to allow the user to input these fields and have them validated.
    /// </summary>
    /// <param name="node">The parent node that this variable belongs to.</param>
    public VariableObserver(NodeObserver node) : base(new Variable())
    {
        _group = Model.Group;
        _node = node;
        Track(nameof(Name));
        Track(nameof(Value));
    }

    public override Guid Id => Model.VariableId;
    public string ScopedReference => Model.ScopedReference;
    public string AbsoluteReference => Model.AbsoluteReference;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScopedReference))]
    [NotifyPropertyChangedFor(nameof(AbsoluteReference))]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Name is a required field")]
    [CustomValidation(typeof(VariableObserver), nameof(ValidateName))]
    private string? _name;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Value is a required field")]
    [CustomValidation(typeof(VariableObserver), nameof(ValidateValue))]
    private object? _value;

    [ObservableProperty] private string? _description;

    [ObservableProperty] private TypeGroup _group;

    [ObservableProperty] private NodeObserver? _node;

    public Action<object?> CommitValue => UpdateValue;
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> Suggestions => GetSuggestions;

    public override string ToString() => ScopedReference;
    public static implicit operator VariableObserver(Variable variable) => new(variable);
    public static implicit operator Variable(VariableObserver observer) => observer.Model;

    /// <summary>
    /// Update the underlying model property when the the value changed to keep in sync.
    /// We should only get a change when the name is validated.
    /// </summary>
    /// <param name="value">The updated name.</param>
    partial void OnNameChanged(string? value)
    {
        if (GetErrors(nameof(Name)).Any() || value is null) return;
        Model.Name = value;
    }

    /// <summary>
    /// Update the underlying model property when the value changed to keep in sync.
    /// We should only get a change when the value is validated.
    /// </summary>
    /// <param name="value">The updated value.</param>
    partial void OnValueChanged(object? value)
    {
        if (GetErrors(nameof(Value)).Any()) return;
        Model.Value = value;
    }

    /// <summary>
    /// If the user selects a different type group then we want to change the underlying variable data. This will
    /// change variable type, group, and the default value, hence why we want to notify property change.
    /// </summary>
    partial void OnGroupChanged(TypeGroup value)
    {
        Model.ChangeType(value);
        Value = Model.Value;
    }

    partial void OnDescriptionChanged(string? value)
    {
        Model.Description = value;
    }

    /// <summary>
    /// Set the value property based on the received entry input. If input is text, we attempt to parse th input.
    /// Otherwise, we set the value to what was received.
    /// </summary>
    private void UpdateValue(object? value)
    {
        switch (value)
        {
            case string text when Group.TryParse(text, out var parsed):
                Value = parsed;
                return;
            case ValueObserver observer:
                Value = observer.Model;
                return;
            default:
                Value = value;
                break;
        }
    }

    public static ValidationResult? ValidateName(string name, ValidationContext context)
    {
        var observer = (VariableObserver)context.ObjectInstance;
        var names = observer.RequestNames().ToList();

        return names.Any(name.Equals)
            ? new ValidationResult("Name must be unique to this node.")
            : ValidationResult.Success;
    }

    public static ValidationResult? ValidateValue(object value, ValidationContext context)
    {
        var observer = (VariableObserver)context.ObjectInstance;

        //If the entered value is text then try to parse and if not return parse error.
        if (value is string text && !observer.Group.TryParse(text, out _))
        {
            return new ValidationResult($"Unable to parse input as {observer.Group}");
        }

        //If the entered value is an object whose type does not match the selected variable group, return type error.
        var group = TypeGroup.FromType(value.GetType());
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (group != observer.Group)
        {
            return new ValidationResult($"Unable to set {group} to a {observer.Group} variable type.");
        }

        return ValidationResult.Success;
    }

    private Task<IEnumerable<object>> GetSuggestions(string? filter, CancellationToken token)
    {
        var suggestions = new List<ValueObserver>();

        suggestions.AddRange(GetTypeOptions(filter));
        
        

        return Task.FromResult(suggestions.Cast<object>().AsEnumerable());
    }

    private IEnumerable<ValueObserver> GetTypeOptions(string? filter)
    {
        if (Group == TypeGroup.Boolean)
            return typeof(bool).GetOptions().Select(x => new ValueObserver(x));

        return Group == TypeGroup.Enum
            ? LogixEnum.Options()
                .SelectMany(x => x.Value)
                .Select(x => new ValueObserver(x))
                .Where(x => x.Passes(filter))
            : Enumerable.Empty<ValueObserver>();
    }

    private Task<IEnumerable<ValueObserver>> GetSourceComponents(string? filter)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sends the <see cref="GetNames"/> request message to find the names of variables that are defined in the same
    /// node as this variable. We want this to validate that the name input is unique for this variable.
    /// </summary>
    private IEnumerable<string> RequestNames()
    {
        var request = new GetNames(this);
        Messenger.Send(request);
        return request.HasReceivedResponse ? request.Response : Enumerable.Empty<string>();
    }

    /// <summary>
    /// A request to retrieve the names of the variables that are defined on the same node as this variable.
    /// We need this information to validate the entered name for the node.
    /// </summary>
    public class GetNames(VariableObserver variable) : RequestMessage<IEnumerable<string>>
    {
        public VariableObserver Variable { get; } = variable;
    }
}