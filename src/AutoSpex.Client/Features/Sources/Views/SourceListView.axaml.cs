﻿using Avalonia.Controls;

namespace AutoSpex.Client.Features.Sources;

public partial class SourceListView : UserControl
{
    public SourceListView()
    {
        InitializeComponent();
        DataContext = Container.Resolve<SourceListViewModel>();
    }
}