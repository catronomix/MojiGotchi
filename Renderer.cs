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

	public void DrawSprite(Sprite? sprite, Vec2 position, SimpleRect? crop = null)
	{
		if (sprite == null) return;
		
		// 1. Define the sprite's bounding box in screen coordinates
		int spriteLeft = position.X;
		int spriteTop = position.Y;
		int spriteRight = position.X + sprite.Size.X;
		int spriteBottom = position.Y + sprite.Size.Y;

		// 2. Determine the effective drawing area (crop, clamped to console bounds)
		int drawAreaLeft, drawAreaTop, drawAreaRight, drawAreaBottom;

		if (crop == null)
		{
			// If no crop is provided, the drawing area is the entire console
			drawAreaLeft = 0;
			drawAreaTop = 0;
			drawAreaRight = _consoleWidth;
			drawAreaBottom = _consoleHeight;
		}
		else
		{
			// If a crop is provided, clamp it to the console bounds
			drawAreaLeft = Math.Max(crop.Left, 0);
			drawAreaTop = Math.Max(crop.Top, 0);
			drawAreaRight = Math.Min(crop.Right, _consoleWidth);
			drawAreaBottom = Math.Min(crop.Bottom, _consoleHeight);
		}

		// 3. Calculate the intersection of the sprite's screen bounds and the effective drawing area
		int renderLeft = Math.Max(spriteLeft, drawAreaLeft);
		int renderTop = Math.Max(spriteTop, drawAreaTop);
		int renderRight = Math.Min(spriteRight, drawAreaRight);
		int renderBottom = Math.Min(spriteBottom, drawAreaBottom);
		
		// 4. If there's no valid intersection (or it's empty), return early
		if (renderLeft >= renderRight || renderTop >= renderBottom)
		{
			return;
		}
		
		// 5. Iterate over the intersection area in screen coordinates
		for (int y = renderTop; y < renderBottom; y++)
		{
			for (int x = renderLeft; x < renderRight; x++)
			{
				// Convert screen coordinates (x, y) to sprite-local coordinates (spriteX, spriteY)
				int spriteLocalY = y - position.Y;
				int spriteLocalX = x - position.X;
				ScreenCell spriteCell = sprite.Data[spriteLocalY, spriteLocalX];
				if (spriteCell.Character == ScreenCell.SkipChar) continue;
				// Draw to the screen buffer (no need for console bounds check here, as renderLeft/Right/Top/Bottom are already clamped)
				_screenBuffer[y, x] = spriteCell;
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