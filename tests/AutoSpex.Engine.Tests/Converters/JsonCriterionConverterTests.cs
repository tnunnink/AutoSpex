using System.Text.Json;

namespace AutoSpex.Engine.Tests.Converters;

public class JsonCriterionConverterTests
{
    private JsonSerializerOptions? _options;

    [SetUp]
    public void Setup()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonCriterionConverter());
        options.Converters.Add(new JsonArgumentConverter());
        options.Converters.Add(new JsonTypeConverter());
        _options = options;
    }

    [Test]
    public void RunSerializationForLogixAtomicTypeAndValidateResults()
    {
        var criterion = new Criterion("Test", Operation.All, 1, 2, 3);
        
        var data = JsonSerializer.Serialize(criterion, _options);
        data.Should().NotBeEmpty();

        var result = JsonSerializer.Deserialize(data, typeof(Criterion), _options);
        result.Should().BeEquivalentTo(criterion, options => options.Excluding(m => m.Type == typeof(Guid)));
    }
}