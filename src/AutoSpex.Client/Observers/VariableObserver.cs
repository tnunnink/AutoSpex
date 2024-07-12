using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using L5Sharp.Core;

namespace AutoSpex.Client.Observers;

public partial class VariableObserver : Observer<Variable>
{
    /// <summary>
    /// Default variable constructor taking loaded variable objects and a parent node (the node that loaded them).
    /// </summary>
    public VariableObserver(Variable model) : base(model)
    {
        Track(nameof(Name));
        Track(nameof(Value));
        Track(nameof(Group));
    }

    /// <summary>
    /// This is intended to be used for creating new in memory variables where we don't want to immediately set
    /// name or value. to allow the user to input these fields and have them validated.
    /// </summary>
    /// <param name="node">The parent node that this variable belongs to.</param>
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor must be node.
    public VariableObserver(NodeObserver node) : this(new Variable(node.Id))
    {
    }

    public override Guid Id => Model.VariableId;
    public NodeObserver? Node => FindInstance<NodeObserver>(Model.NodeId);

    public TypeGroup Group
    {
        get => Model.Group;
        set => SetProperty(Model.Group, value, Model, (v, g) => v.ChangeGroup(g));
    }

    [Required(ErrorMessage = "Name is a required field")]
    [CustomValidation(typeof(VariableObserver), nameof(ValidateName))]
    public override string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (v, x) => v.Name = x, true);
    }

    public object? Value
    {
        get => Model.Value;
        set => SetProperty(Model.Value, value, Model, (v, x) => v.Value = x);
    }

    public Func<string?, CancellationToken, Task<IEnumerable<object>>> Suggestions => GetSuggestions;


    /// <summary>
    /// Updates the <see cref="Group"/> property for the variable indicating what type the value should be.
    /// This will also reset value to the default for the type group.
    /// </summary>
    [RelayCommand]
    private void UpdateGroup(TypeGroup? group)
    {
        if (group is null) return;
        Group = group;
        Value = group.DefaultValue;
    }

    /// <summary>
    /// Set the value property based on the received entry input. If input is text, we attempt to parse th input.
    /// Otherwise, we set the value to what was received.
    /// </summary>
    [RelayCommand]
    private void UpdateValue(object? value)
    {
        switch (value)
        {
            case string text when Group.TryParse(text, out var parsed) && parsed is not null:
                Value = parsed;
                return;
            case ValueObserver observer:
                Value = observer.Value;
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

    /// <summary>
    /// Gets a collection of values that represent suggestion of options for the variable value entry. These values are
    /// wrapped in a <see cref="ValueObserver"/> for displaying type information to the UI. It also filters the results
    /// based on the provided filter text.
    /// </summary>
    private IEnumerable<ValueObserver> GetTypeOptions(string? filter)
    {
        if (Group == TypeGroup.Boolean)
            return typeof(bool).GetOptions().Select(x => new ValueObserver(x));

        return Group == TypeGroup.Enum
            ? LogixEnum.Options()
                .SelectMany(x => x.Value)
                .Select(x => new ValueObserver(x))
                .Where(x => x.Filter(filter))
            : Enumerable.Empty<ValueObserver>();
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

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateMenuItems()
    {
        yield return new MenuActionItem
        {
            Header = "Copy",
            Gesture = new KeyGesture(Key.C, KeyModifiers.Control)
        };

        yield return new MenuActionItem
        {
            Header = "Duplicate",
            Command = DuplicateCommand,
            Gesture = new KeyGesture(Key.D, KeyModifiers.Control)
        };

        yield return new MenuActionItem
        {
            Header = "Delete",
            Classes = "danger",
            Command = DeleteCommand,
            Gesture = new KeyGesture(Key.Delete)
        };
    }

    /// <summary>
    /// A request to retrieve the names of the variables that are defined on the same node as this variable.
    /// We need this information to validate the entered name for the node.
    /// </summary>
    public class GetNames(VariableObserver variable) : RequestMessage<IEnumerable<string>>
    {
        public VariableObserver Variable { get; } = variable;
    }

    public static implicit operator VariableObserver(Variable variable) => new(variable);
    public static implicit operator Variable(VariableObserver observer) => observer.Model;
}