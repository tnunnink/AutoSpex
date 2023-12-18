using System;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;

namespace AutoSpex.Client.Shared;

public abstract partial class DialogViewModel : ViewModelBase, IModalDialogViewModel, ICloseable
{
    public bool? DialogResult { get; private set; }
    public event EventHandler? RequestClose;
    
    [RelayCommand]
    private void Ok()
    {
        DialogResult = true;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
    
    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}