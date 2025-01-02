using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Styling;

namespace AutoSpex.Client.Resources.Controls;

public class DataTableView : TemplatedControl
{
    private const string PartDataGrid = "PART_DataGrid";
    private DataGrid? _dataGrid;

    #region Properties

    public static readonly StyledProperty<DataTable?> TableProperty =
        AvaloniaProperty.Register<DataTableView, DataTable?>(
            nameof(Table));

    public static readonly StyledProperty<IDataTemplate> ColumnTemplateProperty =
        AvaloniaProperty.Register<DataTableView, IDataTemplate>(
            nameof(ColumnTemplate));

    public static readonly StyledProperty<ControlTheme> ColumnThemeProperty =
        AvaloniaProperty.Register<DataTableView, ControlTheme>(
            nameof(ColumnTheme));

    public static readonly StyledProperty<ControlTheme> RowThemeProperty =
        AvaloniaProperty.Register<DataTableView, ControlTheme>(
            nameof(RowTheme));

    public static readonly StyledProperty<ControlTheme> CellThemeProperty =
        AvaloniaProperty.Register<DataTableView, ControlTheme>(
            nameof(CellTheme));

    public static readonly StyledProperty<int> ColumnCountProperty =
        AvaloniaProperty.Register<DataTableView, int>(
            nameof(ColumnCount));

    public static readonly StyledProperty<ControlTheme> DefaultIconProperty =
        AvaloniaProperty.Register<ListView, ControlTheme>(
            nameof(DefaultIcon));

    public static readonly StyledProperty<string?> DefaultMessageProperty =
        AvaloniaProperty.Register<ListView, string?>(
            nameof(DefaultMessage));

    public static readonly StyledProperty<string?> DefaultCaptionProperty =
        AvaloniaProperty.Register<ListView, string?>(
            nameof(DefaultCaption));

    #endregion

    public DataTable? Table
    {
        get => GetValue(TableProperty);
        set => SetValue(TableProperty, value);
    }

    public IDataTemplate ColumnTemplate
    {
        get => GetValue(ColumnTemplateProperty);
        set => SetValue(ColumnTemplateProperty, value);
    }

    public ControlTheme ColumnTheme
    {
        get => GetValue(ColumnThemeProperty);
        set => SetValue(ColumnThemeProperty, value);
    }

    public ControlTheme RowTheme
    {
        get => GetValue(RowThemeProperty);
        set => SetValue(RowThemeProperty, value);
    }

    public ControlTheme CellTheme
    {
        get => GetValue(CellThemeProperty);
        set => SetValue(CellThemeProperty, value);
    }

    public ControlTheme DefaultIcon
    {
        get => GetValue(DefaultIconProperty);
        set => SetValue(DefaultIconProperty, value);
    }

    public string? DefaultMessage
    {
        get => GetValue(DefaultMessageProperty);
        set => SetValue(DefaultMessageProperty, value);
    }

    public string? DefaultCaption
    {
        get => GetValue(DefaultCaptionProperty);
        set => SetValue(DefaultCaptionProperty, value);
    }

    public int ColumnCount
    {
        get => GetValue(ColumnCountProperty);
        private set => SetValue(ColumnCountProperty, value);
    }

    public ObservableCollection<object?[]> Items { get; } = [];

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TableProperty)
        {
            UpdateDataGrid(change.GetNewValue<DataTable>());
        }
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        RegisterDataGrid(e);
    }

    /// <summary>
    /// Stores the instance of the data grid part which we need to populate the data from the bound data table.
    /// </summary>
    private void RegisterDataGrid(TemplateAppliedEventArgs e)
    {
        _dataGrid = e.NameScope.Get<DataGrid>(PartDataGrid);
    }

    /// <summary>
    /// Refreshes the control's data grid with the data of the provided data table.
    /// </summary>
    private void UpdateDataGrid(DataTable? table)
    {
        if (table is null) return;
        if (_dataGrid is null) return;

        _dataGrid.Columns.Clear();
        Items.Clear();

        BuildColumns(table).ForEach(_dataGrid.Columns.Add);
        BuildRows(table).ForEach(Items.Add);
    }

    /// <summary>
    /// Builds a data grid column for each column of the current data table.
    /// </summary>
    private static List<DataGridColumn> BuildColumns(DataTable table)
    {
        var columns = new List<DataGridColumn>();

        foreach (DataColumn column in table.Columns)
        {
            DataGridTextColumn dgc = new()
            {
                Header = column.ColumnName,
                Binding = new Binding($"[{column.Ordinal}]"),
                IsReadOnly = column.ReadOnly,
                Width = new DataGridLength(1, DataGridLengthUnitType.SizeToCells)
            };

            columns.Add(dgc);
        }

        return columns;
    }

    /// <summary>
    /// Extracts the items array from each show and returns the collection to be used for populating the data grid.
    /// </summary>
    private static List<object?[]> BuildRows(DataTable table)
    {
        var items = new List<object?[]>();

        foreach (DataRow row in table.Rows)
        {
            items.Add(row.ItemArray);
        }

        return items;
    }
}