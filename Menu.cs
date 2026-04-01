namespace MojiGotchi;

//MenuItem class
class MenuItem
{
	private string _title;
	internal string Title
	{
		get{
			return _title;
		}
		set
		{
			_title = value;
		}
	}
	
	private bool _enabled;
	internal bool Enabled
	{
		get{
			return _enabled;
		}
	}
	private GameAction _action;
	internal GameAction Action
	{
		get
		{
			return _action;
		}
		set
		{
			_action = value;
		}
	}

	private Sprite _enabledSprite;
	private Sprite _disabledSprite;
	internal Sprite Sprite
	{
		get
		{
			return _enabled ? _enabledSprite : _disabledSprite;
		}
	}

	private Color _defaultFgColor = Color.White;
	private Color _defaultBgColor = Color.DarkGreen;
	private Color _disabledFgColor = Color.DarkGray;

	internal MenuItem(string title, GameAction action, int width)
	{
		_title = title;
		_action = action;
		
		int height = 3; // Fixed height
		Vec2 spriteSize = new Vec2(width, height);
		
		// Create sprites for enabled and disabled states
		// These are populated once on initialization

		_enabledSprite = new Sprite(spriteSize);
		_disabledSprite = new Sprite(spriteSize);

		PopulateSprite(_enabledSprite, _defaultFgColor, _defaultBgColor);
		PopulateSprite(_disabledSprite, _disabledFgColor, _defaultBgColor);

		_enabled = true; // Default to enabled
	}

	private void PopulateSprite(Sprite targetSprite, Color textFgColor, Color cellBgColor)
	{
		int width = targetSprite.Size.X;
		int height = targetSprite.Size.Y;

		// Fill sprite background
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				targetSprite.WriteCell(new Vec2(x, y), new ScreenCell(' ', _defaultFgColor, cellBgColor));
			}
		}

		// Center text
		int textX = Math.Max(0, (width - Title.Length) / 2);
		int textY = height / 2;
		for (int i = 0; i < Title.Length; i++)
		{
			if (textX + i < width)
			{
				targetSprite.WriteCell(new Vec2(textX + i, textY), new ScreenCell(Title[i], textFgColor, cellBgColor));
			}
		}
		// Draw a border on the outer cells using ASCII box-drawing characters
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				char borderChar = ' ';
				bool isBorderCell = false;

				if (y == 0) // Top row
				{
					if (x == 0) borderChar = '╔'; // Top-left
					else if (x == width - 1) borderChar = '╗'; // Top-right
					else borderChar = '═'; // Top horizontal
					isBorderCell = true;
				}
				else if (y == height - 1) // Bottom row
				{
					if (x == 0) borderChar = '╚'; // Bottom-left
					else if (x == width - 1) borderChar = '╝'; // Bottom-right
					else borderChar = '═'; // Bottom horizontal
					isBorderCell = true;
				}
				else if (x == 0 || x == width - 1) // Middle rows, left/right columns
				{
					borderChar = '║'; // Vertical
					isBorderCell = true;
				}

				if (isBorderCell)
				{
					targetSprite.Data[y, x].Character = borderChar;
					targetSprite.Data[y, x].Color = textFgColor; // Border color matches text color
					targetSprite.Data[y, x].BgColor = cellBgColor; // Border background matches item background
				}
			}
		}
	}

	internal void Enable()
	{
		_enabled = true;
	}

	internal void Disable()
	{
		_enabled = false;
	}

}

//Menu class
class Menu
{
	internal int Width { get; private set; }
	internal Color BgColor { get; private set; }
	internal Color Color { get; private set; } // Foreground color for the menu container
	internal List<MenuItem> MenuItems { get; private set; }
	internal int SelectedIndex { get; private set; }
	internal bool Enabled { get; private set; }

	// Constructor
	internal Menu(int width, Color bgColor, Color color)
	{
		Width = width;
		BgColor = bgColor;
		Color = color;
		MenuItems = new List<MenuItem>(); // Ensure the list is never null

		// Set selected item to the first if available
		SelectedIndex = 0;
		Enabled = true;
	}

	internal void AddItem(string title, GameAction action, bool enabled = true)
	{
		MenuItems.Add(new MenuItem(title, action, Width - 2));
		if (!enabled)
		{
			MenuItems[MenuItems.Count - 1].Disable();
		}
	}

	// Moves the selection down to the next enabled menu item.
	internal void SelectDown()
	{
		int currentSelection = SelectedIndex;
		for (int i = currentSelection + 1; i < MenuItems.Count; i++)
		{
			if (MenuItems[i].Enabled)
			{
				SelectedIndex = i;
				SoundManager.Play("select.wav");
				return;
			}
		}
	}

	// Moves the selection up to the previous enabled menu item.
	internal void SelectUp()
	{
		int currentSelection = SelectedIndex;
		for (int i = currentSelection - 1; i >= 0; i--)
		{
			if (MenuItems[i].Enabled)
			{
				SelectedIndex = i;
				SoundManager.Play("select.wav");
				return;
			}
		}
	}

	internal void SelectFirstEnabled()
	{
		while (SelectedIndex < MenuItems.Count && !MenuItems[SelectedIndex].Enabled)
		{
			SelectedIndex++;
		}
	}

	internal void Enable()
	{
		Enabled = true;
	}

	internal void Disable()
	{
		Enabled = false;
	}

}