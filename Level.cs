using System.IO;

namespace MojiGotchi;

// Defines the template for a level element. Each blueprint is mapped to a character
// in a level text file and contains all the information needed to create an instance of that element.
public class LevelElementBlueprint
{
	public string Name { get; }
	public Animation Animation { get; }
	public bool IsBlocking { get; }
	public string Layer { get; }

	public LevelElementBlueprint(string name, Animation animation, bool isBlocking, string layer)
	{
		Name = name;
		Animation = animation;
		IsBlocking = isBlocking;
		Layer = layer;
	}
}

// A static manager that holds all the defined LevelElementBlueprints.
public static class BlueprintManager
{
	private static readonly Dictionary<char, LevelElementBlueprint> _blueprints = new();


	// Initializes all the blueprints for the game. This should be called once at startup.
	public static void Initialize()
	{
		_blueprints.Clear();
		// Define blueprints here. Each character in your level file will map to one of these.

		// '═' horizontal wall
		MakeBlueprint1("WallH", '=', '═', Color.White, Color.DarkRed, "MidLayer", true, 1000);

		// '║' vertical wall
		MakeBlueprint1("WallV", '|', '║', Color.White, Color.DarkRed, "MidLayer", true, 1000);

		// '╔' topleft corner wall
		MakeBlueprint1("CornerTL", '<', '╔', Color.White, Color.DarkRed, "MidLayer", true, 1000);

		// '╗' topright corner wall
		MakeBlueprint1("CornerTR", '>', '╗', Color.White, Color.DarkRed, "MidLayer", true, 1000);

		// '╚' bottomleft corner wall
		MakeBlueprint1("CornerBL", '(', '╚', Color.White, Color.DarkRed, "MidLayer", true, 1000);

		// '╝' bottomright corner wall
		MakeBlueprint1("CornerBR", ')', '╝', Color.White, Color.DarkRed, "MidLayer", true, 1000);

		// '#' floor tile
		MakeBlueprint1("Floor", '#', '#', Color.DarkYellow, Color.Yellow, "BottomLayer", false, 1000);

		// '%' window
		MakeBlueprint1("Window", '%', '▒', Color.LightBlue, Color.Blue, "MidLayer", true, 1000);

		// 'D' door
		MakeBlueprint1("Door", 'D', '•', Color.Gray, Color.DarkRed, "BottomLayer", false, 1000);

		// ',' grass
		MakeBlueprint1("Grass", ',', new char[] { '\\', '|', '/' }, Color.GrassGreen, Color.GroundGreen, "BottomLayer", false, 1000);

		// 'B', 'b' bush
		MakeBlueprint1("MidBush", 'B', new char[] { '@', 'O' }, Color.BushGreen, Color.DarkGreen, "MidLayer", true, 500);
		MakeBlueprint1("TopBush", 'b', new char[] { '@', 'O' }, Color.BushGreen, Color.DarkGreen, "TopLayer", false, 500);

		// 'W', 'w' wood
		MakeBlueprint1("MidWood", 'W', '#', Color.WoodLight, Color.WoodDark, "MidLayer", true, 500);
		MakeBlueprint1("TopWood", 'w', '#', Color.WoodLight, Color.WoodDark, "TopLayer", false, 500);

		// '~' Water
		MakeBlueprint1("Water", '~', new char[] { '~', '-' }, Color.WaterLight, Color.WaterDark, "MidLayer", true, 400);
	}

	public static void MakeBlueprint1(string name, char key, char[] chars, Color fg, Color bg, string layer, bool blocking = false, int animtime = 500)
	{
		var anim = new Animation(animtime);
		foreach (char c in chars)
		{
			var sprite = new Sprite(new Vec2(1, 1));
			sprite.WriteCell(new Vec2(0, 0), new ScreenCell(c, fg, bg));
			anim.addFrame(sprite);
		}
		_blueprints.Add(key, new LevelElementBlueprint(name, anim, blocking, layer));
	}

	public static void MakeBlueprint1(string name, char key, char character, Color fg, Color bg, string layer, bool blocking = false, int animtime = 500)
	{
		MakeBlueprint1(name, key, new char[] { character }, fg, bg, layer, blocking, animtime);
	}


	public static LevelElementBlueprint? GetBlueprint(char key)
	{
		_blueprints.TryGetValue(key, out var blueprint);
		return blueprint;
	}
}

// Represents a level/room in the game, composed of various LevelElements organized into layers.
class Level
{
	public List<LevelElement> BottomLayer { get; private set; }
	public List<LevelElement> MidLayer { get; private set; }
	public List<LevelElement> TopLayer { get; private set; }
	public Sprite? BottomSprite { get; private set; }
	public Sprite? MidSprite { get; private set; }
	public Sprite? TopSprite { get; private set; }
	private Vec2 _size;
	public Vec2 Size
	{
		get
		{
			return _size;
		}
	}

