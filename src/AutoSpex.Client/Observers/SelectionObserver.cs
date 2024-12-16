using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Observers;

public partial class SelectionObserver : Observer<Selection>
{
    /// <summary>
    /// A function that returns the expected input type for this selection instance.
    /// </summary>
    private readonly Func<Property> _input;

    public SelectionObserver(Selection model, Func<Property> input) : base(model)
    {
        _input = input;

        Property = new PropertyInput(() => Model.Property, x => Model.Property = x, _input);
        Property.PropertyChanged += OnPropertyPropertyChanged;

        Track(Property);
        Track(nameof(Alias));
    }

    /// <summary>
    /// The <see cref="PropertyInput"/> that wraps this model and the underlying Property. This observer contains logic
    /// for getting, setting, and finding suggestions for this criterion instance.
    /// </summary>
    public PropertyInput Property { get; }

    /// <summary>
    /// The alias name for the selection. This is set based on the configured proeprty but can be overriden by the user.
    /// The alias name must not be empty, contain invalid characters, or be the same as any other alias for this step.
    /// </summary>
    [Required]
    [CustomValidation(typeof(SelectionObserver), nameof(ValidateAlias))]
    public string Alias
    {
        get => Model.Alias;
        set => SetProperty(Model.Alias, value, Model, (s, a) => s.Alias = a, true);
    }

    /// <inheritdoc />
    protected override bool PromptForDeletion => false;

    /// <summary>
    /// A command to add a new <see cref="Criterion"/> instance after this instance in the same collection
    /// as this criterion belongs.
    /// </summary>
    [RelayCommand]
    private void AddAfter()
    {
        var step = GetObserver<StepObserver>(s => s.Contains(this));
        if (step is not SelectObserver select) return;

        var index = select.Selections.IndexOf(this) + 1;
        if (index < 0 || index > select.Selections.Count) return;

        var next = new SelectionObserver(new Selection(), _input);
        select.Selections.Insert(index, next);
    }

    /// <inheritdoc />
    protected override void OnDeactivated()
    {
        Property.PropertyChanged -= OnPropertyPropertyChanged;
        base.OnDeactivated();
    }

    private void OnPropertyPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Property.Value))
        {
            OnPropertyChanged(nameof(Alias));
        }
    }

    /// <summary>
    /// Validates the provided alias for uniqueness and invalid characters within the context of the current SelectionObserver.
    /// </summary>
    /// <param name="alias">The alias to be validated.</param>
    /// <param name="context">The validation context.</param>
    /// <returns>A ValidationResult indicating whether the alias is valid or not.</returns>
    public static ValidationResult ValidateAlias(string alias, ValidationContext context)
    {
        var selection = (SelectionObserver)context.ObjectInstance;
        var step = selection.GetObserver<StepObserver>(s => s.Contains(selection));

        if (alias.Contains('.'))
        {
            return new ValidationResult(
                "Alias contains invalid characters. Only alphanumeric and underscores are permitted.");
        }

        if (step is not SelectObserver select)
        {
            return new ValidationResult("Could not validate alias name in current context.");
        }

        if (select.Selections.Where(s => !s.Is(selection)).Any(s => s.Alias == alias))
        {
            return new ValidationResult("Alias name must be unique to this select step.");
        }

        return ValidationResult.Success!;
    }
}