using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Observers;

public partial class VariableObserver(Variable model) : Observer<Variable>(model)
{
    private const char VariableStart = '{';
    private const char VariableEnd = '}';

    public Guid Id { get; } = Guid.NewGuid();

    public string Name
    {
        get => Model.Name;
        set
        {
            SetProperty(Model.Name, value, Model, (m, v) => m.Name = v);
            OnPropertyChanged(nameof(Formatted));
        }
    }

    public string Value
    {
        get => Model.Value;
        set => SetProperty(Model.Value, value, Model, (m, v) => m.Value = v);
    }

    public string? Description
    {
        get => Model.Description;
        set => SetProperty(Model.Description, value, Model, (m, v) => m.Description = v);
    }

    [ObservableProperty] private string? _override;
    public string Formatted => $"{VariableStart}{Name}{VariableEnd}";

    [RelayCommand]
    private void Remove()
    {
        //todo send message to parent
    }

    public override string ToString() => Name;

    public class RemoveMessage(VariableObserver variable);
    
    public class RenameRequest(VariableObserver variable);
}