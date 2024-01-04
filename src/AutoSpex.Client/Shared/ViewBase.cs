using System;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSpex.Client.Shared;

public abstract class ViewBase<TViewModel> : UserControl where TViewModel : ViewModelBase
{
    protected TViewModel ViewModel =>
        DataContext as TViewModel ?? throw new InvalidOperationException(
            $"View {GetType().Name} DataContext must be of type {typeof(TViewModel)}.");
}