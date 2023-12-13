using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Shared;

public abstract class ViewModelBase : ObservableValidator, IRevertibleChangeTracking
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

    public bool IsChanged { get; private set; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (IsChanged) return;
        IsChanged = true;
        OnPropertyChanged(nameof(IsChanged));
    }

    public virtual void AcceptChanges()
    {
        IsChanged = false;
        OnPropertyChanged(nameof(IsChanged));
    }

    public virtual void RejectChanges()
    {
        IsChanged = false;
        OnPropertyChanged(nameof(IsChanged));
    }

    /*public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }*/
}