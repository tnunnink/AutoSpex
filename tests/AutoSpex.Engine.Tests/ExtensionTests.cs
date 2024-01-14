namespace AutoSpex.Engine.Tests;

[TestFixture]
public class ExtensionTests
{
    [Test]
    public void Compress_Test_ShouldWork()
    {
        var content = L5X.Load(Known.Test);

        var data = content.ToString().Compress();
        
        File.WriteAllText(Known.Compressed, data);
    }
    
    [Test]
    public void Compress_Example_ShouldWork()
    {
        var content = L5X.Load(Known.Example);

        var data = content.ToString().Compress();
        
        File.WriteAllText(Known.Compressed, data);
    }

    [Test]
    public void Decompress_Compressed_ShouldWork()
    {
        var text = File.ReadAllText(Known.Compressed);

        var data = text.Decompress();

        var content = L5X.Parse(data);
        
        content.Should().NotBeNull();
    }
}