using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Pages;

public partial class AddFilterPageModel(Element element) : PageViewModel
{
    public CriterionObserver Filter = new(new Criterion(element.Type));

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        //todo how to signal back to caller to add Filter?
    }

    private bool CanAdd()
    {
        return !Filter.HasErrors && Filter.Property != Property.Default && Filter.Operation != Operation.None;
    }
}