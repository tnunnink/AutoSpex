using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace AutoSpex.Client.Components;

public class RunNodeButton : TemplatedControl
{
    private const string FilterTextBox = "FilterText";

    public static readonly DirectProperty<RunNodeButton, ICommand?> RunCommandProperty =
        AvaloniaProperty.RegisterDirect<RunNodeButton, ICommand?>(
            nameof(RunCommand), o => o.RunCommand, (o, v) => o.RunCommand = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<RunNodeButton, ICommand?> AddSourceCommandProperty =
        AvaloniaProperty.RegisterDirect<RunNodeButton, ICommand?>(
            nameof(AddSourceCommand), o => o.AddSourceCommand, (o, v) => o.AddSourceCommand = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<RunNodeButton, ObservableCollection<SourceObserver>> SourcesProperty =
        AvaloniaProperty.RegisterDirect<RunNodeButton, ObservableCollection<SourceObserver>>(
            nameof(Sources), o => o.Sources, (o, v) => o.Sources = v);


    private ICommand? _runCommand;
    private ICommand? _addSourceCommand;
    private ObservableCollection<SourceObserver> _sources = [];
    private TextBox? _filterText;

    public ICommand? RunCommand
    {
        get => _runCommand;
        set => SetAndRaise(RunCommandProperty, ref _runCommand, value);
    }

    public ICommand? AddSourceCommand
    {
        get => _addSourceCommand;
        set => SetAndRaise(AddSourceCommandProperty, ref _addSourceCommand, value);
    }

    public ObservableCollection<SourceObserver> Sources
    {
        get => _sources;
        set => SetAndRaise(SourcesProperty, ref _sources, value);
    }

    public ObservableCollection<SourceObserver> SourceCollection { get; } = [];

    public SourceObserver? SelectedSource => Sources.SingleOrDefault(s => s.IsSelected);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SourcesProperty)
            UpdateSourceCollection(change.GetNewValue<ObservableCollection<SourceObserver>>());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterFilterText(e);
    }

    private void RegisterFilterText(TemplateAppliedEventArgs args)
    {
        if (_filterText is not null)
        {
            _filterText.TextChanged -= FilterTextChangedHandler;
        }

        _filterText = args.NameScope.Find<TextBox>(FilterTextBox);
        if (_filterText is null) return;

        _filterText.TextChanged += FilterTextChangedHandler;
    }

    private void FilterTextChangedHandler(object? sender, TextChangedEventArgs e)
    {
        if (e.Source is not TextBox textBox || string.IsNullOrEmpty(textBox.Text))
        {
            UpdateSourceCollection(Sources);
            return;
        }

        var filtered = Sources.Where(s => s.Name.Contains(textBox.Text, StringComparison.OrdinalIgnoreCase));
        UpdateSourceCollection(filtered);
    }

    private void UpdateSourceCollection(IEnumerable<SourceObserver> sources)
    {
        SourceCollection.Clear();

        foreach (var source in sources)
        {
            SourceCollection.Add(source);
        }
    }
}