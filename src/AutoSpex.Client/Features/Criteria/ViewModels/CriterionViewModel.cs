using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoSpex.Client.Features.Nodes;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Engine.Operations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging.Messages;
using DynamicData;

// ReSharper disable UnusedParameterInPartialMethod

namespace AutoSpex.Client.Features.Criteria;

public partial class CriterionViewModel : ViewModelBase, IRecipient<PropertyChangedMessage<Element>>
{
    public CriterionViewModel(dynamic record)
    {
        CriterionId = Guid.Parse(record.CriterionId);
        NodeId = Guid.Parse(record.NodeId);
        Usage = CriterionUsage.FromName(record.Usage);
        Element = Engine.Element.FromName(record.Element);
        PropertyName = record.Property;
        _operation = Engine.Operations.Operation.FromName(record.Operation);

        if (Property is null) return;

        string values = record.Args.ToString();
        foreach (var value in values.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var arg = new Arg(Property, value);
            Args.Add(arg);
        }
    }

    public CriterionViewModel(Guid nodeId, Element element, CriterionUsage usage)
    {
        CriterionId = Guid.NewGuid();
        NodeId = nodeId;

        Element = element ?? throw new ArgumentNullException(nameof(element));
        Usage = usage ?? throw new ArgumentNullException(nameof(usage));

        Messenger.RegisterAll(this, NodeId);
    }

    public Guid CriterionId { get; }
    public Guid NodeId { get; }
    public CriterionUsage Usage { get; }

    [ObservableProperty] private Element _element;

    [ObservableProperty] [NotifyDataErrorInfo] [Required]
    private string? _propertyName;

    [ObservableProperty] [NotifyDataErrorInfo] [Required]
    private Property? _property;

    [ObservableProperty] [NotifyDataErrorInfo] [Required]
    private Operation? _operation;

    [ObservableProperty] private ObservableCollection<Arg> _args = new();

    [ObservableProperty] private ObservableCollection<Operation> _operations = new(Operation.List);

    [ObservableProperty] private bool _canAddValues;

    public object ToRecord()
    {
        return new
        {
            CriterionId,
            NodeId,
            Usage,
            Element,
            Property = PropertyName,
            Operation,
            Args = string.Join(',', Args.Select(a => a.Value)).Trim(',')
        };
    }

    [RelayCommand]
    private void Remove()
    {
        Messenger.Send(new RemoveCriterionMessage(this), NodeId);
    }

    partial void OnPropertyNameChanged(string? value)
    {
        Property = Element.Property(value);
    }

    partial void OnPropertyChanged(Property? value)
    {
        Operation = null;
        Operations.Clear();

        if (value is null) return;

        Operations.AddRange(Operation.Supporting(value));
    }

    partial void OnOperationChanged(Operation? value)
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
    }
}