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
    private static readonly Dictionary<char, LevelElementBlueprint> _blueprints = new();

    public static int NumItems
    {
        get
        {
            return _blueprints.Count;
        }
    }

    // Initializes all the blueprints for the game. This should be called once at startup.
    public static void Initialize()
    {
        _blueprints.Clear();
        // Define blueprints here. Each character in your level file will map to one of these.

        // '═' horizontal wall
        MakeBlueprint1("WallH", '=', '═', Color.White, Color.DarkRed, true, 1000);

        // '║' vertical wall
        MakeBlueprint1("WallV", '|', '║', Color.White, Color.DarkRed, true, 1000);

        // '╔' topleft corner wall
        MakeBlueprint1("CornerTL", '<', '╔', Color.White, Color.DarkRed, true, 1000);

        // '╗' topright corner wall
        MakeBlueprint1("CornerTR", '>', '╗', Color.White, Color.DarkRed, true, 1000);

        // '╚' bottomleft corner wall
        MakeBlueprint1("CornerBL", '(', '╚', Color.White, Color.DarkRed, true, 1000);

        // '╝' bottomright corner wall
        MakeBlueprint1("CornerBR", ')', '╝', Color.White, Color.DarkRed, true, 1000);

        // '#' floor tile
        MakeBlueprint1("Floor", '#', '#', Color.DarkYellow, Color.Yellow, false, 1000);

        // '%' window
        MakeBlueprint1("Window", '%', '▒', Color.LightBlue, Color.Blue, true, 1000);

        // 'D' door
        MakeBlueprint1("Door", 'D', '•', Color.Gray, Color.DarkRed, false, 1000);

        // ',' grass
        MakeBlueprint1("Grass", ',', new char[] { '\\', '|', '/' }, Color.GrassGreen, Color.GroundGreen, false, 1000);

        // 'B', 'b' bush
        MakeBlueprint1("MidBush", 'B', new char[] { '@', 'O' }, Color.BushGreen, Color.DarkGreen, true, 500);
        MakeBlueprint1("TopBush", 'b', new char[] { '@', 'O' }, Color.BushGreen, Color.DarkGreen, false, 500);

        // 'W', 'w' wood
        MakeBlueprint1("MidWood", 'W', '#', Color.WoodLight, Color.WoodDark, true, 500);
        MakeBlueprint1("TopWood", 'w', '#', Color.WoodLight, Color.WoodDark, false, 500);

        // '~' Water
        MakeBlueprint1("Water", '~', new char[] { '~', '-' }, Color.WaterLight, Color.WaterDark, true, 400);
    }

    public static void MakeBlueprint1(string name, char key, char[] chars, Color fg, Color bg, bool blocking = false, int animtime = 500)
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

    public static void MakeBlueprint1(string name, char key, char character, Color fg, Color bg, bool blocking = false, int animtime = 500)
    {
        MakeBlueprint1(name, key, new char[] { character }, fg, bg, blocking, animtime);
    }

    public static LevelElementBlueprint? GetBlueprint(char key)
    {
        _blueprints.TryGetValue(key, out var blueprint);
        return blueprint;
    }

    public static LevelElement? GetElement(char key, Vec2 pos)
    {
        LevelElementBlueprint? blueprint = GetBlueprint(key);
        if (blueprint != null)
        {
            var element = new LevelElement(key, blueprint.IsBlocking);
            element.Name = blueprint.Name;
            element.Pos = new Vec2(pos.X, pos.Y); // Position in the level grid

            //offset animation on creation
            Animation animation = blueprint.Animation.Clone();
            animation.OffsetTime((float)Randomizer.R().NextDouble() * 1000.0f);

            element.Animations = new Dictionary<string, Animation> { { Entity.AnimDefault, animation } };
            return element;
        }
        return null;
    }

    public static Dictionary<char, LevelElement> GetBlueprintElements(int start = 0, int count = -1)
    {
        var result = new Dictionary<char, LevelElement>();
        var keys = _blueprints.Keys.ToList();

        int actualCount = count == -1 ? keys.Count - start : count;
        int end = Math.Min(start + actualCount, keys.Count);

        for (int i = start; i < end; i++)
        {
            char key = keys[i];
            var bp = _blueprints[key];
            var element = new LevelElement(key, bp.IsBlocking);
            element.Name = bp.Name;
            element.Animations = new Dictionary<string, Animation> { { Entity.AnimDefault, bp.Animation } };
            result.Add(key, element);
        }
        return result;
    }
}

// Represents a level/room in the game, composed of various LevelElements organized into layers.
class Level
{
    public LevelLayer[] Layers { get; private set; }
    public static readonly string[] LayerNames = ["Bottom", "Mid", "Top"];

