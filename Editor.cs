namespace MojiGotchi;


class Editor : Game
{
	// Set area sizes
	private new int _menuWidth = 21;
	private new int _statusHeight = 5;

	//have a cursor
	private Cursor? _cursor;
	private bool _editingmode;

	// Initialize the editor
	public Editor()
	{
		base._renderer = new Renderer(programBgColor);
		// Initialize Rects for drawing the editor's menu, status bar and play area
		_menuBgRect = new Rect(new Vec2(0, 0), new Vec2(_menuWidth, 1), 1, 1, Color.Yellow, Color.DarkGray, Color.DarkGray, '.');
		_statusBgRect = new Rect(new Vec2(_menuWidth, 0), new Vec2(1, _statusHeight), 0, 1, Color.White, Color.DarkGray, Color.Black, '.');
		_viewport = new Rect(new Vec2(_menuWidth, _statusHeight), new Vec2(1, 1), 0, 0, Color.White, Color.Black, Color.Black, ' ');

		//init menu
		_menu = new Menu(_menuWidth, Color.Gray, Color.Yellow);

		//init modals
		_currentModal = null;
		EditorHelp _help = new EditorHelp("Editor Help", Color.DarkGray, Color.White);

		//editor
		_menu.AddItem(LM.Get("editor_menu_revert"), SetAction(ActionType.EDITOR_LOAD));
		_menu.AddItem(LM.Get("editor_menu_save"), SetAction(ActionType.EDITOR_SAVE));
		_menu.AddItem(LM.Get("editor_menu_edit"), SetAction(ActionType.EDITOR_EDIT));

		//modals
		_menu.AddItem(LM.Get("menu_help"), SetAction(ActionType.EDITOR_HELP));
		_menu.AddItem(LM.Get("menu_quit"), SetAction(ActionType.EDITOR_QUIT));


		_menu.SelectFirstEnabled();
		//load saved pet from file
		_cursor = new Cursor();
        
		BlueprintManager.Initialize(); // Initialize blueprints before loading a level
		_level = new Level(); // This is now empty, we need to load a level
		_level.LoadFromFile("level1.txt"); // Load the level from the text file
		SimpleRect deadzone = new SimpleRect(new Vec2(-4, -2), new Vec2(8, 4));

		_camera = new Camera(_level, _cursor, deadzone, _viewport); 

		_persistentStatus = LM.Get("status_welcome_editor"); // Welcome
		// Call CheckWindow once at the end of the constructor to ensure all
		// dimensions are correctly set for the first render.
		CheckWindow(true);
		_editingmode = false;
	}

	public new bool Step()
	{
		//Check window resized
		CheckWindow();

		/*--------------------DRAWING--------------------*/
		// don't clear renderer when not needed
		// _renderer.ClearBuffer();
		//draw area Rects to renderer
		DrawRects();
		DrawMenuItems();

		//update camera before drawing editor
		_camera.UpdateCamera();

		DrawLevelLayer(_level.BottomLayer, _level.BottomSprite);
		DrawLevelLayer(_level.MidLayer, _level.MidSprite);
		DrawLevelLayer(_level.TopLayer, _level.TopSprite);
		DrawStatus();
		DrawCursor();
		_renderer.RenderScreen();

		/*--------------------INPUT--------------------*/
		HandleInput();
		Thread.Sleep(sleepDelta);
		return _isRunning;
	}

	new void HandleInput()
	{
		if (Console.KeyAvailable)
		{
			ConsoleKeyInfo key = Console.ReadKey(true);
			if (_menu.Enabled) //menu is active
			{
				switch (key.Key)
				{
					case ConsoleKey.UpArrow:
						//menu selection up
						_menu.SelectUp();
						break;
					case ConsoleKey.DownArrow:
						//menu selection down
						_menu.SelectDown();
						break;
					case ConsoleKey.Enter:
						//confirm menu selection
						GameAction action = _menu.MenuItems[_menu.SelectedIndex].Action;
						action.Use(this);
						break;
					case ConsoleKey.Escape:
						//close any modal if modal is open
						CloseModal();
						break;
					default:
						break;
				}
			}
			else //menu is disabled
			{
				switch (key.Key)
				{
					case ConsoleKey.Escape:
						//close any modal if modal is open
						CloseModal();
						break;
					default:
						break;
				}
			}
		}
	}

