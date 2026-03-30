namespace MojiGotchi;

public struct Color
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }

    //presets
    public static readonly Color Black = new Color("#000000");
    public static readonly Color DarkBlue = new Color("#000f50");
    public static readonly Color DarkGreen = new Color("#004d26");
    public static readonly Color DarkCyan = new Color("#004b47");
    public static readonly Color DarkRed = new Color("#460000");
    public static readonly Color DarkMagenta = new Color("#490022");
    public static readonly Color DarkYellow = new Color("#8b8200");
    public static readonly Color Gray = new Color("#8a8a8a");
    public static readonly Color DarkGray = new Color("#222222");
    public static readonly Color Blue = new Color("#001fd1");
    public static readonly Color Green = new Color("#00ee34");
    public static readonly Color Cyan = new Color("#00ffff");
    public static readonly Color Red = new Color("#f10e0e");
    public static readonly Color Magenta = new Color("#ee0485");
    public static readonly Color Yellow = new Color("#ffc400");
    public static readonly Color White = new Color("#ffffff");
    public static readonly Color LightBlue = new Color("#add8e6");
    public static readonly Color LightGreen = new Color("#98fb98");
    public static readonly Color LightCyan = new Color("#b0e0e6");
    public static readonly Color LightRed = new Color("#ff6347");
    public static readonly Color LightGray = new Color("#c7c7c7");
    public static readonly Color LightYellow = new Color("#ffff99");
    public static readonly Color BabyPink = new Color("#f8a6ba");
    public static readonly Color BabyBrown = new Color("#703615");
    public static readonly Color BabyWhite = new Color("#f5ece7");
    public static readonly Color GrassGreen = new Color("#00a700");
    public static readonly Color GroundGreen = new Color("#007700");
    public static readonly Color BushGreen = new Color("#1b691b");
    public static readonly Color WoodLight = new Color("#aa752e");
    public static readonly Color WoodDark = new Color("#855517");
    public static readonly Color WaterLight = new Color("#76c9d4");
    public static readonly Color WaterDark = new Color("#4e9ca7");

    public Color(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }

    //hex support
    public Color(string hex)
    {
        hex = hex.Replace("#", "");
        if (hex.Length == 6 && hex.All(char.IsAsciiHexDigit)){
            R = Convert.ToByte(hex.Substring(0, 2), 16);
            G = Convert.ToByte(hex.Substring(2, 2), 16);
            B = Convert.ToByte(hex.Substring(4, 2), 16);
        }
        else
        {
            R = 255;
            G = 255;
            B = 0;
        }
    }


    public static bool Equals(Color a, Color b)
    {
        return a.R == b.R && a.G == b.G && a.B == b.B;
    }
}

public struct ScreenCell
{
    public char Character;
    public Color Color;
    public Color BgColor;

    public const char SkipChar = '\0';

    public ScreenCell()
    {
        Character = ' ';
        Color = Color.White;
        BgColor = Color.Black;
    }

    public ScreenCell(char character, Color color, Color bgcolor)
    {
        Character = character;
        Color = color;
        BgColor = bgcolor;
    }
}

public struct Vec2
{
    // Change these from fields to properties
    public int X { get; set; }
    public int Y { get; set; }

    public Vec2(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Vec2 Sum(int x, int y)
    {
        return new Vec2(X + x, Y + y);
    }
    
    public Vec2 Multiply(int scalar)
    {
        return new Vec2(X * scalar, Y * scalar);
    }

    public Vec2 Divide(int scalar)
    {
        return new Vec2(X / scalar, Y / scalar);
    }


    // Vec2 static math functions
    static public bool Equals(Vec2 A, Vec2 B)
    {
        return A.X == B.X && A.Y == B.Y;
    }

    static public Vec2 Add(Vec2 A, Vec2 B)
    {
        return new Vec2(A.X + B.X, A.Y + B.Y);
    }

    static public Vec2 Subtract(Vec2 A, Vec2 B)
    {
        return new Vec2(A.X - B.X, A.Y - B.Y);
    }

    static public Vec2 Multiply(Vec2 A, int scalar)
    {
        return new Vec2(A.X * scalar, A.Y * scalar);
    }

    static public Vec2 Divide(Vec2 A, int scalar)
    {
        return new Vec2(A.X / scalar, A.Y / scalar);
    }

    //taxicab distance
    static public int Taxicab(Vec2 A, Vec2 B)
    {
        return Math.Abs(A.X - B.X) + Math.Abs(A.Y - B.Y);
    }

    // Chebyshev distance
    static public int Chebyshev(Vec2 A, Vec2 B)
    {
        return Math.Max(Math.Abs(A.X - B.X), Math.Abs(A.Y - B.Y));
    }

    //invert values
    static public Vec2 Invert(Vec2 A)
    {
        return new Vec2(-A.X, -A.Y);
    }
}

class SimpleRect
{
    private Vec2 _pos; // Represents Top and Left
    private Vec2 _size; // Represents Width and Height
    protected bool _isDirty; //if rect is changed after creation

    public Vec2 Pos
    {
        get
        {
            return _pos;
        }
        set
        {
            if (_pos.X != value.X || _pos.Y != value.Y)
            {
                _pos = value;
                _isDirty = true;
            }
        }
    }

