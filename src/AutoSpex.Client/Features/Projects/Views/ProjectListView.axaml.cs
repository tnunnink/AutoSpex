﻿using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AutoSpex.Client.Features.Projects;

public partial class ProjectListView : UserControl
{
    public ProjectListView()
    {
        InitializeComponent();
        DataContext = Container.Resolve<ProjectListViewModel>();
    }

    private void OnItemPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var popup = this.FindLogicalAncestorOfType<Popup>();
        if (popup is null) return;
        popup.IsOpen = false;
    }
}