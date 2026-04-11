using System.Text.Json;
using System.Text.Json.Serialization;

namespace MojiGotchi;

// Defines the template for a level element. Each blueprint is mapped to a character
// in a level text file and contains all the information needed to create an instance of that element.
public class LevelElementBlueprint
{
	public string Name { get; }
	public Animation Animation { get; }
	public bool IsBlocking { get; }

	public LevelElementBlueprint(string name, Animation animation, bool isBlocking)
	{
		Name = name;
		Animation = animation;
		IsBlocking = isBlocking;
	}
}

// A static manager that holds all the defined LevelElementBlueprints.
public static class BlueprintManager
{
	private static readonly Dictionary<string, LevelElementBlueprint> _blueprints = new();
	public static int NumItems => _blueprints.Count;
	private const int NOANIM = int.MaxValue;

	// Initializes all the blueprints for the game. This should be called once at startup.
	public static void Initialize()
	{
		_blueprints.Clear();

		// All keys MUST be 2 characters to maintain level file alignment!
		//walls
		MakeBlueprint1("WallH", "==", '═', Color.Wallfg, Color.Wallbg, true, NOANIM);
		MakeBlueprint1("WallV", "=|", '║', Color.Wallfg, Color.Wallbg, true, NOANIM);
		MakeBlueprint1("WallTL", "=<", '╔', Color.Wallfg, Color.Wallbg, true, NOANIM);
		MakeBlueprint1("WallTR", "=>", '╗', Color.Wallfg, Color.Wallbg, true, NOANIM);
		MakeBlueprint1("WallBL", "=(", '╚', Color.Wallfg, Color.Wallbg, true, NOANIM);
		MakeBlueprint1("WallBR", "=)", '╝', Color.Wallfg, Color.Wallbg, true, NOANIM);
		MakeBlueprint1("WallJT", "=T", '╩', Color.Wallfg, Color.Wallbg, true, NOANIM);
		MakeBlueprint1("WallJR", "=R", '╠', Color.Wallfg, Color.Wallbg, true, NOANIM);
		MakeBlueprint1("WallJD", "=D", '╦', Color.Wallfg, Color.Wallbg, true, NOANIM);
		MakeBlueprint1("WallJL", "=L", '╣', Color.Wallfg, Color.Wallbg, true, NOANIM);
		MakeBlueprint1("WallX", "=X", '╬', Color.Wallfg, Color.Wallbg, true, NOANIM);

		//house elements
		MakeBlueprint1("Window", "%%", '▒', Color.LightBlue, Color.Blue, true, NOANIM);
		MakeBlueprint1("Door", "DD", '•', Color.Gray, Color.DarkRed, false, NOANIM);
		MakeBlueprint1("Dark Plank", "PD", '╱', Color.WoodLight, Color.WoodDark, true, NOANIM);
		MakeBlueprint1("Light Plank", "PL", '╱', Color.WoodDark, Color.WoodLight, true, NOANIM);
		MakeBlueprint1("Red Tile", "TR", '┼', Color.DarkRed, Color.Red, true, NOANIM);
		MakeBlueprint1("Magenta Tile", "TM", '╳', Color.DarkMagenta, Color.Magenta, true, NOANIM);
		MakeBlueprint1("Yellow Tile", "TY", '#', Color.DarkYellow, Color.Yellow, true, NOANIM);
		MakeBlueprint1("Red Carpet", "CR", '░', Color.Red, Color.DarkRed, false, NOANIM);
		MakeBlueprint1("Cyan Carpet", "CC", '▒', Color.LightCyan, Color.Cyan, false, NOANIM);
		MakeBlueprint1("Orange Carpet", "CO", '▓', Color.Orange, Color.LightOrange, false, NOANIM);

		//nature
		MakeBlueprint1("Grass", ",,", new char[] {'/', '|', '|', '|', '/', '|', '|', '|', '|', '|' }, Color.GrassGreen, Color.GroundGreen, false, 330);
		MakeBlueprint1("Clover", ",♣", '♣', Color.GrassGreen, Color.GroundGreen, false, NOANIM);
		MakeBlueprint1("Stem", "BS", '¥', Color.WoodLight, Color.DarkGreen, true, NOANIM);
		MakeBlueprint1("Stalk", "Bs", '¥', Color.WoodLight, Color.GroundGreen, true, NOANIM);
		MakeBlueprint1("Bush", "BB", new char[] { '@', '@', 'O', '@', '@', '0' }, Color.BushGreen, Color.DarkGreen, true, 700);
		MakeBlueprint1("Apple", "Ba", '@', Color.Red, Color.DarkGreen, true, NOANIM);
		MakeBlueprint1("Berry", "Bb", '•', Color.Purple, Color.DarkGreen, true, NOANIM);
		MakeBlueprint1("Dark Wood", "WD", '#', Color.WoodLight, Color.WoodDark, true, NOANIM);
		MakeBlueprint1("Light Wood", "WL", '#', Color.WoodDark, Color.WoodLight, true, NOANIM);
		MakeBlueprint1("Water", "~~", new char[] { '~', '-' }, Color.WaterDark, Color.WaterLight, true, 400);
		MakeBlueprint1("Deep Water", "~D", new char[] { '~', '-'}, Color.WaterLight, Color.WaterDark, true, 500);
		MakeBlueprint1("Smiley", ":)", '☺', Color.Black, Color.Yellow, true, NOANIM);
		MakeBlueprint1("Yellow Flower", "FY", '@', Color.Yellow, Color.GroundGreen, true, NOANIM);
		MakeBlueprint1("Red Flower", "FR", '@', Color.Red, Color.GroundGreen, true, NOANIM);
		MakeBlueprint1("Blue Flower", "FB", '@', Color.LightBlue, Color.GroundGreen, true, NOANIM);
		MakeBlueprint1("Purple Flower", "FP", '@', Color.Purple, Color.GroundGreen, true, NOANIM);

		//solids
		MakeBlueprint1("Solid White", "#W", ' ', Color.Black, Color.White, true, NOANIM);
		MakeBlueprint1("Solid Black", "#b", ' ', Color.White, Color.Black, true, NOANIM);
		MakeBlueprint1("Solid Gray", "#g", ' ', Color.DarkGray, Color.Gray, true, NOANIM);
		MakeBlueprint1("Solid Blue", "#B", ' ', Color.Blue, Color.LightBlue, true, NOANIM);
		MakeBlueprint1("Solid Cyan", "#C", ' ', Color.Cyan, Color.LightCyan, true, NOANIM);
		MakeBlueprint1("Solid Green", "#G", ' ', Color.Green, Color.LightGreen, true, NOANIM);
		MakeBlueprint1("Solid Orange", "#O", ' ', Color.Orange, Color.LightOrange, true, NOANIM);
		MakeBlueprint1("Solid Red", "#R", ' ', Color.Red, Color.LightRed, true, NOANIM);
		MakeBlueprint1("Solid Yellow", "#Y", ' ', Color.Yellow, Color.LightYellow, true, NOANIM);

	}

