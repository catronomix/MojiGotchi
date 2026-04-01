namespace MojiGotchi;

// Defines the template for a level element. Each blueprint is mapped to a character
// in a level text file and contains all the information needed to create an instance of that element.
internal class LevelElementBlueprint
{
	internal string Name { get; }
	internal Animation Animation { get; }
	internal bool IsBlocking { get; }
	internal int Depth { get; }

	internal LevelElementBlueprint(string name, Animation animation, bool isBlocking, int depth)
	{
		Name = name;
		Animation = animation;
		IsBlocking = isBlocking;
		Depth = depth;
	}
}

// A static manager that holds all the defined LevelElementBlueprints.
internal static class BlueprintManager
{
	private static readonly Dictionary<char, LevelElementBlueprint> _blueprints = new();


	// Initializes all the blueprints for the game. This should be called once at startup.
	internal static void Initialize()
	{
		_blueprints.Clear();
		// Define blueprints here. Each character in your level file will map to one of these.

		// '═' horizontal wall
		MakeBlueprint1("WallH", '=', '═', Color.White, Color.DarkRed, 1, true, 1000);

		// '║' vertical wall
		MakeBlueprint1("WallV", '|', '║', Color.White, Color.DarkRed, 1, true, 1000);

		// '╔' topleft corner wall
		MakeBlueprint1("CornerTL", '<', '╔', Color.White, Color.DarkRed, 1, true, 1000);

		// '╗' topright corner wall
		MakeBlueprint1("CornerTR", '>', '╗', Color.White, Color.DarkRed, 1, true, 1000);

		// '╚' bottomleft corner wall
		MakeBlueprint1("CornerBL", '(', '╚', Color.White, Color.DarkRed, 1, true, 1000);

		// '╝' bottomright corner wall
		MakeBlueprint1("CornerBR", ')', '╝', Color.White, Color.DarkRed, 1, true, 1000);

		// '#' floor tile
		MakeBlueprint1("Floor", '#', '#', Color.DarkYellow, Color.Yellow, 0, false, 1000);

		// '%' window
		MakeBlueprint1("Window", '%', '▒', Color.LightBlue, Color.Blue, 1, true, 1000);

		// 'D' door
		MakeBlueprint1("Door", 'D', '•', Color.Gray, Color.DarkRed, 0, false, 1000);

		// ',' grass
		MakeBlueprint1("Grass", ',', new char[] { '\\', '|', '/' }, Color.GrassGreen, Color.GroundGreen, 0, false, 1000);

		// 'B', 'b' bush
		MakeBlueprint1("MidBush", 'B', new char[] { '@', 'O' }, Color.BushGreen, Color.DarkGreen, 1, true, 500);
		MakeBlueprint1("TopBush", 'b', new char[] { '@', 'O' }, Color.BushGreen, Color.DarkGreen, 2, false, 500);

		// 'W', 'w' wood
		MakeBlueprint1("MidWood", 'W', '#', Color.WoodLight, Color.WoodDark, 1, true, 500);
		MakeBlueprint1("TopWood", 'w', '#', Color.WoodLight, Color.WoodDark, 2, false, 500);

		// '~' Water
		MakeBlueprint1("Water", '~', new char[] { '~', '-' }, Color.WaterLight, Color.WaterDark, 1, true, 400);
	}

	internal static void MakeBlueprint1(string name, char key, char[] chars, Color fg, Color bg, int depth, bool blocking = false, int animtime = 500)
	{
		var anim = new Animation(animtime);
		foreach (char c in chars)
		{
			var sprite = new Sprite(new Vec2(1, 1));
			sprite.WriteCell(new Vec2(0, 0), new ScreenCell(c, fg, bg));
			anim.addFrame(sprite);
		}
		_blueprints.Add(key, new LevelElementBlueprint(name, anim, blocking, depth));
	}

