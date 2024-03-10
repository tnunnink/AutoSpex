using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AutoSpex.Client.Components;

public class SaveChangesPrompt : TemplatedControl
{
    private CheckBox? _checkBox;

    public static readonly StyledProperty<string?> ItemNameProperty =
        AvaloniaProperty.Register<SaveChangesPrompt, string?>(
            nameof(ItemName));

    public string? ItemName
    {
        get => GetValue(ItemNameProperty);
        set => SetValue(ItemNameProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterCheckBox(e);
    }

    private void RegisterCheckBox(TemplateAppliedEventArgs e)
    {
        if (_checkBox is not null) _checkBox.IsCheckedChanged -= CheckBoxCheckedHandler;
        _checkBox = e.NameScope.Get<CheckBox>("CheckBox");
        _checkBox.IsChecked = Settings.App.AlwaysDiscardChanges;
        _checkBox.IsCheckedChanged += CheckBoxCheckedHandler;
    }

    private static void CheckBoxCheckedHandler(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not CheckBox checkBox) return;
        SaveAlwaysDiscardSetting(checkBox.IsChecked is true);
    }

    private static void SaveAlwaysDiscardSetting(bool value)
    {
        try
        {
            Settings.App.Save(s => s.AlwaysDiscardChanges = value);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            //todo need to log and perhaps notify, but how would we do that from the control. We could just make this
            //a view model base with a view and just resolve the view model from the view
        }
    }
}