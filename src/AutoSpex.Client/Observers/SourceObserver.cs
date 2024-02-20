using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AutoSpex.Client.Observers;

public partial class SourceObserver(Source source) : Observer<Source>(source)
{
    [ObservableProperty] private bool _isEditing;

    public string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v, true);
    }

    public string Description
    {
        get => Model.Description;
        set => SetProperty(Model.Description, value, Model, (s, v) => s.Description = v, true);
    }

    public bool IsSelected
    {
        get => Model.IsSelected;
        set
        {
            var old = Model.IsSelected;
            var changed = SetProperty(Model.IsSelected, value, Model, (s, v) => s.IsSelected = v, true);
            if (!changed) return;
            Messenger.Send(new PropertyChangedMessage<bool>(this, nameof(IsSelected), old, Model.IsSelected));
        }
    }

    public string TargetName => Model.TargetName;
    public string TargetType => Model.TargetType;
    public string ExportedBy => Model.ExportedBy;
    public DateTime ExportedOn => Model.ExportedOn;

    public class GetSelectedRequest : AsyncRequestMessage<SourceObserver>;
}