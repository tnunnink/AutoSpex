using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Features.Projects;
using AutoSpex.Client.Windows;
using Avalonia;
using Avalonia.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.DialogHost;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;

namespace AutoSpex.Client.Shared;

public static class DialogExtensions
{
    public static async Task<Uri?> ShowSelectFolderDialog(this IDialogService service, string title)
    {
        var owner = (App.MainWindow.DataContext as ViewModelBase)!;

        var settings = new OpenFolderDialogSettings()
        {
            Title = title,
            AllowMultiple = false,
        };

        var folder = await service.ShowOpenFolderDialogAsync(owner, settings);
        return folder?.Path;
    }

    public static async Task<Uri?> ShowSelectProjectDialog(this IDialogService service, string title)
    {
        var owner = (App.MainWindow.DataContext as ViewModelBase)!;

        var settings = new OpenFileDialogSettings
        {
            Title = title,
            AllowMultiple = false,
            Filters = new List<FileFilter> { new("Spex Project", ".spex") }
        };

        var folder = await service.ShowOpenFileDialogAsync(owner, settings);
        return folder?.Path;
    }

    public static async Task<Uri?> ShowSelectSourceDialog(this IDialogService service)
    {
        var owner = (App.MainWindow.DataContext as ViewModelBase)!;

        var settings = new OpenFileDialogSettings
        {
            Title = "Select Source L5X",
            AllowMultiple = false,
            Filters = new List<FileFilter> { new("L5X", new[] { ".L5X", ".l5x" }) }
        };

        var folder = await service.ShowOpenFileDialogAsync(owner, settings);
        return folder?.Path;
    }

    public static async Task<Uri?> ShowNewProjectDialog(this IDialogService service)
    {
        var owner = (App.MainWindow.DataContext as ViewModelBase)!;

        var vm = service.CreateViewModel<NewProjectViewModel>();

        var settings = new DialogHostSettings
        {
            Content = vm,
            CloseOnClickAway = true,
            DialogMargin = new Thickness(0)
        };

        await service.ShowDialogHostAsync(owner, settings).ConfigureAwait(true);

        return vm.DialogResult == true ? vm.Uri : default;
    }
    
    public static async Task<string?> ShowNodeNameDialog(this IDialogService service, string title, NodeType nodeType)
    {
        var owner = (App.MainWindow.DataContext as ViewModelBase)!;

        var vm = new NodeNameViewModel(title, nodeType);

        var settings = new DialogHostSettings
        {
            Content = vm,
            CloseOnClickAway = true,
            DialogMargin = new Thickness(0)
        };

        await service.ShowDialogHostAsync(owner, settings).ConfigureAwait(true);

        return vm.DialogResult == true ? vm.Name : default;
    }
}