namespace AutoSpex.Engine.Tests;

public static class Known
{
    public static readonly string Test = Path.Combine(Path.GetDirectoryName(typeof(Known).Assembly.Location)!,
        "Test.xml");

    public static readonly string Example = Path.Combine(@"C:\Users\tnunn\Documents\L5X\Example.L5X");

    public static readonly string Archive = Path.Combine(Path.GetDirectoryName(typeof(Known).Assembly.Location)!,
        "Test.ACD");

    public static readonly string Compressed = Path.Combine(Path.GetDirectoryName(typeof(Known).Assembly.Location)!,
        "Test.L5Z");

    public static readonly string Fake = Path.Combine(Path.GetDirectoryName(typeof(Known).Assembly.Location)!,
        "Fake.L5X");
}