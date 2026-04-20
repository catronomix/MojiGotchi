namespace MojiGotchi;

//MenuItem class
class MenuItem
{
	private string _title;
	public string Title
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
	public bool Enabled
	{
		get{
			return _enabled;
		}
	}
	private GameAction _action;
	public GameAction Action
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
	public Sprite Sprite
	{
		get
		{
			return _enabled ? _enabledSprite : _disabledSprite;
		}
	}

	private Color _enabledFgColor = Color.White;
	private Color _disabledFgColor = Color.DarkGray;

	public MenuItem(string title, GameAction action, int width, Color bgcolor)
	{
		_title = title;
		_action = action;
		
		int height = 3; // Fixed height
		Vec2 spriteSize = new Vec2(width, height);
		
		// Create sprites for enabled and disabled states
		// These are populated once on initialization

		_enabledSprite = new Sprite(spriteSize);
		_disabledSprite = new Sprite(spriteSize);

		PopulateSprite(_enabledSprite, _enabledFgColor, bgcolor);
		PopulateSprite(_disabledSprite, _disabledFgColor, Color.Mix(bgcolor, Color.Black, 0.2f));

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
				targetSprite.WriteCell(new Vec2(x, y), new ScreenCell(' ', _enabledFgColor, cellBgColor));
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

	public void Enable()
	{
		_enabled = true;
	}

	public void Disable()
	{
		_enabled = false;
	}

}

//Menu class
class Menu
{
	public int Width { get; private set; }
	public Color BgColor { get; private set; }
	public Color Color { get; private set; } // Foreground color for the menu container
	public List<MenuItem> MenuItems { get; private set; }
	public int SelectedIndex { get; private set; }
	public bool Enabled { get; private set; }

	//sounds
	private Sounds _sounds;

	// Constructor
	public Menu(int width, Color bgColor, Color color)
	{
		Width = width;
		BgColor = bgColor;
		Color = color;
		MenuItems = new List<MenuItem>(); // Ensure the list is never null

		// Set selected item to the first if available
		SelectedIndex = 0;
		Enabled = true;

		//load sounds
		_sounds = new();
		_sounds.AddSound("select", "select.wav");
	}

	public void AddItem(string title, GameAction action, Color bgcolor, bool enabled = true)
	{
		MenuItems.Add(new MenuItem(title, action, Width - 2, bgcolor));
		if (!enabled)
		{
			MenuItems[MenuItems.Count - 1].Disable();
		}
	}

	// Moves the selection down to the next enabled menu item.
	public void SelectDown()
	{
		int currentSelection = SelectedIndex;
		for (int i = currentSelection + 1; i < MenuItems.Count; i++)
		{
			if (MenuItems[i].Enabled)
			{
				SelectedIndex = i;
				SoundManager.PlaySound(_sounds.GetSound("select"));
				return;
			}
		}
	}

	// Moves the selection up to the previous enabled menu item.
	public void SelectUp()
	{
		int currentSelection = SelectedIndex;
		for (int i = currentSelection - 1; i >= 0; i--)
		{
			if (MenuItems[i].Enabled)
			{
				SelectedIndex = i;
				SoundManager.PlaySound(_sounds.GetSound("select"));
				return;
			}
		}
	}

	public void SelectFirstEnabled()
	{
		while (SelectedIndex < MenuItems.Count && !MenuItems[SelectedIndex].Enabled)
		{
			SelectedIndex++;
		}
	}

	public void Enable()
	{
		Enabled = true;
	}

	public void Disable()
	{
		Enabled = false;
	}

}