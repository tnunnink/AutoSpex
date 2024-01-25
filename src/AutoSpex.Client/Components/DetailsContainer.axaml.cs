using System.Collections.ObjectModel;
using System.Windows.Input;
using ActiproSoftware.UI.Avalonia.Controls;
using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;

namespace AutoSpex.Client.Components;

[PublicAPI]
public class DetailsContainer : TabControl
{
    private ICommand? _addCommand;
    private ICommand? _closeTabCommand;

    public DetailsContainer()
    {
        CloseTabCommand = new AsyncRelayCommand<PageViewModel>(CloseTab);
    }

    public static readonly DirectProperty<DetailsContainer, ICommand?> AddCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsContainer, ICommand?>(
            nameof(AddCommand), o => o.AddCommand, (o, v) => o.AddCommand = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<DetailsContainer, ICommand?> CloseTabCommandProperty =
        AvaloniaProperty.RegisterDirect<DetailsContainer, ICommand?>(
            nameof(CloseTabCommand), o => o.CloseTabCommand, (o, v) => o.CloseTabCommand = v);

    public ObservableCollection<PageViewModel> PagesSource => (ItemsSource as ObservableCollection<PageViewModel>)!;

    public ICommand? AddCommand
    {
        get => _addCommand;
        set => SetAndRaise(AddCommandProperty, ref _addCommand, value);
    }

    public ICommand? CloseTabCommand
    {
        get => _closeTabCommand;
        set => SetAndRaise(CloseTabCommandProperty, ref _closeTabCommand, value);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new DetailsTab();
    }
    
    private async Task CloseTab(PageViewModel? page)
    {
        if (page is null) return;

        if (!page.IsChanged)
        {
            PagesSource.Remove(page);
            page.Close();
            return;
        }
        
        //todo move to extension method
        var answer = await UserPromptBuilder.Configure().Show();
        //todo add check to stop prompting and persist using settings
        if (answer != MessageBoxResult.Yes) return;
    }
}