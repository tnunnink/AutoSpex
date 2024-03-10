using System;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages;

public partial class SourceInfoPageModel : PageViewModel
{
    public SourceInfoPageModel(SourceObserver source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public override string Route => $"{GetType().Name}/{Source.Model.SourceId}";
    public override string Title => "Info";

    [ObservableProperty] private SourceObserver _source;
}