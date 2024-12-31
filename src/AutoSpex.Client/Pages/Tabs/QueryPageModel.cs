using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Threading;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoSpex.Client.Pages;

public partial class QueryPageModel : PageViewModel
{
    private readonly SourceObserver _source;
    
    public QueryPageModel(SourceObserver source) : base("Query")
    {
        _source = source;
        Query = new QueryObserver(new Query(), source);
        RegisterDisposable(Query);
    }

    public override string Route => $"Source/{_source.Id}/{Title}";
    public override string Icon => "IconFilledBinoculars";
    public QueryObserver Query { get; }

    [ObservableProperty] private DataTable? _results;

    [ObservableProperty] private int _pageSize = 50;

    [ObservableProperty] private bool _showDrawer;


    [RelayCommand(CanExecute = nameof(CanExecute))]
    private async Task Execute()
    {
        if (_source.Model.Content is null) return;
        if (Query.Element == Element.Default) return;

        try
        {
            var query = Query.Model;
            var content = _source.Model.Content;
            var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var results = (await Task.Run(() => query.Execute(content), cancellation.Token)).ToList();

            var table = new DataTable();
            ConfigureTableFor(table, query.Returns);
            PopulateTableWith(table, results);

            Results = table;
            ShowDrawer = true;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            Notifier.ShowError("Failed to search content", e.Message);
        }
    }

    private bool CanExecute() => _source.Model.Content is not null;

    /// <inheritdoc />
    protected override void FilterChanged(string? filter)
    {
    }

    /// <summary>
    /// Configures the provided <see cref="DataTable"/> with properties based on the return type of the provided
    /// <see cref="Property"/> (the type returned byt the query). If the type is an expando object we expect many columns.
    /// Otherwise, we expect a single column.
    /// </summary>
    private static void ConfigureTableFor(DataTable table, Property property)
    {
        if (property.Type != typeof(ExpandoObject))
        {
            var name = property.Name == "This" ? property.Type.Name : property.Path;
            table.Columns.Add(new DataColumn(name, property.Type));
        }
        else
        {
            table.Columns.AddRange(property.Properties.Select(p => new DataColumn(p.Name, p.Type)).ToArray());
        }
    }

    /// <summary>
    /// Popupates the table with rows based on the provided result objects. If the result object is an expando object,
    /// then we want to add an array of values based on the selected properties. Otherwise, we just add the result value. 
    /// </summary>
    private static void PopulateTableWith(DataTable table, IEnumerable<object?> results)
    {
        foreach (var result in results)
        {
            if (result is not ExpandoObject expando)
            {
                table.Rows.Add(result);
                continue;
            }

            table.Rows.Add(((IDictionary<string, object?>)expando).Values.ToArray());
        }
    }
}