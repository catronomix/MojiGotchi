namespace MojiGotchi;

//actions enum for menus
internal enum ActionType
{
	NEWPET,
	QUIT,
	TOPSCORE,
	HELP,
	FEED,
	PLAY,
	PET,
	WAKE,
	EDITOR_LOAD,
	EDITOR_SAVE,
	EDITOR_EDIT,
	EDITOR_HELP,
	EDITOR_QUIT
}

public struct Color
{
    internal byte R { get; set; }
    internal byte G { get; set; }
    internal byte B { get; set; }

    //presets
    internal static readonly Color Black = new Color("#000000");
    internal static readonly Color DarkBlue = new Color("#000f50");
    internal static readonly Color DarkGreen = new Color("#004d26");
    internal static readonly Color DarkCyan = new Color("#004b47");
    internal static readonly Color DarkRed = new Color("#460000");
    internal static readonly Color DarkMagenta = new Color("#490022");
    internal static readonly Color DarkYellow = new Color("#8b8200");
    internal static readonly Color Gray = new Color("#8a8a8a");
    internal static readonly Color DarkGray = new Color("#222222");
    internal static readonly Color Blue = new Color("#001fd1");
    internal static readonly Color Green = new Color("#00ee34");
    internal static readonly Color Cyan = new Color("#00ffff");
    internal static readonly Color Red = new Color("#f10e0e");
    internal static readonly Color Magenta = new Color("#ee0485");
    internal static readonly Color Yellow = new Color("#ffc400");
    internal static readonly Color White = new Color("#ffffff");
    internal static readonly Color LightBlue = new Color("#add8e6");
    internal static readonly Color LightGreen = new Color("#98fb98");
    internal static readonly Color LightCyan = new Color("#b0e0e6");
    internal static readonly Color LightRed = new Color("#ff6347");
    internal static readonly Color LightGray = new Color("#c7c7c7");
    internal static readonly Color LightYellow = new Color("#ffff99");
    internal static readonly Color BabyPink = new Color("#f8a6ba");
    internal static readonly Color BabyBrown = new Color("#703615");
    internal static readonly Color BabyWhite = new Color("#f5ece7");
    internal static readonly Color GrassGreen = new Color("#00a700");
    internal static readonly Color GroundGreen = new Color("#007700");
    internal static readonly Color BushGreen = new Color("#1b691b");
    internal static readonly Color WoodLight = new Color("#aa752e");
    internal static readonly Color WoodDark = new Color("#855517");
    internal static readonly Color WaterLight = new Color("#76c9d4");
    internal static readonly Color WaterDark = new Color("#4e9ca7");

    internal Color(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }

    //hex support
    internal Color(string hex)
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


    internal static bool Equals(Color a, Color b)
    {
        return a.R == b.R && a.G == b.G && a.B == b.B;
    }
}

public struct ScreenCell
{
    internal char Character;
    internal Color Color;
    internal Color BgColor;

    internal const char SkipChar = '\0';

    public ScreenCell()
    {
        Character = ' ';
        Color = Color.White;
        BgColor = Color.Black;
    }

    internal ScreenCell(char character, Color color, Color bgcolor)
    {
        Character = character;
        Color = color;
        BgColor = bgcolor;
    }
}

public struct Vec2
{
    // Change these from fields to properties
    internal int X { get; set; }
    internal int Y { get; set; }

    internal Vec2(int x, int y)
    {
        X = x;
        Y = y;
    }

    internal Vec2 Sum(int x, int y)
    {
        return new Vec2(X + x, Y + y);
    }
    
    internal Vec2 Multiply(int scalar)
    {
        return new Vec2(X * scalar, Y * scalar);
    }

    internal Vec2 Divide(int scalar)
    {
        return new Vec2(X / scalar, Y / scalar);
    }


    // Vec2 static math functions
    static internal bool Equals(Vec2 A, Vec2 B)
    {
        return A.X == B.X && A.Y == B.Y;
    }

    static internal Vec2 Add(Vec2 A, Vec2 B)
    {
        return new Vec2(A.X + B.X, A.Y + B.Y);
    }

    static internal Vec2 Subtract(Vec2 A, Vec2 B)
    {
        return new Vec2(A.X - B.X, A.Y - B.Y);
    }

    static internal Vec2 Multiply(Vec2 A, int scalar)
    {
        return new Vec2(A.X * scalar, A.Y * scalar);
    }

    static internal Vec2 Divide(Vec2 A, int scalar)
    {
        return new Vec2(A.X / scalar, A.Y / scalar);
    }

