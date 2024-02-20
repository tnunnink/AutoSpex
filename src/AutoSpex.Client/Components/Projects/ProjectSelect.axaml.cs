using System.Collections.ObjectModel;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls.Primitives;
using JetBrains.Annotations;

namespace AutoSpex.Client.Components;

public class ProjectSelect : TemplatedControl
{
    
    private string _selectedProject = "Select Project";
    private ObservableCollection<ProjectObserver> _projects = [];

    [UsedImplicitly] public static readonly DirectProperty<ProjectSelect, string> SelectedProjectProperty =
        AvaloniaProperty.RegisterDirect<ProjectSelect, string>(
            nameof(SelectedProject), o => o.SelectedProject, (o, v) => o.SelectedProject = v);

    [UsedImplicitly]
    public static readonly DirectProperty<ProjectSelect, ObservableCollection<ProjectObserver>> ProjectsProperty =
        AvaloniaProperty.RegisterDirect<ProjectSelect, ObservableCollection<ProjectObserver>>(
            nameof(Projects), o => o.Projects, (o, v) => o.Projects = v);

    public string SelectedProject
    {
        get => _selectedProject;
        set => SetAndRaise(SelectedProjectProperty, ref _selectedProject, value);
    }

    public ObservableCollection<ProjectObserver> Projects
    {
        get => _projects;
        set => SetAndRaise(ProjectsProperty, ref _projects, value);
    }
}