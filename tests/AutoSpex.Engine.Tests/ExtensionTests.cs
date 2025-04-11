using System.Diagnostics;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class ExtensionTests
{
    [Test]
    public void ToText_Boolean_ShouldBeExpected()
    {
        const bool value = true;

        var result = value.ToText();

        result.Should().Be("true");
    }

    [Test]
    public void ToText_Integer_ShouldBeExpected()
    {
        const int value = 123;

        var result = value.ToText();

        result.Should().Be("123");
    }

    [Test]
    public void ToText_Double_ShouldBeExpected()
    {
        const double value = 12.3;

        var result = value.ToText();

        result.Should().Be("12.3");
    }

    [Test]
    public void ToText_String_ShouldBeExpected()
    {
        const string value = "This is some text value.";

        var result = value.ToText();

        result.Should().Be("This is some text value.");
    }

    [Test]
    public void ToText_LogixEnum_ShouldBeExpected()
    {
        var value = ExternalAccess.ReadOnly;

        var result = value.ToText();

        result.Should().Be("ReadOnly");
    }

    [Test]
    public void ToText_LogixScoped_ShouldBeExpected()
    {
        var value = new Tag("MyTagName", 123);

        var result = value.ToText();

        result.Should().Be("/Tag/MyTagName");
    }

    [Test]
    public void ToText_ListOfIntegers_ShouldBeExpected()
    {
        var value = new List<int> { 1, 2, 3, 4 };

        var result = value.ToText();

        result.Should().Be("[1,2,3,4]");
    }

    [Test]
    public void ToText_ListOfStrings_ShouldBeExpected()
    {
        var value = new List<string> { "First", "Second", "Third" };

        var result = value.ToText();

        result.Should().Be("[First,Second,Third]");
    }

    [Test]
    public void ToText_ListOfScoped_ShouldBeExpected()
    {
        var tag1 = new Tag("MyTag1", 123);
        var tag2 = new Tag("MyTag2", 123);
        var tag3 = new Tag("MyTag3", 123);
        var value = new List<Tag> { tag1, tag2, tag3 };

        var result = value.ToText();

        result.Should().Be("[/Tag/MyTag1,/Tag/MyTag2,/Tag/MyTag3]");
    }

    [Test]
    public void ToText_Range_ShouldBeExpected()
    {
        var value = new Range(12, 1424);

        var result = value.ToText();

        result.Should().Be("12 and 1424");
    }

    [Test]
    public async Task CompressToAsync_ValidFile_ShouldCreateFileOnDisc()
    {
        var content = await L5X.LoadAsync(Known.Test, L5XOptions.Index);

        await content.Serialize().ToString().CompressToAsync(Known.Compressed);

        File.Exists(Known.Compressed);
    }

    [Test]
    public async Task DecompressToAsync_ValidFile_ShouldNotBeNull()
    {
        var content = await Known.Compressed.DecompressToAsync();

        content.Should().NotBeEmpty();
    }
}