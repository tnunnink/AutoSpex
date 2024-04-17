using System;
using System.Text;
using System.Threading.Tasks;
using AutoSpex.Client.Services;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using AutoSpex.Persistence;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;

namespace AutoSpex.Client.Observers;

public partial class SourceObserver(Source source) : NamedObserver<Source>(source),
    IRecipient<SourceObserver.Selected>
{
    public override Guid Id => Model.SourceId;

    public override string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (s, v) => s.Name = v);
    }

    public string Documentation
    {
        get => Model.Documentation;
        set => SetProperty(Model.Documentation, value, Model, (m, s) => m.Documentation = s);
    }

    public bool IsSelected
    {
        get => Model.IsSelected;
        set => SetProperty(Model.IsSelected, value, Model, (s, v) => s.IsSelected = v);
    }

    public string TargetName => Model.TargetName;
    public string TargetType => Model.TargetType;
    public string TargetTypeFormatted => $"[{TargetType}]";
    public string ExportedBy => Model.ExportedBy;
    public DateTime ExportedOn => Model.ExportedOn;
    /*public string Size => $"{(decimal) Encoding.Unicode.GetByteCount(Model.L5X.ToString()) / 1048576:F1} MB";*/

    public string Size => $"{Model.L5X.ToString().Length * sizeof(char) / 1024:N0} KB";

    /// <inheritdoc />
    protected override async Task Delete()
    {
        var delete = await Prompter.PromptDelete(Name);
        if (delete is not true) return;

        var result = await Mediator.Send(new DeleteSource(Id));
        if (result.IsFailed) return;

        Messenger.Send(new Deleted(this));
        Messenger.UnregisterAll(this);
    }

    [RelayCommand]
    private void Select()
    {
        IsSelected = true;
        Messenger.Send(new Selected(Id));
    }

    public void Receive(Selected message)
    {
        if (Id == message.SourceId) return;
        IsSelected = false;
    }

    protected override Task<Result> RenameModel(string name) => Mediator.Send(new RenameSource(this));

    public static implicit operator Source(SourceObserver observer) => observer.Model;
    public static implicit operator SourceObserver(Source source) => new(source);

    public record Selected(Guid SourceId);
}