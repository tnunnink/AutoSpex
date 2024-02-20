using System.Collections.Generic;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using JetBrains.Annotations;

namespace AutoSpex.Client.Components;

public partial class ProjectSelector : UserControl
{
    private Project? _project;
    private IEnumerable<Project> _projects = [];
    
    public ProjectSelector()
    {
        InitializeComponent();
        DataContext = this;
    }

    [UsedImplicitly] public static readonly DirectProperty<ProjectSelector, Project?> ProjectProperty =
        AvaloniaProperty.RegisterDirect<ProjectSelector, Project?>(
            nameof(Project),
            o => o.Project,
            (o, v) => o.Project = v);
    
    [UsedImplicitly] public static readonly DirectProperty<ProjectSelector, IEnumerable<Project>> ProjectsProperty =
        AvaloniaProperty.RegisterDirect<ProjectSelector, IEnumerable<Project>>(
            nameof(Projects),
            o => o.Projects,
            (o, v) => o.Projects = v);

    public Project? Project
    {
        get => _project;
        set => SetAndRaise(ProjectProperty, ref _project, value);
    }

    public IEnumerable<Project> Projects
    {
        get => _projects;
        set => SetAndRaise(ProjectsProperty, ref _projects, value);
    }
}