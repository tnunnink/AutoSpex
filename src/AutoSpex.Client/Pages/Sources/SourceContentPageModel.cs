using System;
using AutoSpex.Client.Observers;
using AutoSpex.Client.Shared;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Pages;

public partial class SourceContentPageModel : PageViewModel
{
    public SourceContentPageModel(SourceObserver source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Content = Source.Model.L5X.ToString();
    }

    public override string Route => $"{GetType().Name}/{Source.Model.SourceId}";
    public override string Title => "Content";

    [ObservableProperty] private SourceObserver _source;

    [ObservableProperty] private string? _content;
}