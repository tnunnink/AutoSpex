using System;
using System.Linq;
using System.Windows.Input;
using ActiproSoftware.UI.Avalonia.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;

namespace AutoSpex.Client.Components;

public class SelectSourceView : TemplatedControl
{
    public static readonly DirectProperty<SelectSourceView, string?> LocationProperty =
        AvaloniaProperty.RegisterDirect<SelectSourceView, string?>(
            nameof(Location), o => o.Location, (o, v) => o.Location = v, defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<SelectSourceView, ICommand?> PickFileCommandProperty =
        AvaloniaProperty.RegisterDirect<SelectSourceView, ICommand?>(
            nameof(PickFileCommand), o => o.PickFileCommand, (o, v) => o.PickFileCommand = v);
    
    public static readonly DirectProperty<SelectSourceView, IStorageItem?> DroppedFileProperty =
        AvaloniaProperty.RegisterDirect<SelectSourceView, IStorageItem?>(
            nameof(DroppedFile), o => o.DroppedFile, (o, v) => o.DroppedFile = v, defaultBindingMode: BindingMode.OneWayToSource);

    private string? _location;
    private ICommand? _pickFileCommand;
    private IStorageItem? _droppedFile;
    private Border? _dropBorder;

    public string? Location
    {
        get => _location;
        set => SetAndRaise(LocationProperty, ref _location, value);
    }

    public ICommand? PickFileCommand
    {
        get => _pickFileCommand;
        set => SetAndRaise(PickFileCommandProperty, ref _pickFileCommand, value);
    }
    
    public IStorageItem? DroppedFile
    {
        get => _droppedFile;
        set => SetAndRaise(DroppedFileProperty, ref _droppedFile, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterDropBorder(e);
    }

    private void RegisterDropBorder(TemplateAppliedEventArgs args)
    {
        _dropBorder?.RemoveHandler(DragDrop.DropEvent, OnDrop);
        _dropBorder?.RemoveHandler(DragDrop.DragOverEvent, OnDragOver);
        _dropBorder?.RemoveHandler(DragDrop.DragEnterEvent, OnDragEnter);
        _dropBorder?.RemoveHandler(DragDrop.DragLeaveEvent, OnDragLeave);
        
        _dropBorder = args.NameScope.Find<Border>("DropBorder");
        
        _dropBorder?.AddHandler(DragDrop.DropEvent, OnDrop);
        _dropBorder?.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        _dropBorder?.AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
        _dropBorder?.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        var files = e.Data.GetFiles()?.ToList() ?? [];
        if (files.Count != 1) return;
        DroppedFile = files[0];
    }

    private static void OnDragOver(object? sender, DragEventArgs e)
    {
        ValidateData(e);
    }

    private static void OnDragEnter(object? sender, DragEventArgs e)
    {
        ValidateData(e);
        
        if (e is not {Source: Border border, DragEffects: DragDropEffects.Move}) return;
        
        if (Application.Current?.TryGetResource(
                ThemeResourceKind.ControlBackgroundBrushSoftAccent.ToResourceKey(),
                Application.Current?.ActualThemeVariant ?? ThemeVariant.Light,
                out var resource) is not true) return;

        if (resource is not SolidColorBrush brush) return;
            
        border.Background = brush;
    }

    private static void OnDragLeave(object? sender, DragEventArgs e)
    {
        if (e is not {Source: Border border}) return;
        
        if (Application.Current?.TryGetResource(
                ThemeResourceKind.Container3BackgroundBrush.ToResourceKey(),
                Application.Current?.ActualThemeVariant ?? ThemeVariant.Light,
                out var resource) is not true) return;

        if (resource is not SolidColorBrush brush) return;
            
        border.Background = brush;
    }
    
    private static void ValidateData(DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.None;
        
        var files = e.Data.GetFiles()?.ToList() ?? [];
        if (files.Count != 1) return;
        
        var file = files[0];
        if (!file.Path.LocalPath.EndsWith(".l5x", StringComparison.OrdinalIgnoreCase)) return;
        
        e.DragEffects = DragDropEffects.Move;
    }
}