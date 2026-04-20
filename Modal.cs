namespace MojiGotchi;

public class Modal
{
	//colors
	private Color _bgColor;
	public Color BgColor{
		get
		{
			return _bgColor;
		}
	}
	private Color _edgeColor;

	private string _title;
	public string Title
	{
		get
		{
			return _title;
		}
		protected set // Keep set protected so it's set in constructor but not arbitrarily changed later
		{
			_title = value;
		}
	}

	private Sprite? _bgSprite;
	private Sprite? _contentSprite;

	//modal buttons
	internal List<MenuItem> Options { get; private set; }
	public int SelectedIndex { get; private set; }

	//button sounds
	private Sounds _sounds;

	public Modal(string title, Color bgColor, Color edgeColor)
	{
		_title = title;
		_bgColor = bgColor;
		_edgeColor = edgeColor;
		_bgSprite = null;
		_contentSprite = null;

		//have buttons
		Options = new();
		SelectedIndex = 0;

		//load sounds
		_sounds = new();
		_sounds.AddSound("select", "select.wav");
	}

	internal void AddMenuItem(string title, GameAction action, Color bgcolor, bool enabled = true)
	{
		Options.Add(new MenuItem(title, action, title.Length + 4, bgcolor));
		if (!enabled)
		{
			Options[Options.Count - 1].Disable();
		}
	}

	// Moves the selection down to the next enabled menu item.
	public void SelectRight()
	{
		int currentSelection = SelectedIndex;
		for (int i = currentSelection + 1; i < Options.Count; i++)
		{
			if (Options[i].Enabled)
			{
				SelectedIndex = i;
				SoundManager.PlaySound(_sounds.GetSound("select"));
				return;
			}
		}
	}

	// Moves the selection up to the previous enabled menu item.
	public void SelectLeft()
	{
		int currentSelection = SelectedIndex;
		for (int i = currentSelection - 1; i >= 0; i--)
		{
			if (Options[i].Enabled)
			{
				SelectedIndex = i;
				SoundManager.PlaySound(_sounds.GetSound("select"));
				return;
			}
		}
	}

	public void SelectFirstEnabled()
	{
		while (SelectedIndex < Options.Count && !Options[SelectedIndex].Enabled)
		{
			SelectedIndex++;
		}
	}
	
	public void SetSpriteBg(Vec2 size)
	{
		Sprite sprite = new Sprite(size);
		ScreenCell[,] data = sprite.Data;

		for (int y = 0; y < size.Y; y++)
		{
			for (int x = 0; x < size.X; x++)
			{
				// Determine if the current cell is part of the border
				bool isBorder = (x == 0 || x == size.X - 1 || y == 0 || y == size.Y - 1);

				if (isBorder)
				{
					// Use a simple character for the border, like a space,
					// and rely on the background color for visual distinction.
					// Or, you could use ASCII box-drawing characters for a more defined border.
					data[y, x] = new ScreenCell { Character = ' ', Color = Color.White, BgColor = _edgeColor };
				}
				else
				{
					data[y, x] = new ScreenCell { Character = ' ', Color = Color.White, BgColor = _bgColor };
				}
			}
		}
		//draw title in center of top border
		int titleX = (size.X - _title.Length) / 2;
		for (int x = 0; x < _title.Length; x++)
		{
			data[0, titleX + x] = new ScreenCell { Character = _title[x], Color = Color.White, BgColor = _bgColor };
		}
		string helptext = "Druk op [ESC] om te sluiten";
		int helpX = (size.X - helptext.Length) / 2;
		for (int x = 0; x < helptext.Length; x++)
		{
			data[size.Y-1, helpX + x] = new ScreenCell { Character = helptext[x], Color = Color.Black, BgColor = _edgeColor };
		}
		_bgSprite = sprite;
	}

	public Sprite? GetSpriteBg()
	{
		return _bgSprite;
	}

	public void ClearContentSprite(Vec2 size)
	{
		_contentSprite = new Sprite(size);
	}
	
	//a modal can have a sprite drawn over the content sprite to build it up in steps
	public void AddContent(Sprite content, Vec2 pos)
	{
		//iterate over the sprite's cells and update the content sprite
		if (_contentSprite == null || content == null) return;

		for (int y = 0; y < content.Size.Y; y++)
		{
			for (int x = 0; x < content.Size.X; x++)
			{
				int targetX = pos.X + x;
				int targetY = pos.Y + y;

				// Check boundaries of the content sprite
				if (targetX >= 0 && targetX < _contentSprite.Size.X &&
					targetY >= 0 && targetY < _contentSprite.Size.Y)
				{
					//check if cell is not a null char
					if (content.Data[y, x].Character != '\u0000')
					{
						_contentSprite.WriteCell(new Vec2(targetX, targetY), content.Data[y, x]);
					}
				}
			}
		}
	}

	public Sprite? GetSpriteContent()
	{
		return _contentSprite;
	}
}