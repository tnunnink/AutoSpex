using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia.Controls;

namespace AutoSpex.Client.Components;

public partial class ProjectItem : UserControl
{
    public ProjectItem()
    {
        InitializeComponent();
    }
}

public class DesignProjectObserver() : ProjectObserver(new Project(@"C:\Users\admin\Documents\Spex\Test.spex"));