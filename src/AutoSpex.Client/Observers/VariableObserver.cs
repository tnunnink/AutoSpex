using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AutoSpex.Client.Observers;

public partial class VariableObserver : Observer<Variable>
{
    public VariableObserver(Variable model) : base(model)
    {
        Track(Name);
        Track(Value);
    }

    public override Guid Id => Model.VariableId;
    public NodeObserver? Node => RequestNode();

    [ObservableProperty] private bool _isChecked;

    [ObservableProperty] private bool _isOverridden;

    public string Name
    {
        get => Model.Name;
        set
        {
            var set = SetProperty(Model.Name, value, Model, (m, v) => m.Name = v);
            if (!set) return;
            OnPropertyChanged(Formatted);
        }
    }

    public string Value
    {
        get => Model.Value;
        set => SetProperty(Model.Value, value, Model, (m, v) => m.Value = v);
    }

    public string? Override
    {
        get => Model.Override;
        set => SetProperty(Model.Override, value, Model, (m, v) => m.Override = v);
    }

    public string Formatted => Model.Formatted;

    public override string ToString() => Model.ToString();

    partial void OnIsCheckedChanged(bool value)
    {
        Messenger.Send(new Checked(this));
    }

    private NodeObserver? RequestNode()
    {
        var request = new NodeRequest(Id);
        Messenger.Send(request);
        if (!request.HasReceivedResponse) return default;
        return request.Response;
    }

    public record Checked(VariableObserver Variable);

    public class NodeRequest(Guid variableId) : RequestMessage<NodeObserver>
    {
        public Guid VariableId { get; } = variableId;
    }
}