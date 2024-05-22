using System;
using System.ComponentModel;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Observers;

public partial class CriterionObserver : Observer<Criterion>
{
    public CriterionObserver(Criterion model, Type? type = default) : base(model)
    {
        Type = type?.SelfOrInnerType();

        Arguments = new ObserverCollection<Argument, ArgumentObserver>(
            Model.Arguments, a => new ArgumentObserver(a));

        Track(nameof(Invert));
        Track(nameof(PropertyName));
        Track(nameof(Operation));
        Track(Arguments);

        ResolveProperty();
    }

    public override Guid Id => Model.CriterionId;

    [ObservableProperty] private bool _isEnabled = true;

    [ObservableProperty] private bool _isResolved;

    [ObservableProperty] private Type? _type;

    [ObservableProperty] private Property? _property;

    public bool Invert
    {
        get => Model.Invert;
        set => SetProperty(Model.Invert, value, Model, (c, v) => c.Invert = v);
    }

    public string? PropertyName
    {
        get => Model.Property;
        set => SetProperty(Model.Property, value, Model, (c, p) => c.Property = p);
    }

    public Operation? Operation
    {
        get => Model.Operation;
        set => SetProperty(Model.Operation, value, Model, (c, o) => c.Operation = o);
    }

    public ObserverCollection<Argument, ArgumentObserver> Arguments { get; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(PropertyName):
            case nameof(Type):
                ResolveProperty();
                break;
            case nameof(Operation):
                UpdateArguments();
                break;
        }
    }

    private void ResolveProperty()
    {
        if (string.IsNullOrEmpty(PropertyName))
            return;

        Property = Type?.Property(PropertyName);
        IsResolved = Property is not null;
    }

    private void UpdateArguments()
    {
        Arguments.Clear();

        if (Operation is BinaryOperation)
            Arguments.Add(ArgumentObserver.Empty);

        if (Operation is TernaryOperation)
        {
            Arguments.Add(ArgumentObserver.Empty);
            Arguments.Add(ArgumentObserver.Empty);
        }

        if (Operation is not CollectionOperation)
            return;

        Arguments.Add(new ArgumentObserver(new Argument(new Criterion())));
    }

    /// <summary>
    /// Implicit conversion operator from an <see cref="CriterionObserver"/> instance to a <see cref="Criterion"/> instance.
    /// This enables an <see cref="CriterionObserver"/> to be used wherever a <see cref="Criterion"/> is expected.
    /// </summary>
    /// <param name="observer">An instance of the <see cref="CriterionObserver"/> class.</param>
    /// <returns>An instance of the <see cref="Criterion"/> class that corresponds to the same data as in the observer.</returns>
    public static implicit operator Criterion(CriterionObserver observer) => observer.Model;
}