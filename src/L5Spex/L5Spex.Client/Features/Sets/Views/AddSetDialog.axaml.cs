﻿using Avalonia.Controls;

namespace L5Spex.Client.Features.Sets.Views;

public partial class AddSetDialog : Window
{
    public AddSetDialog()
    {
        InitializeComponent();
        //DataContext = App.Current.Services.GetRequiredService<AddSetViewModel>();
    }
}