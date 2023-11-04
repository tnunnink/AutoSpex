using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace L5Spex.Client.ViewModels;

public abstract class ViewModelBase : ObservableValidator
{
    private TaskNotifier? _notifier;
    
    /// <summary>
    /// The task that executes the initialization of the view mode.
    /// </summary>
    protected Task? Run
    {
        get => _notifier;
        set => SetPropertyAndNotifyOnCompletion(ref _notifier, value);
    }
}