using System.Text;
using System.Text.Json;

namespace AutoSpex.Persistence.Tests.Transforms;

[TestFixture]
public class CriterionTransformTests
{
    [Test]
    public Task Transformation_RemoveIdsMoveNegationFlattenArguemnt_ShouldBeVerified()
    {
        const string oldschmea =
            """
            {
              "CriterionId": "cf7af320-c45a-408e-a441-50381adffff3",
              "Type": "L5Sharp.Core.Tag",
              "Property": {
                "Origin": "L5Sharp.Core.Tag",
                "Path": "[Alarms.SP.ChFaultEnabled].Value"
              },
              "Operation": "Equal To",
              "Argument": {
                "ArgumentId": "981acfc4-470f-43b7-8c80-ec281916e54b",
                "Value": {
                  "Type": "L5Sharp.Core.SINT",
                  "Data": "1"
                }
              },
              "Negation": "Is"
            }
            """;

        var document = JsonDocument.Parse(oldschmea);
        var root = document.RootElement;

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        writer.WriteStartObject();

        writer.WriteString("Type", root.GetProperty("Type").GetString());

        writer.WritePropertyName("Property");
        root.GetProperty("Property").WriteTo(writer);

        writer.WriteString("Negation", root.GetProperty("Negation").GetString());
        writer.WriteString("Operation", root.GetProperty("Operation").GetString());

        writer.WritePropertyName("Argument");
        root.GetProperty("Argument").GetProperty("Value").WriteTo(writer);

        writer.WriteEndObject();
        writer.Flush();

        var result = Encoding.UTF8.GetString(stream.ToArray());
        return VerifyJson(result);
    }
}