using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Indentation.CSharp;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace AutoSpex.Client.Components;

public class XmlEditor : TemplatedControl
{
    #region Properties

    public static readonly DirectProperty<XmlEditor, string?> XmlProperty =
        AvaloniaProperty.RegisterDirect<XmlEditor, string?>(
            nameof(Xml), o => o.Xml, (o, v) => o.Xml = v, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<ThemeVariant> ThemeVariantProperty =
        AvaloniaProperty.Register<XmlEditor, ThemeVariant>(
            nameof(ThemeVariant));

    #endregion

    private string? _xml;
    private TextEditor? _textEditor;
    private FoldingManager? _foldingManager;

    public string? Xml
    {
        get => _xml;
        set => SetAndRaise(XmlProperty, ref _xml, value);
    }

    public ThemeVariant ThemeVariant
    {
        get => GetValue(ThemeVariantProperty);
        set => SetValue(ThemeVariantProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterTextEditor(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == XmlProperty)
        {
            if (_textEditor is not null)
            {
                UninstallFolding();
                InstallFolding(_textEditor);
            }
        }

        if (change.Property == ThemeVariantProperty)
            UpdateEditorTheme(change.GetNewValue<ThemeVariant>());
    }

    private void RegisterTextEditor(TemplateAppliedEventArgs e)
    {
        _textEditor = e.NameScope.Find<TextEditor>("Editor");

        if (_textEditor is null) return;
        ConfigureEditor(_textEditor);
        ApplyEditorTheme(_textEditor);
        InstallFolding(_textEditor);
    }

    private void InstallFolding(TextEditor? editor)
    {
        if (editor?.Document is null) return;
        _foldingManager = FoldingManager.Install(editor.TextArea);
        var strategy = new XmlFoldingStrategy();
        strategy.UpdateFoldings(_foldingManager, editor.Document);
    }

    private void UninstallFolding()
    {
        if (_foldingManager is null) return;
        _foldingManager.Clear();
        FoldingManager.Uninstall(_foldingManager);
    }

    private static void ApplyEditorTheme(TextEditor editor)
    {
        var registryOptions = new RegistryOptions(ThemeName.Dark);
        var textMateInstallation = editor.InstallTextMate(registryOptions);
        var grammar = registryOptions.GetLanguageByExtension(".xml").Id;
        textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(grammar));
        //_textMateInstallation.SetTheme(_registryOptions.LoadTheme(ThemeName.DarkPlus));
    }

    private void UpdateEditorTheme(ThemeVariant theme)
    {
    }

    private static void ConfigureEditor(TextEditor editor)
    {
        editor.Options.ShowBoxForControlCharacters = true;
        editor.Options.HighlightCurrentLine = true;
        editor.Options.ColumnRulerPositions = new List<int> {80, 100};
        editor.TextArea.IndentationStrategy = new CSharpIndentationStrategy(editor.Options);
        editor.TextArea.RightClickMovesCaret = true;
    }
}

public class DocumentStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string text)
            return AvaloniaProperty.UnsetValue;

        return new TextDocument(text);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TextDocument document) return AvaloniaProperty.UnsetValue;
        return document.Text;
    }
}