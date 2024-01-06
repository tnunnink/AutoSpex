using FluentMigrator;

namespace AutoSpex.Persistence;

public class MigrationIdAttribute(int major, int minor, int revision, string? description = null)
    : MigrationAttribute(CalculateVersion(major, minor, revision), description)
{
    private static long CalculateVersion(int major, int minor, int revision) =>
        major * 10000L + minor * 100L + revision;
}