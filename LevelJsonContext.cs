using System.Text.Json.Serialization;

namespace MojiGotchi;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(LevelDataDto))]
[JsonSerializable(typeof(LayerDataDto))]
partial class LevelJsonContext : JsonSerializerContext
{
}