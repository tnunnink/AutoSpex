using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AutoSpex.Client.Components;

public class EvaluationList : TemplatedControl
{
    #region Properties

    public static readonly DirectProperty<EvaluationList, ObservableCollection<EvaluationObserver>>
        EvaluationsProperty =
            AvaloniaProperty.RegisterDirect<EvaluationList, ObservableCollection<EvaluationObserver>>(
                nameof(Evaluations), o => o.Evaluations, (o, v) => o.Evaluations = v, []);

    #endregion

    private ObservableCollection<EvaluationObserver> _evaluations = [];
    private TextBox? _filterText;
    private ComboBox? _resultCombo;

    public ObservableCollection<EvaluationObserver> Evaluations
    {
        get => _evaluations;
        set => SetAndRaise(EvaluationsProperty, ref _evaluations, value);
    }

    public ObservableCollection<EvaluationObserver> EvaluationCollection { get; } = [];
    public ObservableCollection<string> ResultOptions => ["All", "Passed", "Failed", "Error"];


    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateEvaluationCollection(Evaluations);
        RegisterFilterText(e);
        RegisterResultCombo(e);
    }

    /// <summary>
    /// Get the filter text box and attach the text change handler to respond to filter input.
    /// </summary>
    private void RegisterFilterText(TemplateAppliedEventArgs e)
    {
        if (_filterText is not null)
            _filterText.TextChanged -= FilterTextChangedHandler;

        _filterText = e.NameScope.Get<TextBox>("FilterText");

        if (_filterText is null) return;
        _filterText.TextChanged += FilterTextChangedHandler;
    }

    /// <summary>
    /// When the filter text changes filter the source evaluation observer objects and update the view collection. 
    /// </summary>
    private void FilterTextChangedHandler(object? sender, TextChangedEventArgs e)
    {
        if (e.Source is not TextBox textBox) return;
        var filter = textBox.Text;

        var filtered = Evaluations.Where(x => x.Filter(filter)).ToList();
        UpdateEvaluationCollection(filtered);
    }

    /// <summary>
    /// Get the result combo box and attach the selection change handler to respond to filter based on the selected state.
    /// </summary>
    private void RegisterResultCombo(TemplateAppliedEventArgs e)
    {
        if (_resultCombo is not null)
            _resultCombo.SelectionChanged -= ResultSelectionChangedHandler;

        _resultCombo = e.NameScope.Get<ComboBox>("ResultCombo");

        if (_resultCombo is null) return;
        _resultCombo.SelectedValue = "All";
        _resultCombo.SelectionChanged += ResultSelectionChangedHandler;
    }

    /// <summary>
    /// 
    /// </summary>
    private void ResultSelectionChangedHandler(object? sender, SelectionChangedEventArgs e)
    {
        if (e.Source is not ComboBox comboBox) return;
        var selection = comboBox.SelectedValue?.ToString();
        //todo finish
        var filtered = Evaluations.ToList();
        UpdateEvaluationCollection(filtered);
    }

    /// <summary>
    /// Refreshes the evaluation collection to UI is bound to with the provided collection of evaluations.
    /// </summary>
    private void UpdateEvaluationCollection(IEnumerable<EvaluationObserver>? evaluations)
    {
        EvaluationCollection.Clear();

        if (evaluations is null) return;

        foreach (var evaluation in evaluations)
        {
            EvaluationCollection.Add(evaluation);
        }
    }
}