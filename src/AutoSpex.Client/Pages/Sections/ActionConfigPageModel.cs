using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Pages;

public partial class ActionConfigPageModel(ActionObserver action) : PageViewModel
{
    public override bool KeepAlive => false;
    public ActionObserver Action { get; } = action;

    [RelayCommand]
    private void Close()
    {
        Navigator.Close(this);
    }
}