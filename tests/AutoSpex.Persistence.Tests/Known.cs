namespace AutoSpex.Persistence.Tests;

public static class Known
{
    public const string Repo = @"C:\Users\tnunnink\Documents\Rockwell";

    public static readonly string Test = Path.Combine(Path.GetDirectoryName(typeof(Known).Assembly.Location)!,
        "Test.xml");

    public static readonly string Compressed = Path.Combine(Path.GetDirectoryName(typeof(Known).Assembly.Location)!,
        "Compressed.txt");

    public static readonly string Example = Path.Combine(Repo, "Example.L5X");
}