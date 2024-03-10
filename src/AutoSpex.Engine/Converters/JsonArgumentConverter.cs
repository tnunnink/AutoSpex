﻿using System.Text.Json;
using System.Text.Json.Serialization;
using L5Sharp.Core;

namespace AutoSpex.Engine.Converters;

public class JsonArgumentConverter : JsonConverter<Argument>
{
    public override Argument? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Type? type = default;
        object? value = default;

        while (reader.Read() & reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case nameof(Argument.Type):
                    type = JsonSerializer.Deserialize<Type>(ref reader, options);
                    break;
                case nameof(Argument.Value):
                    if (type is null)
                        throw new JsonException("Can not deserialize Argument without a known type.");
                    
                    //These are the only supported complex types argument should contain.
                    if (type == typeof(Variable) || type == typeof(Criterion))
                    {
                        value = JsonSerializer.Deserialize(ref reader, type, options);
                    }
                    //Everything else should be text which we will parse using L5Sharp.
                    else
                    {
                        var text = reader.GetString();
                        value = text?.Parse(type);
                    }
                    
                    break;
            }
        }

        if (type is null)
            throw new JsonException("Conversion failed to deserialize Type property for Argument");
        
        if (value is null)
            throw new JsonException("Conversion failed to deserialize Value property for Argument");
        
        return new Argument(value);
    }

    public override void Write(Utf8JsonWriter writer, Argument value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(Argument.Type), value.Type.FullName);

        if (value.Type == typeof(Variable) || value.Type == typeof(Criterion))
        {
            writer.WritePropertyName(nameof(Argument.Value));
            JsonSerializer.Serialize(writer, value.Value, value.Type, options);    
        }
        else
        {
            writer.WriteString(nameof(Argument.Value), value.Value.ToString());
        }
        
        writer.WriteEndObject();
    }
}