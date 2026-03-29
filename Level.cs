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
		var wallSpriteH = new Sprite(new Vec2(1, 1));
		wallSpriteH.WriteCell(new Vec2(0, 0), new ScreenCell('═', Color.White, Color.DarkRed));
		var wallAnimationH = new Animation(1000);
		wallAnimationH.addFrame(wallSpriteH);
		_blueprints.Add('=', new LevelElementBlueprint("WallH", wallAnimationH, true, "MidLayer"));

		// '║' vertical wall
		var wallSpriteV = new Sprite(new Vec2(1, 1));
		wallSpriteV.WriteCell(new Vec2(0, 0), new ScreenCell('║', Color.White, Color.DarkRed));
		var wallAnimationV = new Animation(1000);
		wallAnimationV.addFrame(wallSpriteV);
		_blueprints.Add('|', new LevelElementBlueprint("WallV", wallAnimationV, true, "MidLayer"));

		// '╔' topleft corner wall
		var cornerSpriteTL = new Sprite(new Vec2(1, 1));
		cornerSpriteTL.WriteCell(new Vec2(0, 0), new ScreenCell('╔', Color.White, Color.DarkRed));
		var cornerAnimationTL = new Animation(1000);
		cornerAnimationTL.addFrame(cornerSpriteTL);
		_blueprints.Add('<', new LevelElementBlueprint("CornerTL", cornerAnimationTL, true, "MidLayer"));

		// '╗' topright corner wall
		var cornerSpriteTR = new Sprite(new Vec2(1, 1));
		cornerSpriteTR.WriteCell(new Vec2(0, 0), new ScreenCell('╗', Color.White, Color.DarkRed));
		var cornerAnimationTR = new Animation(1000);
		cornerAnimationTR.addFrame(cornerSpriteTR);
		_blueprints.Add('>', new LevelElementBlueprint("CornerTR", cornerAnimationTR, true, "MidLayer"));

		// '╚' bottomleft corner wall
		var cornerSpriteBL = new Sprite(new Vec2(1, 1));
		cornerSpriteBL.WriteCell(new Vec2(0, 0), new ScreenCell('╚', Color.White, Color.DarkRed));
		var cornerAnimationBL = new Animation(1000);
		cornerAnimationBL.addFrame(cornerSpriteBL);
		_blueprints.Add('(', new LevelElementBlueprint("CornerBL", cornerAnimationBL, true, "MidLayer"));

		// '╝' bottomright corner wall
		var cornerSpriteBR = new Sprite(new Vec2(1, 1));
		cornerSpriteBR.WriteCell(new Vec2(0, 0), new ScreenCell('╝', Color.White, Color.DarkRed));
		var cornerAnimationBR = new Animation(1000);
		cornerAnimationBR.addFrame(cornerSpriteBR);
		_blueprints.Add(')', new LevelElementBlueprint("CornerBR", cornerAnimationBR, true, "MidLayer"));

		// '#' floor tile
		var floorSprite = new Sprite(new Vec2(1, 1));
		floorSprite.WriteCell(new Vec2(0, 0), new ScreenCell('#', Color.DarkYellow, Color.Yellow));
		var floorAnimation = new Animation(1000);
		floorAnimation.addFrame(floorSprite);
		_blueprints.Add('#', new LevelElementBlueprint("Floor", floorAnimation, false, "BottomLayer"));

		// '%' window
		var windowSprite = new Sprite(new Vec2(1, 1));
		windowSprite.WriteCell(new Vec2(0, 0), new ScreenCell('▒', Color.LightBlue, Color.Blue));
		var windowAnimation = new Animation(1000);
		windowAnimation.addFrame(windowSprite);
		_blueprints.Add('%', new LevelElementBlueprint("Window", windowAnimation, true, "MidLayer"));

		// 'D' door
		var doorSprite = new Sprite(new Vec2(1, 1));
		doorSprite.WriteCell(new Vec2(0, 0), new ScreenCell('•', Color.Gray, Color.DarkRed));
		var doorAnimation = new Animation(1000);
		doorAnimation.addFrame(doorSprite);
		_blueprints.Add('D', new LevelElementBlueprint("Door", doorAnimation, false, "BottomLayer"));

		// ',' grass
		var grassAnimation = new Animation(1000);
		var grassSprite1 = new Sprite(new Vec2(1, 1));
		grassSprite1.WriteCell(new Vec2(0, 0), new ScreenCell('\\', Color.GrassGreen, Color.GroundGreen));
		grassAnimation.addFrame(grassSprite1);
		var grassSprite2 = new Sprite(new Vec2(1, 1));
		grassSprite2.WriteCell(new Vec2(0, 0), new ScreenCell('|', Color.GrassGreen, Color.GroundGreen));
		grassAnimation.addFrame(grassSprite2);
		var grassSprite3 = new Sprite(new Vec2(1, 1));
		grassSprite3.WriteCell(new Vec2(0, 0), new ScreenCell('/', Color.GrassGreen, Color.GroundGreen));
		grassAnimation.addFrame(grassSprite3);
		_blueprints.Add(',', new LevelElementBlueprint("Grass", grassAnimation, false, "BottomLayer"));

		// 'B', 'b' bush
		var bushAnimation = new Animation(500);
		var bushSprite1 = new Sprite(new Vec2(1, 1));
		bushSprite1.WriteCell(new Vec2(0, 0), new ScreenCell('@', Color.BushGreen, Color.DarkGreen));
		bushAnimation.addFrame(bushSprite1);
		var bushSprite2 = new Sprite(new Vec2(1, 1));
		bushSprite2.WriteCell(new Vec2(0, 0), new ScreenCell('O', Color.BushGreen, Color.DarkGreen));
		bushAnimation.addFrame(bushSprite2);
		_blueprints.Add('B', new LevelElementBlueprint("MidBush", bushAnimation, true, "MidLayer"));
		_blueprints.Add('b', new LevelElementBlueprint("TopBush", bushAnimation, false, "TopLayer"));

		// 'W', 'w' wood
		var WoodAnim = new Animation(500);
		var WoodSprite = new Sprite(new Vec2(1, 1));
		WoodSprite.WriteCell(new Vec2(0, 0), new ScreenCell('#', Color.WoodLight, Color.WoodDark));
		WoodAnim.addFrame(WoodSprite);
		_blueprints.Add('W', new LevelElementBlueprint("MidWood", WoodAnim, true, "MidLayer"));
		_blueprints.Add('w', new LevelElementBlueprint("TopWood", WoodAnim, false, "TopLayer"));

		// '~' Water
		var WaterAnim = new Animation(400);
		var WaterSprite1 = new Sprite(new Vec2(1, 1));
		WaterSprite1.WriteCell(new Vec2(0, 0), new ScreenCell('~', Color.WaterLight, Color.WaterDark));
		WaterAnim.addFrame(WaterSprite1);
		var WaterSprite2 = new Sprite(new Vec2(1, 1));
		WaterSprite2.WriteCell(new Vec2(0, 0), new ScreenCell('-', Color.WaterLight, Color.WaterDark));
		WaterAnim.addFrame(WaterSprite2);
		_blueprints.Add('~', new LevelElementBlueprint("Water", WaterAnim, true, "MidLayer"));


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
			List<string> lines = File.ReadAllLines(filePath).ToList();
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