	void DrawCursor()
	{
		if (_cursor != null && _editingmode == true)
		{
			Sprite? cursorSprite = _cursor.GetSprite(); // Capture the sprite once
			if (cursorSprite != null)
			{
				// 2. Calculate Top-Left based on Cursor's World Position and its Pivot (center)
				// Cursor position (0,0) is world center.
				// We subtract cursorSprite.Size / 2 to make (0,0) the center of the pet.
				Vec2 drawPos = Vec2.Add(_camera.GetAbsCenter(), _cursor.Position.Sum(-1,-1));
				_renderer.DrawSprite(cursorSprite, drawPos, _viewport);
			}
		}
	}

	//editor actions
	private GameAction SetAction(ActionType type)
	{
		Action<Editor> logic;
		switch (type)
		{
			case ActionType.EDITOR_LOAD:
				logic = (editor) =>
				{
					//todo
				};
				break;
			case ActionType.EDITOR_SAVE:
				logic = (editor) =>
				{
					//todo
				};
                break;
			case ActionType.EDITOR_EDIT:
				logic = (editor) =>
				{
					//todo
				};
				break;
			case ActionType.EDITOR_SAVE:
				logic = (editor) =>
				{
					//todo
				};
				break;
			case ActionType.EDITOR_EDIT:
				logic = (editor) =>
				{
					_editingmode = true;
					_menu.Disable();
				};
				break;
			case ActionType.EDITOR_QUIT:
				logic = (editor) =>
				{
					base._isRunning = false;
				};
				break;
			case ActionType.EDITOR_HELP:
				logic = (editor) =>
				{
					editor._help.UpdatePage(_viewport.Size);
					base._currentModal = _help;
					_menu.Disable();
				};
				break;
			
			default:
				logic = (editor) => { }; // Default action does nothing
				break;
		}
		return new GameAction(type, logic);
	}
}

class EditorAction : GameAction
{
	
	private Action<Editor> _logic;

	public EditorAction(ActionType type, Action<Editor> logic): base(type, logic)
	{
		_myActionType = type;
		_logic = logic;
	}

	public void Use(Editor editor)
	{
		if (_logic != null)
		{
			_logic(editor);
		}
	}
}

public class Cursor: Entity
{
    
    //constructor
    public Cursor()
    {
        _animations = JsonParser.LoadAnimations("CursorSprites.json", 200);
        SetAnimation(AnimDefault);
        _position = new Vec2(0,0);
    }
}

public class EditorHelp: Modal
{
	private string _helpText = @"
EDITOR HELP PAGINA
------------------

Nog in te vullen";

	//constructor
	public EditorHelp(string title, Color bgcolor, Color edgecolor) : base(title, bgcolor, edgecolor)
	{
		
	}

	public void UpdatePage(Vec2 size)
	{
		SetSpriteBg(size);
		ClearContentSprite(size);
		string[] lines = _helpText.Split('\n');

		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i].TrimEnd('\r');
			if (string.IsNullOrEmpty(line)) continue;

			Sprite lineSprite = new Sprite(new Vec2(line.Length, 1));
			for (int x = 0; x < line.Length; x++)
			{
				lineSprite.WriteCell(new Vec2(x, 0), new ScreenCell
				{
					Character = line[x],
					Color = Color.White,
					BgColor = base.BgColor
				});
			}

			// Add content with a small margin (x=2) and vertical offset
			AddContent(lineSprite, new Vec2(4, i + 2));
		}
	}
}