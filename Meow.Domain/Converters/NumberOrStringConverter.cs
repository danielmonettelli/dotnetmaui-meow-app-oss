using System.Text.Json;
using System.Text.Json.Serialization;

namespace Meow.Domain.Converters;

/// <summary>
/// A JSON converter that reads either a JSON number or string and outputs a string.
/// This prevents deserialization failures when APIs (like TheCatAPI) return numeric IDs.
/// </summary>
public class NumberOrStringConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.TryGetInt64(out var longVal)
                ? longVal.ToString()
                : reader.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unexpected token {reader.TokenType} when parsing string or number.")
        };
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value);
        }
    }
}
