using System;
using System.Linq;
using System.Windows.Input;
using AutoSpex.Client.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using L5Sharp.Core;

namespace AutoSpex.Client.Components;

[PseudoClasses(PcDragOver)]
public class SourceLoader : TemplatedControl
{
    #region Properties

    public static readonly DirectProperty<SourceLoader, L5X?> ContentProperty =
        AvaloniaProperty.RegisterDirect<SourceLoader, L5X?>(
            nameof(Content), o => o.Content, (o, v) => o.Content = v, defaultBindingMode: BindingMode.OneWayToSource);

    public static readonly DirectProperty<SourceLoader, string?> LocationProperty =
        AvaloniaProperty.RegisterDirect<SourceLoader, string?>(
            nameof(Location), o => o.Location, (o, v) => o.Location = v, defaultBindingMode: BindingMode.OneWay);

    public static readonly DirectProperty<SourceLoader, bool> ScrubDataProperty =
        AvaloniaProperty.RegisterDirect<SourceLoader, bool>(
            nameof(ScrubData), o => o.ScrubData, (o, v) => o.ScrubData = v, true, BindingMode.TwoWay);

    public static readonly DirectProperty<SourceLoader, bool> UpdateVariablesProperty =
        AvaloniaProperty.RegisterDirect<SourceLoader, bool>(
            nameof(UpdateVariables), o => o.UpdateVariables, (o, v) => o.UpdateVariables = v, true, BindingMode.TwoWay);

    public static readonly DirectProperty<SourceLoader, string> ErrorMessageProperty =
        AvaloniaProperty.RegisterDirect<SourceLoader, string>(
            nameof(ErrorMessage), o => o.ErrorMessage, (o, v) => o.ErrorMessage = v);

    #endregion

    public SourceLoader()
    {
        LoadFileCommand = new RelayCommand<string?>(LoadSourceFile);
        OpenFileCommand = new AsyncRelayCommand(OpenFile);
    }

    private const string PcDragOver = ":dragover";
    private const string DropBorder = "DropBorder";
    private L5X? _content;
    private string? _location;
    private bool _scrubData;
    private bool _updateVariables;
    private string _errorMessage = string.Empty;
    private Border? _dropBorder;


    public L5X? Content
    {
        get => _content;
        set => SetAndRaise(ContentProperty, ref _content, value);
    }

    public string? Location
    {
        get => _location;
        private set => SetAndRaise(LocationProperty, ref _location, value);
    }

    public bool ScrubData
    {
        get => _scrubData;
        set => SetAndRaise(ScrubDataProperty, ref _scrubData, value);
    }

    public bool UpdateVariables
    {
        get => _updateVariables;
        set => SetAndRaise(UpdateVariablesProperty, ref _updateVariables, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetAndRaise(ErrorMessageProperty, ref _errorMessage, value);
    }

    public ICommand LoadFileCommand { get; }
    public ICommand OpenFileCommand { get; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterDropBorder(e);
    }

    private async Task OpenFile()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var uri = await topLevel.StorageProvider.SelectL5XUri();
        if (uri is null) return;

        LoadSourceFile(uri.LocalPath);
    }

    private void LoadSourceFile(string? location)
    {
        if (string.IsNullOrEmpty(location)) return;

        ErrorMessage = string.Empty;
        Content = default;

        try
        {
            Content = L5X.Load(location);
            Location = location;
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        var files = e.Data.GetFiles()?.ToList() ?? [];
        if (files.Count != 1) return;

        var file = files[0];

        LoadSourceFile(file.Path.LocalPath);
        UpdateDraggingPseudoClass(false);
    }

    private static void OnDragOver(object? sender, DragEventArgs e)
    {
        ValidateData(e);
    }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        ValidateData(e);
        if (e.DragEffects != DragDropEffects.Move) return;
        UpdateDraggingPseudoClass(true);
    }

    private void OnDragLeave(object? sender, DragEventArgs e)
    {
        UpdateDraggingPseudoClass(false);
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

    private void UpdateDraggingPseudoClass(bool value)
    {
        if (value)
        {
            PseudoClasses.Add(PcDragOver);
            return;
        }

        PseudoClasses.Remove(PcDragOver);
    }

    private void RegisterDropBorder(TemplateAppliedEventArgs args)
    {
        _dropBorder?.RemoveHandler(DragDrop.DropEvent, OnDrop);
        _dropBorder?.RemoveHandler(DragDrop.DragOverEvent, OnDragOver);
        _dropBorder?.RemoveHandler(DragDrop.DragEnterEvent, OnDragEnter);
        _dropBorder?.RemoveHandler(DragDrop.DragLeaveEvent, OnDragLeave);

        _dropBorder = args.NameScope.Find<Border>(DropBorder);

        _dropBorder?.AddHandler(DragDrop.DropEvent, OnDrop);
        _dropBorder?.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        _dropBorder?.AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
        _dropBorder?.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
    }
}