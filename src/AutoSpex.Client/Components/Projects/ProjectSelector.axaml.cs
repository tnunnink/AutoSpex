using System.Collections.ObjectModel;
using System.Windows.Input;
using AutoSpex.Client.Observers;
using Avalonia;
using Avalonia.Controls.Primitives;
using JetBrains.Annotations;

namespace AutoSpex.Client.Components;

public class ProjectSelector : TemplatedControl
{
    #region AvaloniaProperties
    
    public static readonly DirectProperty<ProjectSelector, string> SelectedProjectProperty =
        AvaloniaProperty.RegisterDirect<ProjectSelector, string>(
            nameof(SelectedProject), o => o.SelectedProject, (o, v) => o.SelectedProject = v);
    
    public static readonly DirectProperty<ProjectSelector, ObservableCollection<ProjectObserver>> ProjectsProperty =
        AvaloniaProperty.RegisterDirect<ProjectSelector, ObservableCollection<ProjectObserver>>(
            nameof(Projects), o => o.Projects, (o, v) => o.Projects = v);

    public static readonly DirectProperty<ProjectSelector, ICommand?> LaunchProjectCommandProperty =
        AvaloniaProperty.RegisterDirect<ProjectSelector, ICommand?>(
            nameof(LaunchProjectCommand), o => o.LaunchProjectCommand, (o, v) => o.LaunchProjectCommand = v);

    public static readonly DirectProperty<ProjectSelector, ICommand?> CreateCommandProperty =
        AvaloniaProperty.RegisterDirect<ProjectSelector, ICommand?>(
            nameof(CreateCommand), o => o.CreateCommand, (o, v) => o.CreateCommand = v);

    public static readonly DirectProperty<ProjectSelector, ICommand?> OpenCommandProperty =
        AvaloniaProperty.RegisterDirect<ProjectSelector, ICommand?>(
            nameof(OpenCommand), o => o.OpenCommand, (o, v) => o.OpenCommand = v);
    
    #endregion
    
    private string _selectedProject = "Select Project";
    private ObservableCollection<ProjectObserver> _projects = [];
    private ICommand? _launchProjectCommand;
    private ICommand? _createCommand;
    private ICommand? _openCommand;

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
    
    public ICommand? LaunchProjectCommand
    {
        get => _launchProjectCommand;
        set => SetAndRaise(LaunchProjectCommandProperty, ref _launchProjectCommand, value);
    }

    public ICommand? CreateCommand
    {
        get => _createCommand;
        set => SetAndRaise(CreateCommandProperty, ref _createCommand, value);
    }
    
    public ICommand? OpenCommand
    {
        get => _openCommand;
        set => SetAndRaise(OpenCommandProperty, ref _openCommand, value);
    }
}