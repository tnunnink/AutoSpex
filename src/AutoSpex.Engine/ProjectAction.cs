using Ardalis.SmartEnum;

namespace AutoSpex.Engine;

/// <summary>
/// Defines the possible actions that can be taken when launching a project, 
/// depending on the comparison between the project and application's version.
/// </summary>
public class ProjectAction : SmartEnum<ProjectAction, int>
{
    /// <summary>
    /// No action is required. This is often the case when the application 
    /// and database versions match or only their patch versions differ.
    /// </summary>
    public static readonly ProjectAction NoActionRequired = new(nameof(NoActionRequired), 0);

    /// <summary>
    /// A database migration is required. This is usually the case when the
    /// application's major or minor version is higher than the project's.
    /// </summary>
    public static readonly ProjectAction MigrationRequired = new(nameof(MigrationRequired), 1);

    /// <summary>
    /// The application requires an update due to incompatible changes. This 
    /// typically happens when the application's major version is lower than the project's.
    /// </summary>
    public static readonly ProjectAction UpdateRequired = new(nameof(UpdateRequired), 2);

    /// <summary>
    /// An update for the application is suggested. This is usually the case when 
    /// the application and the project have the same major version, but the 
    /// application has a lower minor version.
    /// </summary>
    public static readonly ProjectAction UpdateSuggested = new(nameof(UpdateSuggested), 3);

    private ProjectAction(string name, int value) : base(name, value)
    {
    }

    /// <summary>
    /// Determines the necessary action to reconcile application to project versions.
    /// </summary>
    /// <param name="current"></param>
    /// <param name="target"></param>
    /// <returns>
    /// Returns the action required depending on the major and minor versions of the application and project:
    /// - NoAction, if versions are equal or only the revision version is different.
    /// - MigrationRequired, if the application has a higher major or minor version.
    /// - UpdateRequired, if the application has a lower major version.
    /// - UpdateSuggested, if application and project have the same major version, but the application has a lower minor version.
    /// </returns>
    public static ProjectAction DetermineAction(long current, long target)
    {
        var currentMajor = GetMajorVersion(current);
        var targetMajor = GetMajorVersion(target);

        if (currentMajor > targetMajor) return MigrationRequired;
        if (currentMajor < targetMajor) return UpdateRequired;

        // if we made it here, Major versions are equal, so we compare Minor version
        var currentMinor = GetMinorVersion(current);
        var targetMinor = GetMinorVersion(target);
        if (currentMinor > targetMinor) return MigrationRequired;
        return currentMinor < targetMinor ? UpdateSuggested : NoActionRequired;
    }

    private static int GetMajorVersion(long version) => (int) (version / 10000L);

    private static int GetMinorVersion(long version) => (int) (version % 10000L / 100L);
}