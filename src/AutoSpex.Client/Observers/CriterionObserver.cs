using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Observers;

public partial class CriterionObserver : Observer<Criterion>
{
    public CriterionObserver(Criterion model) : base(model)
    {
        Arguments = new ObserverCollection<Argument, ArgumentObserver>(
            Model.Arguments, a => new ArgumentObserver(a, this));

        Track(nameof(PropertyName));
        Track(nameof(Operation));
        Track(nameof(Invert));
        Track(Arguments);

        ResolveProperty();
    }

    public override Guid Id => Model.CriterionId;

    public string? PropertyName
    {
        get => Model.Property;
        set => SetProperty(Model.Property, value, Model, (c, p) => c.Property = p);
    }

    [ObservableProperty] [NotifyDataErrorInfo] [Required]
    private Property? _property;

    public Action<object?> PropertyCommiter => CommitProperty;
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> PropertyGetter => GetProperties;

    /// <summary>
    /// The <see cref="Engine.Operation"/> to execute for the criterion. This property wraps the underlying model.
    /// </summary>
    [Required]
    public Operation? Operation
    {
        get => Model.Operation;
        set => SetProperty(Model.Operation, value, Model, (c, o) => c.Operation = o, true);
    }

    public Func<object?, string> OperationSelector => SelectOperation;
    public Action<object?> OperationCommiter => CommitOperation;
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> OperationGetter => GetOperations;

    public ObserverCollection<Argument, ArgumentObserver> Arguments { get; }

    public bool Invert
    {
        get => Model.Invert;
        set => SetProperty(Model.Invert, value, Model, (c, v) => c.Invert = v);
    }

    [ObservableProperty] private bool _isEnabled = true;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(PropertyName):
                ResolveProperty();
                break;
            case nameof(Operation):
                UpdateArguments();
                break;
        }
    }

    public static implicit operator Criterion(CriterionObserver observer) => observer.Model;
    public static implicit operator CriterionObserver(Criterion model) => new(model);


    private Task<IEnumerable<object>> GetProperties(string? filter, CancellationToken token)
    {
        var type = Model.Type;
        
        if (type is null)
            return Task.FromResult(Enumerable.Empty<object>());

        if (string.IsNullOrEmpty(filter))
            return Task.FromResult(type.Properties().Cast<object>());

        var index = filter.LastIndexOf('.');
        var path = index > -1 ? filter[..index] : string.Empty;
        var member = index > -1 ? filter[(index + 1)..] : filter;

        var property = type.Property(path);
        var properties = property?.Properties ?? type.Properties();

        var filtered = properties
            .Where(p => p.Name.Contains(member, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Name)
            .Cast<object>();

        return Task.FromResult(filtered);
    }

    private void CommitProperty(object? obj)
    {
        switch (obj)
        {
            case Property property:
                PropertyName = property.Path;
                return;
            case string text:
                PropertyName = text;
                return;
        }
    }

    /// <summary>
    /// Gets all <see cref="Engine.Operation"/> types that are supported by the current configured property, and filters
    /// them based on the entry text.
    /// </summary>
    private Task<IEnumerable<object>> GetOperations(string? filter, CancellationToken token)
    {
        var filtered = Property is not null
            ? Operation.Supporting(Property).Where(x => x.Name.PassesFilter(filter))
            : Enumerable.Empty<object>();

        return Task.FromResult(filtered);
    }

    /// <summary>
    /// Selects the correct text to display in the operation entry control given a strongly typed object.
    /// </summary>
    private static string SelectOperation(object? item)
    {
        return item switch
        {
            Operation x => x.Name,
            _ => item?.ToString() ?? string.Empty
        };
    }

    /// <summary>
    /// Updates the criterion <see cref="Operation"/> based on the provided value type. If the user provides text we
    /// can try to parse it but if not then we need to set to null.
    /// </summary>
    private void CommitOperation(object? obj)
    {
        switch (obj)
        {
            case Operation operation:
                Operation = operation;
                return;
            case string text:
                var found = Operation.TryFromName(text, out var parsed);
                Operation = found ? parsed : null;
                return;
        }
    }

    private void ResolveProperty()
    {
        if (string.IsNullOrEmpty(PropertyName))
            return;

        Property = Model.Type?.Property(PropertyName);
    }

    /// <summary>
    /// Updates the criterion arguments collection based on the selected operation.
    /// Each operation type expects a certain number of arguments (except for In).
    /// Collection operations expect an inner criterion that's type needs to be the inner type of the collection.
    /// </summary>
    private void UpdateArguments()
    {
        Arguments.Clear();

        if (Operation is BinaryOperation)
        {
            Arguments.Add(new ArgumentObserver(this));
        }

        if (Operation is TernaryOperation)
        {
            Arguments.Add(new ArgumentObserver(this));
            Arguments.Add(new ArgumentObserver(this));
        }

        // ReSharper disable once InvertIf
        if (Operation is CollectionOperation)
        {
            var innerType = Property?.Type.SelfOrInnerType() ?? typeof(object);
            Arguments.Add(new ArgumentObserver(new Argument(new Criterion(innerType)), this));
        }

        if (Operation == Operation.In)
        {
        }
    }
}