	public static void MakeBlueprint1(string name, string key, char[] chars, Color fg, Color bg, bool blocking = false, int animtime = 500)
	{
		var anim = new Animation(animtime);
		foreach (char c in chars)
		{
			var sprite = new Sprite(new Vec2(1, 1));
			sprite.WriteCell(new Vec2(0, 0), new ScreenCell(c, fg, bg));
			anim.addFrame(sprite);
		}
		_blueprints.Add(key, new LevelElementBlueprint(name, anim, blocking));
	}

	public static void MakeBlueprint1(string name, string key, char character, Color fg, Color bg, bool blocking = false, int animtime = 500)
	{
		MakeBlueprint1(name, key, new char[] { character }, fg, bg, blocking, animtime);
	}

	public static LevelElementBlueprint? GetBlueprint(string key)
	{
		_blueprints.TryGetValue(key, out var blueprint);
		return blueprint;
	}

	public static LevelElement? GetElement(string key, Vec2 pos)
	{
		LevelElementBlueprint? blueprint = GetBlueprint(key);
		if (blueprint != null)
		{
			var element = new LevelElement(key, blueprint.IsBlocking)
			{
				Name = blueprint.Name,
				Pos = new Vec2(pos.X, pos.Y)
			};

			Animation animation = blueprint.Animation.Clone();
			animation.OffsetTime((float)Randomizer.R().NextDouble() * 1000.0f);
			element.Animations = new Dictionary<string, Animation> { { Entity.AnimDefault, animation } };
			return element;
		}
		return null;
	}

