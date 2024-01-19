using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AutoSpex.Client.Components;

public partial class SpecificationView : UserControl
{
    public SpecificationView()
    {
        InitializeComponent();
        Initialized += OnInitialized;
        AttachedToVisualTree += OnAttachedToVisualTree;
        Loaded += OnLoaded;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        Console.WriteLine("AttachedToVisualTree");
    }

    private void OnInitialized(object? sender, EventArgs e)
    {
        Console.WriteLine("Initialized");
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("Loaded");
    }
}