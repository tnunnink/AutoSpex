using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

public class JsonEvaluationConverter : JsonConverter<Criterion>
{
    public override Criterion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case nameof(Evaluation.CriterionId):
                    break;
                case nameof(Evaluation.Result):
                    break;
                case nameof(Evaluation.Message):
                    break;
                case nameof(Evaluation.Candidate):
                    break;
                
            }
        }
        
        return default!;
    }

    public override void Write(Utf8JsonWriter writer, Criterion value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(Evaluation.CriterionId), value.CriterionId);

        writer.WriteEndObject();
    }
}