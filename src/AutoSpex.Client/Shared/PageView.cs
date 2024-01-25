using System;
using Avalonia.Controls;

namespace AutoSpex.Client.Shared;

public abstract class PageView<TViewModel> : UserControl where TViewModel : PageViewModel
{
    protected TViewModel ViewModel =>
        DataContext as TViewModel ?? throw new InvalidOperationException(
            $"View {GetType().Name} DataContext must be of type {typeof(TViewModel)}.");
}