    public Vec2 Size
    {
        get
        {
            return _size;
        }
        set 
        {
            if (_size.X != value.X || _size.Y != value.Y)
            {
                _size = value;
                _isDirty = true;
            } 
        }
    }

    public Vec2 BottomRight
    {
        get
        {
            return Vec2.Add(Pos, Size);     
        }
    }

    //some ints for convenience
    public int Left
    {
        get
        {
            return Pos.X;
        }
    }
    public int Top
    {
        get
        {
            return Pos.Y;
        }
    }
    public int Right
    {
        get
        {
            return BottomRight.X;
        }
    }
    public int Bottom
    {
        get
        {
            return BottomRight.Y;
        }
    }

    public SimpleRect(Vec2 pos, Vec2 size)
    {
        // _isDirty is already true by default, ensuring the sprite is generated on first GetSprite() call.
        _pos = pos;
        _size = size;
        _isDirty = true;
    }

    public SimpleRect(SimpleRect other)
    {
        // _isDirty is already true by default, ensuring the sprite is generated on first GetSprite() call.
        _pos = other.Pos;
        _size = other.Size;
        _isDirty = true;
    }

    //helper properties for setting width and setting height by making new Vec2 for size
    public int Width { get => Size.X; set { Size = new Vec2(value, Size.Y); } }
    public int Height { get => Size.Y; set { Size = new Vec2(Size.X, value); } }
    public Vec2 RelativeCenter {get => _size.Divide(2);}
    public Vec2 AbsCenter {get => new Vec2(Left + Width / 2, Top + Height / 2);}
}

// for testing: a rectangle class
class Rect : SimpleRect // Changed to public class for broader access if needed.
{
    private int _borderWidth;
    private int _borderHeight;
    private Color _fgColor;
    private Color _borderBgColor;
    private Color _bgColor;
    private char _character;


    public int BorderWidth { get => _borderWidth; set { if (_borderWidth != value) { _borderWidth = value; _isDirty = true; } } }
    public int BorderHeight { get => _borderHeight; set { if (_borderHeight != value) { _borderHeight = value; _isDirty = true; } } }
    public Color FgColor { get => _fgColor; set { if (!Color.Equals(_fgColor, value)) { _fgColor = value; _isDirty = true; } } }
    public Color BorderBgColor { get => _borderBgColor; set { if (!Color.Equals(_borderBgColor, value)) { _borderBgColor = value; _isDirty = true; } } }
    public Color BgColor { get => _bgColor; set { if (!Color.Equals(_bgColor, value)) { _bgColor = value; _isDirty = true; } } }
    public char Character { get => _character; set { if (_character != value) { _character = value; _isDirty = true; } } }
    

    private Sprite? _cachedSprite;

    public Rect(Vec2 pos, Vec2 size, int borderwidth, int borderheight, Color fgColor, Color borderBgColor, Color bgColor, char character) : base (pos, size)
    {
        // Assign directly to backing fields during construction.
        _borderWidth = borderwidth;    // Thickness of left/right borders
        _borderHeight = borderheight;  // Thickness of top/bottom borders
        _fgColor = fgColor;
        _borderBgColor = borderBgColor;
        _bgColor = bgColor;
        _character = character;
    }

    public SimpleRect GetSimple()
    {
        return new SimpleRect(Pos, Size);
    }

    public Sprite GetSprite()
    {
        // Regenerate sprite only if properties have changed (_isDirty is true),
        // or if there's no cached sprite yet,
        // or if the dimensions of the cached sprite no longer match the current Rect dimensions.
        // The last check is crucial because if Size change, the underlying
        // ScreenCell[,] array needs to be re-allocated, which means a new Sprite instance.
        if (_isDirty || _cachedSprite == null || _cachedSprite.Size.X != Size.X || _cachedSprite.Size.Y != Size.Y)
        {
            // Create a new Sprite instance. The Sprite constructor allocates a new ScreenCell[,] array.
            _cachedSprite = new Sprite(Size);
            var data = _cachedSprite.Data;

            for (int y = 0; y < Size.Y; y++)
            {
                for (int x = 0; x < Size.X; x++)
                {
                    // Determine if the current cell is part of the border
                    bool isBorder = y < BorderHeight ||                  // Top border
                                    y >= Size.Y - BorderHeight ||         // Bottom border
                                    x < BorderWidth ||                    // Left border
                                    x >= Size.X - BorderWidth;             // Right border

                    if (isBorder)
                    {
                        data[y, x] = new ScreenCell { Character = Character, Color = FgColor, BgColor = BorderBgColor };
                    }
                    else
                    {
                        data[y, x] = new ScreenCell { Character = ' ', Color = FgColor, BgColor = BgColor };
                    }
                }
            }
        }
        _isDirty = false; // Mark as clean after (re)generation
        return _cachedSprite;
    }

}


class GameAction
{
    private	ActionType _myActionType;
    public ActionType MyActionType
    {
        get
        {
            return _myActionType;
        }
    }

    private Action<Game> _logic;

    public GameAction(ActionType type, Action<Game> logic)
    {
        _myActionType = type;
        _logic = logic;
    }
    
    public void Use(Game game)
    {
        if (_logic != null)
        {
            _logic(game);
        }
    }
}

public static class AnimRandom
{
    private static Random _random = new Random();
    public static int GetRandom(int min, int max)
    {
        return _random.Next(min, max);
    }
}