	private Vec2 _relativeCenter;
	public Vec2 RelativeCenter
	{
		get
		{
			return _relativeCenter;
		}
	}

	//for randomizing animations
	Random randomizer = new Random();

	public Level()
	{
		BottomLayer = new List<LevelElement>();
		MidLayer = new List<LevelElement>();
		TopLayer = new List<LevelElement>();
	}

	public void LoadFromFile(string filePath, int borderv = 10, int borderh = 30)
	{
		DebugLogger.Log($"LoadFromFile called with path: {filePath}");
		if (File.Exists(filePath))
		{
			DebugLogger.Log($"Level file exists: {filePath}");
			Random randomBorder = new Random();
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var reader = new StreamReader(stream);
			List<string> lines = reader.ReadToEnd().Split('\n').ToList();
			DebugLogger.Log($"Level file loaded: {lines.Count} lines");
			
			//pad left and right
			for (int i = 0; i < lines.Count; i++)
			{
				for (int j = 0; j < borderh; j++)
				{
					string pre = randomBorder.Next(10) > 0 ? "B" : ",";
					string post = randomBorder.Next(10) > 0 ? "B" : ",";
					lines[i] = pre + lines[i] + post;
				}
			}

			//pad top and bottom
			
			for (int i = 0; i < borderv; i++)
			{
				string emptyline = "";
				for (int j = 0; j < lines[0].Length; j++)
				{
					emptyline += randomBorder.Next(10) > 0 ? "B" : ",";
				}
				lines.Insert(0, emptyline);
				lines.Add(emptyline);
			}
			
			//update size for padding
			_size.X = lines[0].Length;
			_size.Y = lines.Count;
			//set sprite sizes
			BottomSprite = new Sprite(_size);
			MidSprite = new Sprite(_size);
			TopSprite = new Sprite(_size);

			for (int y = 0; y < lines.Count; y++)
			{
				for (int x = 0; x < lines[y].Length; x++)
				{
					char key = lines[y][x];
					if (key == '.') continue; // dots are empty/transparent

					LevelElementBlueprint? blueprint = BlueprintManager.GetBlueprint(key);
					if (blueprint != null)
					{
						var element = new LevelElement(blueprint.IsBlocking);
						element.Name = blueprint.Name;
						element.Position = new Vec2(x, y); // Position in the level grid

						//offset animation on creation
						Animation animation = blueprint.Animation.Clone();
						animation.OffsetTime((float)randomizer.NextDouble()*1000.0f);

						element.Animations = new Dictionary<string, Animation> { { Entity.AnimDefault, animation } };

						// Add to the correct layer based on the blueprint
						if (blueprint.Layer == "BottomLayer")
						{
							BottomLayer.Add(element);
						}
						else if (blueprint.Layer == "MidLayer")
						{
							MidLayer.Add(element);
						}
						else if (blueprint.Layer == "TopLayer")
						{
							TopLayer.Add(element);
						}
					}
				}
			}
			//update world origin
			_relativeCenter = _size.Divide(2);
			DebugLogger.Log($"Level loaded successfully. Size: {_size.X}x{_size.Y}");
		}
		else
		{
			DebugLogger.Log($"Level file not found: {filePath}");
		}
	}

	//draw elements to layer sprite
	public static void SetSprite(List<LevelElement> layer, Sprite targetSprite)
	{
		// Iterate over each element in the provided layer
		foreach (var element in layer)
		{
			// Get the element's current sprite. This is dynamic and will get the correct
			// frame if the element is animated.
			Sprite? elementSprite = element.GetSprite();

			if (elementSprite != null)
			{
				// Get the element's top-left position within the level grid
				Vec2 elementPos = element.Position;

				// Iterate over the cells of the element's sprite
				for (int y = 0; y < elementSprite.Size.Y; y++)
				{
					for (int x = 0; x < elementSprite.Size.X; x++)
					{
						// Calculate the destination coordinates on the large target sprite
						int targetX = elementPos.X + x;
						int targetY = elementPos.Y + y;

						// Draw the cell onto the target sprite. The WriteCell method handles boundary checks.
						targetSprite.WriteCell(new Vec2(targetX, targetY), elementSprite.Data[y, x]);
					}
				}
			}
		}
	}
}

public class LevelElement : Entity
{
	public bool IsBlocking { get; }

	public LevelElement(bool blocking = false) : base()
	{
		IsBlocking = blocking;
	}
}