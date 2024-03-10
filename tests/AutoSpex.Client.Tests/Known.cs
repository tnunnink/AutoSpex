namespace AutoSpex.Client.Tests;

public static class Known
{
    public static readonly string Test = Path.Combine(Path.GetDirectoryName(typeof(Known).Assembly.Location)!,
        "Test.xml");
    
    public static readonly string Compressed = Path.Combine(Path.GetDirectoryName(typeof(Known).Assembly.Location)!,
        "Compressed.txt");

    public static readonly string Example = Path.Combine(@"C:\Users\admin\Documents\L5X\Example.L5X");
}