	public static Dictionary<string, LevelElement> GetBlueprintElements(int start = 0, int count = -1)
	{
		var result = new Dictionary<string, LevelElement>();
		var keys = _blueprints.Keys.ToList();

		int actualCount = count == -1 ? keys.Count - start : count;
		int end = Math.Min(start + actualCount, keys.Count);

		for (int i = start; i < end; i++)
		{
			string key = keys[i];
			var bp = _blueprints[key];
			var element = new LevelElement(key, bp.IsBlocking)
			{
				Name = bp.Name,
				Animations = new Dictionary<string, Animation> { { Entity.AnimDefault, bp.Animation } }
			};
			result.Add(key, element);
		}
		return result;
	}
}

class Level
{
	public LevelLayer[] Layers { get; private set; }
	public static readonly string[] LayerNames = { "Bottom", "Mid", "Top" };

	private Vec2 _size;
	public Vec2 Size => _size;

	private Vec2 _relativeCenter;
	public Vec2 RelativeCenter => _relativeCenter;

	public Level()
	{
		Layers = new LevelLayer[3];
		_size = new Vec2(0, 0);
		_relativeCenter = new Vec2(0, 0);
	}

	public void SetCell(string? key, Vec2 pos, int depth = -1)
	{
		// Default to ".." for null keys to maintain grid alignment
		LevelElement? element = BlueprintManager.GetElement(key ?? "..", pos);
		int layerIndex = depth == -1 ? 0 : Math.Clamp(depth, 0, Layers.Length - 1);
		
		if (pos.X >= 0 && pos.X < _size.X && pos.Y >= 0 && pos.Y < _size.Y)
		{
			Layers[layerIndex].Elements[pos.X, pos.Y] = element;
		}
	}

	public void LoadFromFile(string filePath)
	{
		try
		{
			if (!File.Exists(filePath))
			{
				DebugLogger.Log($"Level file not found! ({filePath})");
				CreateNewLevel(100, 40);
				return;
			}

			string json = File.ReadAllText(filePath);
			var dto = JsonSerializer.Deserialize<LevelDataDto>(json);
			if (dto == null) return;

			_size = new Vec2(dto.Width, dto.Height);
			_relativeCenter = _size.Divide(2);
			
			for (int i = 0; i < Layers.Length; i++)
			{
				Layers[i] = new LevelLayer(LayerNames[i], i, _size);
			}

			foreach (var layerDto in dto.Layers)
			{
				int layerIndex = Array.IndexOf(LayerNames, layerDto.Name);
				if (layerIndex == -1) continue;

				for (int y = 0; y < Math.Min(layerDto.Data.Count, _size.Y); y++)
				{
					string row = layerDto.Data[y];
					for (int x = 0; x < _size.X; x++)
					{
						int charIndex = x * 2;
						if (charIndex + 1 >= row.Length) break;

						string cell = row.Substring(charIndex, 2);
						if (cell != "..")
						{
							SetCell(cell, new Vec2(x, y), layerIndex);
						}
					}
				}
			}

			foreach (var layer in Layers) layer.UpdateSprite();
			DebugLogger.Log($"Loaded level: {filePath}");
		}
		catch (Exception ex)
		{
			DebugLogger.Log($"Error loading level: {ex.Message}");
		}
	}

	public bool SaveToFile(string filePath)
	{
		try
		{
			string? directory = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

			var jsonBuilder = new System.Text.StringBuilder();
			jsonBuilder.AppendLine("{");
			jsonBuilder.AppendLine($"  \"width\": {Size.X},");
			jsonBuilder.AppendLine($"  \"height\": {Size.Y},");
			jsonBuilder.AppendLine("  \"layers\": [");

			for (int i = 0; i < Layers.Length; i++)
			{
				LevelLayer layer = Layers[i];
				jsonBuilder.AppendLine("    {");
				jsonBuilder.AppendLine($"      \"name\": \"{layer.LayerName}\",");
				jsonBuilder.AppendLine("      \"data\": [");

				for (int y = 0; y < Size.Y; y++)
				{
					var rowBuilder = new System.Text.StringBuilder();
					for (int x = 0; x < Size.X; x++)
					{
						// Alignment Check: Always use 2 characters
						LevelElement? element = layer.Elements[x, y];
						rowBuilder.Append(element?.Key ?? "..");
					}
					jsonBuilder.AppendLine($"        \"{rowBuilder}\"{(y < Size.Y - 1 ? "," : "")}");
				}
				jsonBuilder.AppendLine("      ]");
				jsonBuilder.AppendLine($"    }}{(i < Layers.Length - 1 ? "," : "")}");
			}

			jsonBuilder.AppendLine("  ]");
			jsonBuilder.AppendLine("}");

			File.WriteAllText(filePath, jsonBuilder.ToString());
			return true;
		}
		catch (Exception ex)
		{
			DebugLogger.Log($"Error saving level: {ex.Message}");
			return false;
		}
	}

