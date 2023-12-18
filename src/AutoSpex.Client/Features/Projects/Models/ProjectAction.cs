namespace AutoSpex.Client.Features.Projects;

/// <summary>
/// Defines the possible actions that can be taken when launching a project, 
/// depending on the comparison between the project and application's version.
/// </summary>
public enum ProjectAction
{
    /// <summary>
    /// No action is required. This is often the case when the application 
    /// and database versions match or only their patch versions differ.
    /// </summary>
    NoActionRequired,

    /// <summary>
    /// A database migration is required. This is usually the case when the
    /// application's major or minor version is higher than the project's.
    /// </summary>
    MigrationRequired,

    /// <summary>
    /// The application requires an update due to incompatible changes. This 
    /// typically happens when the application's major version is lower than the project's.
    /// </summary>
    UpdateRequired,

    /// <summary>
    /// An update for the application is suggested. This is usually the case when 
    /// the application and the project have the same major version, but the 
    /// application has a lower minor version.
    /// </summary>
    UpdateSuggested
}