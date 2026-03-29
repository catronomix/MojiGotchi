namespace MojiGotchi;

using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Pet))]
[JsonSerializable(typeof(Stat))]
[JsonSerializable(typeof(Color))]
[JsonSerializable(typeof(Vec2))]
[JsonSerializable(typeof(StatType))]
partial class PetJsonContext : JsonSerializerContext
{
}
