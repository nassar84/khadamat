using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Khadamat.WebAPI.Infrastructure;

/// <summary>
/// System.Text.Json does not natively support TimeSpan.
/// Blazor WASM serializes TimeSpan as "HH:mm:ss" strings.
/// This converter handles both string ("HH:mm:ss") and long (ticks) representations.
/// </summary>
public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (TimeSpan.TryParse(str, out var ts))
                return ts;
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            // Fallback: ticks as long
            return TimeSpan.FromTicks(reader.GetInt64());
        }

        return TimeSpan.Zero;
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(@"hh\:mm\:ss"));
    }
}