    private Vec2 _size;
    public Vec2 Size {
        get {
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

    public Level()
    {
        Layers = new LevelLayer[3];
        _size = new Vec2(0, 0);
        _relativeCenter = new Vec2(0, 0);
    }

    public void SetCell(char? key, Vec2 pos, int depth = -1)
    {
        LevelElement? element = BlueprintManager.GetElement(key == null ? '.' : (char)key, pos);
        if (element != null)
        {
            element.Pos = pos;
            LevelLayer layer = depth == -1 ? Layers[0] : Layers[depth];
            layer.Elements[pos.X, pos.Y] = element;
        }
        else
        {
            LevelLayer layer = depth == -1 ? Layers[0] : Layers[depth];
            layer.Elements[pos.X, pos.Y] = null;
        }
    }

    public void LoadFromTxt(string filePath, int borderv = 10, int borderh = 30)
    {
        DebugLogger.Log($"LoadFromTxt called with path: {filePath}");
        if (File.Exists(filePath))
        {
            DebugLogger.Log($"Txt level file exists: {filePath}");
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream);
            List<string> lines = reader.ReadToEnd().Split('\n').ToList();
            DebugLogger.Log($"Txt level file loaded: {lines.Count} lines");

            //pad left and right
            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = 0; j < borderh; j++)
                {
                    string pre = Randomizer.R().Next(10) > 0 ? "B" : ",";
                    string post = Randomizer.R().Next(10) > 0 ? "B" : ",";
                    lines[i] = pre + lines[i] + post;
                }
            }

            //pad top and bottom
            for (int i = 0; i < borderv; i++)
            {
                string emptyline = "";
                for (int j = 0; j < lines[0].Length; j++)
                {
                    emptyline += Randomizer.R().Next(10) > 0 ? "B" : ",";
                }
                lines.Insert(0, emptyline);
                lines.Add(emptyline);
            }

            //update size for padding
            _size.X = lines[0].Length;
            _size.Y = lines.Count;

            //initialize layer arrays
            for (int i = 0; i < Layers.Length; i++)
            {
                Layers[i] = new LevelLayer(LayerNames[i], i, _size);
            }

            for (int y = 0; y < lines.Count; y++)
            {
                for (int x = 0; x < lines[y].Length; x++)
                {
                    char key = lines[y][x];
                    SetCell(key, new Vec2(x, y));
                }
            }
            //update world origin
            _relativeCenter = _size.Divide(2);

            // Level.SetSprite fills the layerSprite (which is level-sized)
            // Do this only when the sprite has changed
            foreach (LevelLayer layer in Layers)
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

    public void LoadFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return;

            string json = File.ReadAllText(filePath);
            var dto = JsonSerializer.Deserialize<LevelDataDto>(json);

            if (dto == null) return;

            _size = new Vec2(dto.Width, dto.Height);
            
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
                    for (int x = 0; x < Math.Min(row.Length, _size.X); x++)
                    {
                        if (row[x] != '.')
                        {
                            SetCell(row[x], new Vec2(x, y), layerIndex);
                        }
                    }
                }
            }

            _relativeCenter = _size.Divide(2);
            foreach (var layer in Layers) layer.UpdateSprite();
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
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            System.Text.StringBuilder jsonBuilder = new System.Text.StringBuilder();
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
                    System.Text.StringBuilder rowBuilder = new System.Text.StringBuilder();
                    for (int x = 0; x < Size.X; x++)
                    {
                        LevelElement? element = layer.Elements[x, y];
                        rowBuilder.Append(element?.Character ?? '.');
                    }
                    jsonBuilder.AppendLine($"        \"{rowBuilder.ToString()}\"{(y < Size.Y - 1 ? "," : "")}");
                }
                jsonBuilder.AppendLine("      ]");
                jsonBuilder.AppendLine($"    }}{(i < Layers.Length - 1 ? "," : "")}");
            }

            jsonBuilder.AppendLine("  ]");
            jsonBuilder.AppendLine("}");

            File.WriteAllText(filePath, jsonBuilder.ToString());

            DebugLogger.Log($"Level saved successfully to {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"Error saving level to {filePath}: {ex.Message}");
            return false;
        }
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

    //draw elements to layer sprite
    public void UpdateSprite(float shade = 0.0f)
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
                Vec2 elementPos = element.Pos;

                // Iterate over the cells of the element's sprite
                for (int y = 0; y < elementSprite.Size.Y; y++)
                {
                    for (int x = 0; x < elementSprite.Size.X; x++)
                    {
                        // Calculate the destination coordinates on the layer sprite
                        int targetX = elementPos.X + x;
                        int targetY = elementPos.Y + y;

                        //shade
                        ScreenCell cell = elementSprite.Data[y, x];
                        cell.BgColor = Color.Mix(cell.BgColor, Color.Black, shade);
                        cell.Color = Color.Mix(cell.Color, Color.Black, shade);

                        // Draw the cell onto the target sprite. The WriteCell method handles boundary checks.
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
    public char Character { get; private set; }

    public LevelElement(char character = '.', bool blocking = false) : base()
    {
        IsBlocking = blocking;
        Character = character;
    }
}

// Internal classes for JSON Deserialization
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