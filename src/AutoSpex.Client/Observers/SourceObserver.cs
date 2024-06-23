using System;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using L5Sharp.Core;

namespace AutoSpex.Client.Observers;

public partial class SourceObserver : Observer<Source>
{
    public SourceObserver(Source source) : base(source)
    {
        Track(nameof(TargetName));
        Track(nameof(TargetType));
        Track(nameof(ExportedBy));
        Track(nameof(ExportedOn));
    }

    public override Guid Id => Model.SourceId;
    public string Name => Model.Name;
    public bool HasContent => !string.IsNullOrEmpty(Model.Content);
    public string TargetName => Model.TargetName;
    public string TargetType => Model.TargetType;
    public string ExportedBy => Model.ExportedBy;
    public DateTime ExportedOn => Model.ExportedOn;
    public string Size => $"{ComputeSize():N0} KB";
    public string Compressed => $"{ComputeCompressedSize():N0} KB";
    public static SourceObserver Empty(Node node) => new(new Source(node));

    /// <summary>
    /// Updates the content of the Source with the specified L5X data and applies optional scrubbing.
    /// </summary>
    /// <param name="content">The L5X data to update the Source with.</param>
    /// <param name="scrub">A flag indicating whether to apply scrubbing during the update.</param>
    /// <exception cref="ArgumentNullException">Thrown when the content parameter is null.</exception>
    public void Update(L5X content, bool scrub)
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));

        Model.Update(content, scrub);

        //Will trigger property change event for all source properties to notify the UI.
        Refresh();
        Messenger.Send(new Updated(this));
    }

    [RelayCommand]
    private Task Search()
    {
        throw new NotImplementedException();
    }

    private decimal ComputeSize()
    {
        if (!HasContent) return 0;
        var data = Model.L5X.ToString();
        var bytes = System.Text.Encoding.UTF8.GetByteCount(data);
        return (decimal)bytes / 1024;
    }

    private decimal ComputeCompressedSize()
    {
        if (!HasContent) return 0;
        var data = Model.Content;
        var bytes = System.Text.Encoding.UTF8.GetByteCount(data);
        return (decimal)bytes / 1024;
    }

    public static implicit operator Source(SourceObserver observer) => observer.Model;
    public static implicit operator SourceObserver(Source source) => new(source);

    /// <summary>
    /// A message send when this source object has its content updated. 
    /// </summary>
    /// <param name="Source">The updated <see cref="SourceObserver"/></param>
    public record Updated(SourceObserver Source);
}