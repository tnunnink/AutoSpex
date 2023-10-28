using Avalonia.Controls;

namespace L5Spex.Client.Features.Sources;

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