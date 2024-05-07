using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;

namespace AutoSpex.Client.Observers;

public class VariableObserver : Observer<Variable>
{
    public VariableObserver(Variable model) : base(model)
    {
        Track(nameof(Name));
        Track(nameof(Value));
    }

    public override Guid Id => Model.VariableId;
    public Guid NodeId => Model.NodeId;
    public TypeGroup Group => Model.Group;
    public string Formatted => Model.Formatted;

    public string Name
    {
        get => Model.Name;
        set
        {
            var set = SetProperty(Model.Name, value, Model, (m, v) => m.Name = v);
            if (!set) return;
            OnPropertyChanged(Formatted);
        }
    }

    public object Value
    {
        get => Model.Value;
        set => SetProperty(Model.Value, value, Model, (m, v) => m.Value = v);
    }

    public override string ToString() => Model.ToString();
}