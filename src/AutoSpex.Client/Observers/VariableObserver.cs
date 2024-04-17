using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Observers;

public partial class VariableObserver : Observer<Variable>
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

    [ObservableProperty] private bool _isChecked;

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

    partial void OnIsCheckedChanged(bool value)
    {
        Messenger.Send(new Checked(this));
    }

    public record Checked(VariableObserver Variable);
}