using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace L5Spex.Features.Sources;

public partial class SourceMenuView : UserControl
{
    public SourceMenuView()
    {
        InitializeComponent();
        
        if (App.Local.Services is not null)
        {
            
        }
    }
}