	internal static void MakeBlueprint1(string name, char key, char character, Color fg, Color bg, int depth, bool blocking = false, int animtime = 500)
	{
		MakeBlueprint1(name, key, new char[] { character }, fg, bg, depth, blocking, animtime);
	}


	internal static LevelElementBlueprint? GetBlueprint(char key)
	{
		_blueprints.TryGetValue(key, out var blueprint);
		return blueprint;
	}
}

// Represents a level/room in the game, composed of various LevelElements organized into layers.
internal class Level
{
	internal LevelLayer[] Layers;
	internal static readonly string[] LayerNames = ["Bottom", "Mid", "Top"];
	
	private Vec2 _size;
	internal Vec2 Size
	{
		get
		{
			return _size;
		}
	}

	private Vec2 _relativeCenter;
	internal Vec2 RelativeCenter
	{
		get
		{
			return _relativeCenter;
		}
	}

	//for randomizing animations
	Random randomizer = new Random();

	internal Level()
	{
		Layers = new LevelLayer[3];
		_size = new Vec2(0, 0);
		_relativeCenter = new Vec2(0, 0);
	}

	internal void LoadFromFile(string filePath, int borderv = 10, int borderh = 30)
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

			//initialize layer arrays
			for(int i = 0; i < Layers.Length; i++)
			{
				Layers[i] = new LevelLayer(LayerNames[i], i, _size);
			}

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
						element.SetDepth(blueprint.Depth);

						//offset animation on creation
						Animation animation = blueprint.Animation.Clone();
						animation.OffsetTime((float)randomizer.NextDouble()*1000.0f);

						element.Animations = new Dictionary<string, Animation> { { Entity.AnimDefault, animation } };

						Layers[blueprint.Depth].Elements[x,y] = element;
					}
				}
			}
			//update world origin
			_relativeCenter = _size.Divide(2);

			// Level.SetSprite fills the layerSprite (which is level-sized)
			// Do this only when the sprite has changed
			foreach(LevelLayer layer in Layers)
			{
				layer.UpdateSprite();
			}

			DebugLogger.Log($"Level loaded successfully. Size: {_size.X}x{_size.Y}");
		}
		else
		{
			DebugLogger.Log($"Level file not found: {filePath}");
		}
	}

}

internal class LevelLayer
{
	internal LevelElement[,] Elements { get; private set; }
	internal string LayerName { get; private set; }
	internal Sprite? Sprite { get; private set; }
	internal int Depth { get; private set; }
	internal Vec2 Size { get; private set; }

	internal LevelLayer(string layername, int depth, Vec2 size)
	{
		Elements = new LevelElement[size.X, size.Y];
		LayerName = layername;
		Depth = depth;
		Size = size;
		Sprite = null;
	}

	//draw elements to layer sprite
	internal void UpdateSprite()
	{
		if (Elements == null)
		{
			DebugLogger.Log("Could not generate layer sprite: No elements in layer!");
			return;
		}

		Sprite targetsprite = new Sprite(new Vec2(Elements.GetLength(0), Elements.GetLength(1)));

		// Iterate over each element in the provided layer
		foreach (var element in Elements)
		{
			//skip non-existing elements
			if (element == null)
			{
				continue;
			}
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
						// Calculate the destination coordinates on the layer sprite
						int targetX = elementPos.X + x;
						int targetY = elementPos.Y + y;

						// Draw the cell onto the target sprite. The WriteCell method handles boundary checks.
						targetsprite.WriteCell(new Vec2(targetX, targetY), elementSprite.Data[y, x]);
					}
				}
			}
		}
		Sprite = targetsprite;
	}
}

internal class LevelElement : Entity
{
	internal bool IsBlocking { get; set;}
	internal int Depth { get; private set; }

	internal LevelElement(bool blocking = false, int depth = 1) : base()
	{
		IsBlocking = blocking;
		Depth = depth;
	}

	internal void SetDepth(int depth)
	{
		Depth = Math.Clamp(depth, 0, 2);
	}
}