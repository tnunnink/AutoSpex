using System.Linq;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AutoSpex.Client.Pages;

public partial class VariablesPageModel(NodeObserver node) : PageViewModel, 
    IRecipient<VariableObserver.Checked>
{
    public override string Route => $"Node/{node.Id}/Variables";
    public override string Title => "Variables";
    public override string Icon => "Variables";
    public ObserverCollection<Variable, VariableObserver> Variables => node.Variables;

    [ObservableProperty] private bool _checkAll;

    [RelayCommand]
    private void AddVariable()
    {
        Variables.Add(new VariableObserver(new Variable("Var01")));
    }

    [RelayCommand]
    private void RemoveVariable(VariableObserver? variable)
    {
        if (variable is null) return;
        Variables.Remove(variable);
    }

    [RelayCommand(CanExecute = nameof(AnyChecked))]
    private void RemoveChecked()
    {
        var variables = Variables.Where(v => v.IsChecked).ToList();

        foreach (var variable in variables)
            Variables.Remove(variable);
    }

    private bool AnyChecked() => Variables.Any(v => v.IsChecked);

    partial void OnCheckAllChanged(bool value)
    {
        foreach (var variable in Variables)
        {
            variable.IsChecked = value;
        }
    }

    public void Receive(VariableObserver.Checked message)
    {
        if (Variables.Any(v => v.Id == message.Variable.Id))
        {
            RemoveCheckedCommand.NotifyCanExecuteChanged();
        }
    }
}