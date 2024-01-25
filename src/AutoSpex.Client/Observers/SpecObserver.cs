using System.ComponentModel.DataAnnotations;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Observers;

public partial class SpecObserver : Observer<Spec>
{
    public SpecObserver(Spec model) : base(model)
    {
        Filters = new ObserverCollection<Criterion, CriterionObserver>(Model.Filters, 
            m => new CriterionObserver(m));
        Verifications = new ObserverCollection<Criterion, CriterionObserver>(Model.Verifications,
            m => new CriterionObserver(m));
    }

    public override bool IsChanged => base.IsChanged || Filters.IsChanged || Verifications.IsChanged;

    [Required]
    public Element Element
    {
        get => Model.Element;
        set => SetProperty(Model.Element, value, Model, (s, e) => s.Element = e, validate: true);
    }

    public ObserverCollection<Criterion, CriterionObserver> Filters { get; }
    public ObserverCollection<Criterion, CriterionObserver> Verifications { get; }

    [RelayCommand]
    private void AddFilter()
    {
        Filters.Add(new Criterion());
    }
    
    [RelayCommand]
    private void RemoveFilter(Criterion criterion)
    {
        Filters.Add(criterion);
    }
    
    [RelayCommand]
    private void AddVerification()
    {
        Verifications.Add(new Criterion());
    }
    
    [RelayCommand]
    private void RemoveVerifications(Criterion criterion)
    {
        Verifications.Add(criterion);
    }

    public static implicit operator SpecObserver(Spec model) => new(model);

    public static implicit operator Spec(SpecObserver observer) => observer.Model;
}