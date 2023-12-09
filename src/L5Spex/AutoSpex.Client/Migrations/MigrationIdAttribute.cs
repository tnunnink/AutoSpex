using AutoSpex.Client.Features.Projects;
using AutoSpex.Client.Shared;
using FluentMigrator;

namespace AutoSpex.Client.Migrations;

public class MigrationIdAttribute : MigrationAttribute
{
    public int Major { get; }
    public int Minor { get; }
    public int Revision { get; }

    public MigrationIdAttribute(int major, int minor, int revision, string? description = null)
        : base(CalculateVersion(major, minor, revision), description)
    {
        Major = major;
        Minor = minor;
        Revision = revision;
    }

    private static long CalculateVersion(int major, int minor, int revision) =>
        major * 10000L + minor * 100L + revision;

    public static MigrationIdAttribute FromVersion(long version)
    {
        var major = (int) (version / 10000L);
        var minor = (int) (version % 10000L / 100L);
        var revision = (int) (version % 100L);
        return new MigrationIdAttribute(major, minor, revision);
    }

    /// <summary>
    /// Determines the necessary action to reconcile application to project versions.
    /// </summary>
    /// <param name="projectId">The version of the project obtained from the database.</param>
    /// <returns>
    /// Returns the action required depending on the major and minor versions of the application and project:
    /// - NoAction, if versions are equal or only the revision version is different.
    /// - MigrationRequired, if the application has a higher major or minor version.
    /// - UpdateRequired, if the application has a lower major version.
    /// - UpdateSuggested, if application and project have the same major version, but the application has a lower minor version.
    /// </returns>
    public ProjectAction DetermineActionTo(MigrationIdAttribute projectId)
    {
        if (Major > projectId.Major) return ProjectAction.MigrationRequired;
        if (Major < projectId.Major) return ProjectAction.UpdateRequired;
    
        // if we made it here, Major versions are equal, so we compare Minor version   
        if (Minor > projectId.Minor) return ProjectAction.MigrationRequired;
        return Minor < projectId.Minor ? ProjectAction.UpdateSuggested :
            // if we made it here, Major and Minor versions are equal,
            // and they are either equal or only have a revision difference for which we don't
            // need to migrate or update for.
            ProjectAction.NoActionRequired;
    }
}