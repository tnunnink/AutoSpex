using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

// ReSharper disable UnusedParameterInPartialMethod

namespace AutoSpex.Client.Features;

public partial class CriterionViewModel : ViewModelBase
{
    private readonly Criterion _criterion;
    
    public CriterionViewModel(Criterion criterion)
    {
        _criterion = criterion;
    }

    /*public Guid CriterionId => _criterion.CriterionId;*/
    public Guid NodeId { get; }
    public Operation Operation { get; set; }

    private Element _element;

    [ObservableProperty] [NotifyDataErrorInfo] [Required]
    private string? _propertyName;

    [ObservableProperty] [NotifyDataErrorInfo] [Required]
    private Property? _property;

    [ObservableProperty] [NotifyDataErrorInfo] [Required]
    private Operation? _selectedOperation;

    [ObservableProperty] private ObservableCollection<Argument> _args = new();

    [ObservableProperty] private ObservableCollection<Operation> _operations = new(Operation.List);

    [ObservableProperty] private bool _canAddValues;

    /*partial void OnPropertyNameChanged(string? value)
    {
        Property = Element.Property(value);
    }

    partial void OnPropertyChanged(Property? value)
    {
        SelectedOperation = null;
        Operations.Clear();

        if (value is null) return;

        Operations.AddRange(Operation.Supporting(value));
    }

    partial void OnSelectedOperationChanged(Operation? value)
    {
        Args.Clear();

        if (value is null || Property is null) return;

        CanAddValues = value == Operation.In;

        for (var i = 0; i < value.NumberOfArguments; i++)
        {
            Args.Add(new Arg(Property));
        }
    }
    
    public Task<IEnumerable<object>> GetProperties(string? searchText, CancellationToken token)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return Element.Properties.Cast<object>();
            }

            var index = searchText.LastIndexOf('.');
            var path = index > -1 ? searchText[..index] : searchText;
            var member = index > -1 ? searchText[(index + 1)..] : searchText;

            var property = Element.Property(path);
            var properties = property is not null ? property.Properties : Element.Properties;

            return properties
                .Where(p => p.Path.Contains(member, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.Path);
        }, token);
    }

    public void Receive(PropertyChangedMessage<Element> message)
    {
        if (message.Sender is not NodeDetailViewModel details) return;
        if (details.Node.NodeId != NodeId) return;
        Element = message.NewValue;
    }*/
}