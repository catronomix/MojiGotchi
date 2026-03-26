namespace MojiGotchi;

using System.Text;

class Renderer
{
	private ScreenCell[,] _screenBuffer;
	private int _consoleWidth;
	private int _consoleHeight;
	private StringBuilder _stringBuilder = new StringBuilder();
	private Color _programBgColor;

	public Renderer(Color programBgColor)
	{
		_consoleWidth = Console.WindowWidth;
		_consoleHeight = Console.WindowHeight;
		_screenBuffer = new ScreenCell[_consoleHeight, _consoleWidth];
		_programBgColor = programBgColor;
	}

	public int ConsoleWidth => _consoleWidth;
	public int ConsoleHeight => _consoleHeight;

	public bool Resize(int newWidth, int newHeight, bool force = false)
	{
		if (force) { _consoleWidth = 1; _consoleHeight = 1; }
		if (newWidth != _consoleWidth || newHeight != _consoleHeight)
		{
			_consoleWidth = newWidth;
			_consoleHeight = newHeight;
			_screenBuffer = new ScreenCell[_consoleHeight, _consoleWidth];
			ConsoleHelper.HideCursor();
			return true;
		}
		return false;
	}

	public void ClearBuffer()
	{
		var backgroundCell = new ScreenCell(' ', Color.White, _programBgColor);
		for (int y = 0; y < _consoleHeight; y++)
		{
			for (int x = 0; x < _consoleWidth; x++)
			{
				_screenBuffer[y, x] = backgroundCell;
			}
		}
	}

	public void RenderScreen()
	{
		_stringBuilder.Clear();
		_stringBuilder.Append("\x1b[H");

		Color lastFgColor = new Color(0, 0, 0);
		Color lastBgColor = new Color(0, 0, 0);
		bool firstCell = true;

		for (int y = 0; y < _consoleHeight; y++)
		{
			for (int x = 0; x < _consoleWidth; x++)
			{
				ScreenCell cell = _screenBuffer[y, x];
				if (firstCell || !Color.Equals(cell.Color, lastFgColor))
				{
					_stringBuilder.Append(ConsoleHelper.GetForegroundAnsiCode(cell.Color));
					lastFgColor = cell.Color;
				}
				if (firstCell || !Color.Equals(cell.BgColor, lastBgColor))
				{
					_stringBuilder.Append(ConsoleHelper.GetBackgroundAnsiCode(cell.BgColor));
					lastBgColor = cell.BgColor;
				}
				_stringBuilder.Append(cell.Character);
				firstCell = false;
			}
		}
		Console.Write(_stringBuilder.ToString());
	}

	public void DrawSprite(Sprite? sprite, Vec2 position, Rect? crop = null)
	{
		if (sprite == null) return;
		for (int y = 0; y < sprite.Size.Y; y++)
		{
			for (int x = 0; x < sprite.Size.X; x++)
			{
				int screenY = position.Y + y;
				int screenX = position.X + x;
				ScreenCell spriteCell = sprite.Data[y, x];
				if (spriteCell.Character == ScreenCell.SkipChar) continue;
				if (screenY >= 0 && screenY < _consoleHeight && screenX >= 0 && screenX < _consoleWidth)
				{
					if (crop == null || (screenX >= crop.Pos.X && screenX < crop.Pos.X + crop.Size.X && screenY >= crop.Pos.Y && screenY < crop.Pos.Y + crop.Size.Y))
					{
						_screenBuffer[screenY, screenX] = spriteCell;
					}
				}
			}
		}
	}

	public void DrawText(string text, Vec2 position, Color fgColor, Color bgColor)
	{
		for (int i = 0; i < text.Length; i++)
		{
			int screenY = position.Y;
			int screenX = position.X + i;
			if (screenY >= 0 && screenY < _consoleHeight && screenX >= 0 && screenX < _consoleWidth)
			{
				char c = text[i];
				if (c == ScreenCell.SkipChar) continue;
				_screenBuffer[screenY, screenX] = new ScreenCell(c, fgColor, bgColor);
			}
		}
	}

	public void DrawText(string text, Vec2 position, Color fgColor)
	{
		for (int i = 0; i < text.Length; i++)
		{
			int screenY = position.Y;
			int screenX = position.X + i;
			if (screenY >= 0 && screenY < _consoleHeight && screenX >= 0 && screenX < _consoleWidth)
			{
				char c = text[i];
				if (c == ScreenCell.SkipChar) continue;
				_screenBuffer[screenY, screenX].Character = c;
				_screenBuffer[screenY, screenX].Color = fgColor;
			}
		}
	}
}