using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoSpex.Engine;

public class JsonEvaluationConverter : JsonConverter<Evaluation>
{
    public override Evaluation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var criterionId = Guid.Empty;
        var result = ResultState.None;
        var candidate = string.Empty;
        var message = string.Empty;
        var expected = string.Empty;
        var actual = string.Empty;
        var exception = string.Empty;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case nameof(Evaluation.CriterionId):
                    criterionId = reader.GetGuid();
                    break;
                case nameof(Evaluation.Result):
                    var state = reader.GetString();
                    if (state is null) break;
                    result = Enum.Parse<ResultState>(state);
                    break;
                case nameof(Evaluation.Candidate):
                    candidate = reader.GetString() ?? string.Empty;
                    break;
                case nameof(Evaluation.Criteria):
                    message = reader.GetString() ?? string.Empty;
                    break;
                case nameof(Evaluation.Expected):
                    expected = reader.GetString() ?? string.Empty;
                    break;
                case nameof(Evaluation.Actual):
                    actual = reader.GetString();
                    break;
                case nameof(Evaluation.Error):
                    exception = reader.GetString();
                    break;
            }
        }

        return new Evaluation(criterionId, result, candidate, message, expected, actual, exception);
    }

    public override void Write(Utf8JsonWriter writer, Evaluation value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(Evaluation.CriterionId), value.CriterionId);
        writer.WriteString(nameof(Evaluation.Result), value.Result.ToString());
        writer.WriteString(nameof(Evaluation.Candidate), value.Candidate);
        writer.WriteString(nameof(Evaluation.Criteria), value.Criteria);
        writer.WriteString(nameof(Evaluation.Expected), value.Expected);
        writer.WriteString(nameof(Evaluation.Actual), value.Actual);
        writer.WriteString(nameof(Evaluation.Error), value.Error);
        writer.WriteEndObject();
    }
}