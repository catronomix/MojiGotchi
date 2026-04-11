
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class OptionsJsonContext : JsonSerializerContext
{
}

public static class OptionsJsonContextHelper
{
	public static string Serialize(Dictionary<string, string> options)
	{
		var context = new OptionsJsonContext();
		var typeInfo = context.GetTypeInfo(typeof(Dictionary<string, string>)) as JsonTypeInfo<Dictionary<string, string>>;
		if (typeInfo == null)
			throw new InvalidOperationException("Failed to get type info for Dictionary<string, string>");
		return JsonSerializer.Serialize(options, typeInfo);
	}

	public static Dictionary<string, string> Deserialize(string json)
	{
		var context = new OptionsJsonContext();
		var typeInfo = context.GetTypeInfo(typeof(Dictionary<string, string>)) as JsonTypeInfo<Dictionary<string, string>>;
		if (typeInfo == null)
			throw new InvalidOperationException("Failed to get type info for Dictionary<string, string>");
		return JsonSerializer.Deserialize(json, typeInfo) ?? new Dictionary<string, string>();
	}
}
