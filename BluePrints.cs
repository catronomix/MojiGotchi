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
		MakeBlueprint1("Grass", ",,", new char[] { '/', '|', '|', '|', '/', '|', '|', '|', '|', '|' }, Color.GrassGreen, Color.GroundGreen, false, 330);
		MakeBlueprint1("Clover", ",♣", '♣', Color.GrassGreen, Color.GroundGreen, false, NOANIM);
		MakeBlueprint1("Stem", "BS", '¥', Color.WoodLight, Color.DarkGreen, true, NOANIM);
		MakeBlueprint1("Stalk", "Bs", '¥', Color.WoodLight, Color.GroundGreen, true, NOANIM);
		MakeBlueprint1("Bush", "BB", new char[] { '@', '@', 'O', '@', '@', '0' }, Color.BushGreen, Color.DarkGreen, true, 700);
		MakeBlueprint1("Apple", "Ba", '@', Color.Red, Color.DarkGreen, true, NOANIM);
		MakeBlueprint1("Berry", "Bb", '•', Color.Purple, Color.DarkGreen, true, NOANIM);
		MakeBlueprint1("Dark Wood", "WD", '#', Color.WoodLight, Color.WoodDark, true, NOANIM);
		MakeBlueprint1("Light Wood", "WL", '#', Color.WoodDark, Color.WoodLight, true, NOANIM);
		MakeBlueprint1("Water", "~~", new char[] { '~', '-' }, Color.WaterDark, Color.WaterLight, true, 400);
		MakeBlueprint1("Deep Water", "~D", new char[] { '~', '-' }, Color.WaterLight, Color.WaterDark, true, 500);
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
