namespace MojiGotchi;

using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Pet))]
[JsonSerializable(typeof(Stat))]
[JsonSerializable(typeof(Color))]
[JsonSerializable(typeof(Vec2))]
[JsonSerializable(typeof(Level))]
[JsonSerializable(typeof(LevelLayer))]
[JsonSerializable(typeof(LevelElement))]
[JsonSerializable(typeof(StatType))]
partial class PetJsonContext : JsonSerializerContext
{
}