    //taxicab distance
    static internal int Taxicab(Vec2 A, Vec2 B)
    {
        return Math.Abs(A.X - B.X) + Math.Abs(A.Y - B.Y);
    }

    // Chebyshev distance
    static internal int Chebyshev(Vec2 A, Vec2 B)
    {
        return Math.Max(Math.Abs(A.X - B.X), Math.Abs(A.Y - B.Y));
    }

    //invert values
    static internal Vec2 Invert(Vec2 A)
    {
        return new Vec2(-A.X, -A.Y);
    }

    //get string
    public readonly override string ToString()
    {
        return $"X:{X},Y:{Y}";
    }

}

class SimpleRect
{
    private Vec2 _pos; // Represents Top and Left
    private Vec2 _size; // Represents Width and Height
    protected bool _isDirty; //if rect is changed after creation

    internal Vec2 Pos
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

    internal Vec2 Size
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

    internal Vec2 BottomRight
    {
        get
        {
            return Vec2.Add(Pos, Size);     
        }
    }

    //some ints for convenience
    internal int Left
    {
        get
        {
            return Pos.X;
        }
    }
    internal int Top
    {
        get
        {
            return Pos.Y;
        }
    }
    internal int Right
    {
        get
        {
            return BottomRight.X;
        }
    }
    internal int Bottom
    {
        get
        {
            return BottomRight.Y;
        }
    }

    internal SimpleRect(Vec2 pos, Vec2 size)
    {
        // _isDirty is already true by default, ensuring the sprite is generated on first GetSprite() call.
        _pos = pos;
        _size = size;
        _isDirty = true;
    }

    internal SimpleRect(SimpleRect other)
    {
        // _isDirty is already true by default, ensuring the sprite is generated on first GetSprite() call.
        _pos = other.Pos;
        _size = other.Size;
        _isDirty = true;
    }

    //helper properties for setting width and setting height by making new Vec2 for size
    internal int Width { get => Size.X; set { Size = new Vec2(value, Size.Y); } }
    internal int Height { get => Size.Y; set { Size = new Vec2(Size.X, value); } }
    internal Vec2 RelativeCenter {get => _size.Divide(2);}
    internal Vec2 AbsCenter {get => new Vec2(Left + Width / 2, Top + Height / 2);}
}

// for testing: a rectangle class
class Rect : SimpleRect // Changed to internal class for broader access if needed.
{
    private int _borderWidth;
    private int _borderHeight;
    private Color _fgColor;
    private Color _borderBgColor;
    private Color _bgColor;
    private char _character;


    internal int BorderWidth { get => _borderWidth; set { if (_borderWidth != value) { _borderWidth = value; _isDirty = true; } } }
    internal int BorderHeight { get => _borderHeight; set { if (_borderHeight != value) { _borderHeight = value; _isDirty = true; } } }
    internal Color FgColor { get => _fgColor; set { if (!Color.Equals(_fgColor, value)) { _fgColor = value; _isDirty = true; } } }
    internal Color BorderBgColor { get => _borderBgColor; set { if (!Color.Equals(_borderBgColor, value)) { _borderBgColor = value; _isDirty = true; } } }
    internal Color BgColor { get => _bgColor; set { if (!Color.Equals(_bgColor, value)) { _bgColor = value; _isDirty = true; } } }
    internal char Character { get => _character; set { if (_character != value) { _character = value; _isDirty = true; } } }
    

    private Sprite? _cachedSprite;

    internal Rect(Vec2 pos, Vec2 size, int borderwidth, int borderheight, Color fgColor, Color borderBgColor, Color bgColor, char character) : base (pos, size)
    {
        // Assign directly to backing fields during construction.
        _borderWidth = borderwidth;    // Thickness of left/right borders
        _borderHeight = borderheight;  // Thickness of top/bottom borders
        _fgColor = fgColor;
        _borderBgColor = borderBgColor;
        _bgColor = bgColor;
        _character = character;
    }

    internal SimpleRect GetSimple()
    {
        return new SimpleRect(Pos, Size);
    }

    internal Sprite GetSprite()
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
    internal ActionType MyActionType
    {
        get
        {
            return _myActionType;
        }
    }

    private Action<Game> _logic;

    internal GameAction(ActionType type, Action<Game> logic)
    {
        _myActionType = type;
        _logic = logic;
    }
    
    internal void Use(Game game)
    {
        if (_logic != null)
        {
            _logic(game);
        }
    }
}

internal static class AnimRandom
{
    private static Random _random = new Random();
    internal static int GetRandom(int min, int max)
    {
        return _random.Next(min, max);
    }
}
