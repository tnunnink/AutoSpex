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
        _node = node;
        Track(nameof(Name));
        Track(nameof(Value));
    }

    /// <summary>
    /// This is intended to be used for creating new in memory variables where we don't want to immediately set
    /// name or value. to allow the user to input these fields and have them validated.
    /// </summary>
    /// <param name="node">The parent node that this variable belongs to.</param>
    public VariableObserver(NodeObserver node) : this(new Variable(), node)
    {
    }

    public override Guid Id => Model.VariableId;
    public string ScopedReference => Model.ScopedReference;

    [ObservableProperty] private NodeObserver? _node;

    public TypeGroup Group
    {
        get => Model.Group;
        set => SetProperty(Model.Group, value, Model, UpdateGroup);
    }

    [Required(ErrorMessage = "Name is a required field")]
    [CustomValidation(typeof(VariableObserver), nameof(ValidateName))]
    public string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (v, x) => v.Name = x, true);
    }

    public object? Value
    {
        get => Model.Value;
        set => SetProperty(Model.Value, value, Model, (v, x) => v.Value = x);
    }

    public string? Description
    {
        get => Model.Description;
        set => SetProperty(Model.Description, value, Model, (v, s) => v.Description = s);
    }

    public Action<object?> CommitValue => UpdateValue;
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> Suggestions => GetSuggestions;
    public override string ToString() => ScopedReference;
    public static implicit operator VariableObserver(Variable variable) => new(variable);
    public static implicit operator Variable(VariableObserver observer) => observer.Model;

    /// <summary>
    /// 
    /// </summary>
    private void UpdateGroup(Variable variable, TypeGroup group)
    {
        variable.ChangeGroup(group);
        Value = group.DefaultValue;
    }

    /// <summary>
    /// Set the value property based on the received entry input. If input is text, we attempt to parse th input.
    /// Otherwise, we set the value to what was received.
    /// </summary>
    private void UpdateValue(object? value)
    {
        switch (value)
        {
            case string text when Group.TryParse(text, out var parsed) && parsed is not null:
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

    /// <summary>
    /// Gets a collection of objects to be presented on the value entry as suggestion values based on the variable type.
    /// </summary>
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
        //1. Get the source

        return Task.FromResult(Enumerable.Empty<ValueObserver>());
    }

    /// <summary>
    /// Validates the name of this variable using the <see cref="GetNames"/> request message. The variable name should
    /// be unique and not contains certain characters.
    /// </summary>
    public static ValidationResult? ValidateName(string name, ValidationContext context)
    {
        var observer = (VariableObserver)context.ObjectInstance;
        var names = observer.RequestNames().ToList();

        return names.Any(name.Equals)
            ? new ValidationResult("Name must be unique to this node.")
            : ValidationResult.Success;
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