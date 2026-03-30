namespace MojiGotchi;


class Editor : Game
{
	// Set area sizes
	private new int _menuWidth = 21;
	private new int _statusHeight = 5;

	//have a cursor
	private Cursor? _cursor;

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

		_currentModal = null;

		//editor
		_menu.AddItem(LM.Get("editor_menu_revert"), SetAction(ActionType.EDITOR_LOAD), false);
		_menu.AddItem(LM.Get("editor_menu_save"), SetAction(ActionType.EDITOR_SAVE), false);
		_menu.AddItem(LM.Get("editor_menu_edit"), SetAction(ActionType.EDITOR_EDIT), false);

		//modals
		_menu.AddItem(LM.Get("menu_help"), SetAction(ActionType.EDITOR_HELP));

		_menu.SelectFirstEnabled();
		//load saved pet from file
		_cursor = new Cursor();
        
		BlueprintManager.Initialize(); // Initialize blueprints before loading a level
		_level = new Level(); // This is now empty, we need to load a level
		_level.LoadFromFile("level1.txt"); // Load the level from the text file
		SimpleRect deadzone = new SimpleRect(new Vec2(-4, -2), new Vec2(8, 4));
		if (_cursor != null)
		{
			UpdateMenuAvailability([ActionType.NEWPET], false);
			_menu.SelectFirstEnabled();
		}
		
		_camera = new Camera(_level, _cursor, deadzone, _viewport); 

		_persistentStatus = LM.Get("status_welcome"); // Welcome
		// Call CheckWindow once at the end of the constructor to ensure all
		// dimensions are correctly set for the first render.
		CheckWindow(true);
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
		DrawCursor();
		DrawLevelLayer(_level.MidLayer, _level.MidSprite);
		DrawLevelLayer(_level.TopLayer, _level.TopSprite);
		DrawStatus();
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
	}

	void DrawCursor()
	{
		if (_cursor != null && _currentModal == null)
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
		Action<Game> logic;
		switch (type)
		{
			case ActionType.EDITOR_LOAD:
				logic = (editor) =>
				{
					//todo
				};
				break;
			case ActionType.EDITOR_QUIT:
				logic = (editor) =>
				{
					base._isRunning = false;
				};
                break;
			default:
				logic = (editor) => { }; // Default action does nothing
				break;
		}
		return new GameAction(type, logic);
	}
}

public class Cursor: Pet
{
    
    //constructor
    public Cursor()
    {
        
    }

}