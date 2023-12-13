using System;
using Avalonia.Controls;

namespace AutoSpex.Client.Pages;

public partial class DialogPage : Window
{
    public DialogPage()
    {
        InitializeComponent();
        Title = "AutoSpex";
    }

    public void InjectContent(Control content)
    {
        var control = this.FindControl<ContentControl>("Content");

        if (control is null)
            throw new InvalidOperationException("Could not find dialog content control host.");
        
        control.Content = content;
    }
}