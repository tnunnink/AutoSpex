using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class CriterionObserver : Observer<Criterion>,
    IRecipient<Observer.Request<CriterionObserver>>
{
    public CriterionObserver(Criterion model) : base(model)
    {
        Arguments = new ObserverCollection<Argument, ArgumentObserver>(Model.Arguments, a => new ArgumentObserver(a));

        Track(nameof(Property));
        Track(nameof(Operation));
        Track(nameof(Invert));
        Track(Arguments);
    }

    public override Guid Id => Model.CriterionId;

    [Required]
    public Property Property
    {
        get => Model.Property;
        set => SetProperty(Model.Property, value, Model, (c, p) => c.Property = p);
    }

    /// <summary>
    /// The <see cref="Engine.Operation"/> to execute for the criterion. This property wraps the underlying model.
    /// </summary>
    [Required]
    public Operation Operation
    {
        get => Model.Operation;
        set => SetProperty(Model.Operation, value, Model, (c, o) => c.Operation = o, true);
    }

    public ObserverCollection<Argument, ArgumentObserver> Arguments { get; }

    public bool Invert
    {
        get => Model.Invert;
        set => SetProperty(Model.Invert, value, Model, (c, v) => c.Invert = v);
    }

    public Func<string?, CancellationToken, Task<IEnumerable<object>>> PopulateProperties => GetProperties;
    public Func<string?, CancellationToken, Task<IEnumerable<object>>> PopulateOperations => GetOperations;

    public Func<object?, string> SelectOperation => x =>
        x is Operation operation && operation != Operation.None ? operation.Name : string.Empty;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Operation))
            UpdateArguments();
    }

    public void Receive(Request<CriterionObserver> message)
    {
        if (message.HasReceivedResponse) return;
        if (Id != message.Id && Arguments.All(a => a.Id != message.Id)) return;
        message.Reply(this);
    }

    protected override Task Duplicate()
    {
        Messenger.Send(new Duplicated(this, new CriterionObserver(Model)));
        return Task.CompletedTask;
    }

    /// <summary>
    /// Command to update the configured <see cref="Property"/> for this Criterion given the input object.
    /// This input can be text that the user types or a selected property from the suggestion popup.
    /// </summary>
    [RelayCommand]
    private void UpdateProperty(object? value)
    {
        switch (value)
        {
            case Property property:
                Property = property;
                return;
            case string path:
                Property = Property.This(Model.Type).GetProperty(path) ?? Property.Default;
                return;
        }
    }

    /// <summary>
    /// Updates the criterion <see cref="Operation"/> based on the provided value type.
    /// If the user provides text we can try to parse it but if not then we need to set to null/none.
    /// </summary>
    [RelayCommand]
    private void UpdateOperation(object? value)
    {
        switch (value)
        {
            case Operation operation:
                Operation = operation;
                return;
            case string text:
                var found = Operation.TryFromName(text, out var parsed);
                Operation = found ? parsed : Operation.None;
                return;
        }
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
            Arguments.Add(new ArgumentObserver());
        }

        if (Operation is TernaryOperation)
        {
            Arguments.Add(new ArgumentObserver());
            Arguments.Add(new ArgumentObserver());
        }

        if (Operation == Operation.In)
        {
            Arguments.Add(new ArgumentObserver());
            return;
        }

        if (Operation == Operation.Count)
        {
            Arguments.Add(new ArgumentObserver());
            return;
        }

        if (Operation is not CollectionOperation) return;
        var innerType = Property.InnerType;
        Arguments.Add(new ArgumentObserver(new Argument(new Criterion(innerType))));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private Task<IEnumerable<object>> GetProperties(string? filter, CancellationToken token)
    {
        var type = Model.Type;
        var origin = Property.This(type);

        if (string.IsNullOrEmpty(filter))
            return Task.FromResult(origin.Properties.Cast<object>());

        var index = filter.LastIndexOf('.');
        var path = index > -1 ? filter[..index] : string.Empty;
        var member = index > -1 ? filter[(index + 1)..] : filter;

        var property = origin.GetProperty(path);
        var properties = property?.Properties ?? origin.Properties;

        var filtered = properties
            .Where(p => p.Name.Contains(member, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Name)
            .Cast<object>();

        return Task.FromResult(filtered);
    }

    /// <summary>
    /// Gets all <see cref="Engine.Operation"/> types that are supported by the current configured property, and filters
    /// them based on the entry text.
    /// </summary>
    private Task<IEnumerable<object>> GetOperations(string? filter, CancellationToken token)
    {
        var filtered = Operation.Supporting(Property).Where(x => x.Name.Satisfies(filter));
        return Task.FromResult(filtered.Cast<object>());
    }

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

    public static implicit operator Criterion(CriterionObserver observer) => observer.Model;
    public static implicit operator CriterionObserver(Criterion model) => new(model);
}