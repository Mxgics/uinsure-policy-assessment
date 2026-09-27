using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uinsure.Api.Policies;

// Enum.TryParse also accepts comma-separated combinations for non-flags enums.
public sealed class NamedEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var name = reader.GetString()?.Trim();
            foreach (var declared in Enum.GetNames<T>())
            {
                if (string.Equals(name, declared, StringComparison.OrdinalIgnoreCase))
                    return Enum.Parse<T>(declared);
            }
        }
        throw new JsonException($"Expected one named {typeof(T).Name} value.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        writer.WriteStringValue(Enum.GetName(value) ?? throw new JsonException("Undefined enum value."));
}