	public void CreateNewLevel(int width, int height)
	{
		_size = new Vec2(width, height);
		_relativeCenter = _size.Divide(2);
		Layers = new LevelLayer[3];
		for (int i = 0; i < Layers.Length; i++)
		{
			Layers[i] = new LevelLayer(LayerNames[i], i, _size);
		}
	}

	public void PadLevel(int borderh, int borderv)
	{
		Vec2 newSize = new Vec2(_size.X + (borderh * 2), _size.Y + (borderv * 2));
		
		for (int i = 0; i < Layers.Length; i++)
		{
			LevelLayer oldLayer = Layers[i];
			var newElements = new LevelElement?[newSize.X, newSize.Y];
			for (int y = 0; y < newSize.Y; y++)
			{
				for (int x = 0; x < newSize.X; x++)
				{
					int oldX = x - borderh;
					int oldY = y - borderv;

					if (oldX >= 0 && oldX < _size.X && oldY >= 0 && oldY < _size.Y)
					{
						newElements[x, y] = oldLayer.Elements[oldX, oldY];
						if (newElements[x, y] != null) newElements[x, y]!.Pos = new Vec2(x, y);
					}
					else if (i == 1) // Only pad the middle layer with terrain
					{
						string padKey = Randomizer.R().Next(10) > 0 ? "BB" : ",,";
						newElements[x, y] = BlueprintManager.GetElement(padKey, new Vec2(x, y));
					}
				}
			}
			oldLayer.Elements = newElements;
			oldLayer.UpdateSprite();
		}

		_size = newSize;
		_relativeCenter = _size.Divide(2);
	}
}

public class LevelLayer
{
	public LevelElement?[,] Elements { get; set; }
	public string LayerName { get; private set; }
	public Sprite? Sprite { get; private set; }
	public int Depth { get; private set; }
	public Vec2 Size { get; private set; }

	public LevelLayer(string layername, int depth, Vec2 size)
	{
		Elements = new LevelElement[size.X, size.Y];
		LayerName = layername;
		Depth = depth;
		Size = size;
		Sprite = null;
	}

	public void UpdateSprite(float shade = 0.0f)
	{
		Sprite targetsprite = new Sprite(new Vec2(Elements.GetLength(0), Elements.GetLength(1)));

		foreach (var element in Elements)
		{
			if (element == null) continue;
			Sprite? elementSprite = element.GetSprite();

			if (elementSprite != null)
			{
				Vec2 elementPos = element.Pos;
				for (int y = 0; y < elementSprite.Size.Y; y++)
				{
					for (int x = 0; x < elementSprite.Size.X; x++)
					{
						int targetX = elementPos.X + x;
						int targetY = elementPos.Y + y;

						ScreenCell cell = elementSprite.Data[y, x];
						if (shade > 0)
						{
							cell.BgColor = Color.Mix(cell.BgColor, Color.Black, shade);
							cell.Color = Color.Mix(cell.Color, Color.Black, shade);
						}
						targetsprite.WriteCell(new Vec2(targetX, targetY), cell);
					}
				}
			}
		}
		Sprite = targetsprite;
	}
}

public class LevelElement : Entity
{
	public bool IsBlocking { get; set; }
	public string Key { get; private set; }

	public LevelElement(string key = "..", bool blocking = false) : base()
	{
		IsBlocking = blocking;
		Key = key;
	}
}

internal class LevelDataDto
{
	[JsonPropertyName("width")] public int Width { get; set; }
	[JsonPropertyName("height")] public int Height { get; set; }
	[JsonPropertyName("layers")] public List<LayerDataDto> Layers { get; set; } = new();
}

internal class LayerDataDto
{
	[JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
	[JsonPropertyName("data")] public List<string> Data { get; set; } = new();
}