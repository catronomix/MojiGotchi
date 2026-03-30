using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public class OptionsJsonContext
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new DictionaryConverter() }
    };

    public static string Serialize(Dictionary<string, string> options)
    {
        return JsonSerializer.Serialize(options, _options);
    }

    public static Dictionary<string, string> Deserialize(string json)
    {
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json, _options) ?? new Dictionary<string, string>();
    }
}

public class DictionaryConverter : JsonConverter<Dictionary<string, string>>
{
    public override Dictionary<string, string> Read(ref Utf8JsonReader reader, typeof(Dictionary<string, string>), JsonSerializerOptions options)
    {
        var dict = new Dictionary<string, string>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return dict;

            var key = reader.GetString();
            reader.Read();
            var value = reader.GetString();
            dict[key] = value;
        }
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, string> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            writer.WriteString(kvp.Key, kvp.Value);
        }
        writer.WriteEndObject();
    }
}
