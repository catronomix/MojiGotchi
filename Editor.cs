using System.Data;

namespace MojiGotchi;

public enum Focus
{
	MENU,
	CURSOR,
	BROWSER,
	MODAL
}

public enum EditingMode
{
	DISABLED,
	INSERTSINGLE,
	INSERTRECTSTART,
	INSERTRECTEND,
	FILL,
	DELETE
}

class Editor : Game
{
	//have a cursor
	private Cursor? _cursor;
	private EditingMode _editingmode;
	protected EditorHelp _editorHelp; // New field for editor-specific help modal
	private LevelElement? _selectedElement;
	private LayerBar _layerbar;

	//have a blueprint bar
	BlueprintBar _blueprintBar;

	//be able to focus controls
	private Focus _focus;

	// Initialize the editor
	public Editor()
	{
		// Set area sizes
		base._menuWidth = 21;
		base._statusHeight = 5;

		// Initialize Rects for drawing the editor's menu, status bar and play area
		_menuBgRect = new Rect(new Vec2(0, 0), new Vec2(_menuWidth, 1), 1, 1, Color.Yellow, Color.DarkGray, Color.DarkGray, '.');
		_statusBgRect = new Rect(new Vec2(_menuWidth, 0), new Vec2(1, _statusHeight), 0, 1, Color.White, Color.DarkGray, Color.Black, '.');
		_viewport = new Rect(new Vec2(_menuWidth, _statusHeight), new Vec2(1, 1), 0, 0, Color.White, Color.Black, Color.Black, ' ');

		//init menu
		_menu = new Menu(_menuWidth, Color.Gray, Color.Yellow);

		//init modals
		_currentModal = null;
		EditorHelp _help = new EditorHelp("Editor Help", Color.DarkGray, Color.White);
		_editorHelp = new EditorHelp("Editor Help", Color.DarkGray, Color.White); // Initialize editor-specific help
		//editor
		_menu.AddItem(LM.Get("editor_menu_revert"), SetAction(ActionType.EDITOR_LOAD));
		_menu.AddItem(LM.Get("editor_menu_save"), SetAction(ActionType.EDITOR_SAVE));
		_menu.AddItem(LM.Get("editor_menu_edit"), SetAction(ActionType.EDITOR_EDIT));

		//modals
		_menu.AddItem(LM.Get("menu_help"), SetAction(ActionType.EDITOR_HELP));
		_menu.AddItem(LM.Get("menu_quit"), SetAction(ActionType.EDITOR_QUIT));

		_menu.SelectFirstEnabled();
		
		//has cursor
		_cursor = new Cursor();
		_selectedElement = null;
		_focus = Focus.MENU;

		//has blueprint bar
		_blueprintBar = new BlueprintBar();

		//set active layer
		_layerbar = new LayerBar(_level.Layers.Length, new Vec2(-5, 3).Sum(_viewport.Right, _viewport.Top), new Vec2(3, 5));
		_layerbar.Update();

		BlueprintManager.Initialize(); // Initialize blueprints before loading a level
		_level = new Level(); // This is now empty, we need to load a level
		_level.LoadFromFile("level1.txt"); // Load the level from the text file

		_persistentStatus = LM.Get("editor_status_welcome"); // Welcome
		// Call CheckWindow once at the end of the constructor to ensure all
		// dimensions are correctly set for the first render.

		// has camera
		SimpleRect deadzone = new SimpleRect(new Vec2(-4, -2), new Vec2(8, 4));
		_camera = new Camera(_level, _cursor, deadzone, _viewport);

		CheckWindow(true);
		_editingmode = EditingMode.DISABLED;
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

		DrawLevelLayer(_level.Layers[0]);
		DrawLevelLayer(_level.Layers[1]);
		DrawLevelLayer(_level.Layers[2]);
		DrawStatus();
		DrawCursor();
		DrawGlyphs();
		_renderer.RenderScreen();

		/*--------------------INPUT--------------------*/
		HandleInput();
		Thread.Sleep(sleepDelta);
		return _isRunning;
	}

	internal new void CheckWindow(bool force = false)
	{
		bool resized = base.CheckWindow(force);
		if (resized)
		{
			_layerbar.Pos = new Vec2(-5, 3).Sum(_viewport.Right, _viewport.Top);
		}
	}

	new void DrawStatus(int spacing = 1){
		if (_editingmode > 0){ //when editor is active
			String persistentstring = LM.Get("selected")+" [ ] | ";

			persistentstring += LM.Get("editor_editmode")+": ";
			if (_editingmode == EditingMode.INSERTSINGLE){
				persistentstring += LM.Get("editor_editmode_single");
			}
			else if(_editingmode == EditingMode.INSERTRECTSTART || _editingmode == EditingMode.INSERTRECTEND){
				persistentstring += LM.Get("editor_editmode_rect");
			}
			else if(_editingmode == EditingMode.FILL){
				persistentstring += LM.Get("editor_editmode_fill");
			}
			else if(_editingmode == EditingMode.DELETE){
				persistentstring += LM.Get("editor_editmode_delete");
			}
			_persistentStatus = persistentstring;
		}
		//rest of game status
		base.DrawStatus(spacing);
	}

	private void DrawGlyphs()
	{
		if (_editingmode != EditingMode.DISABLED && _selectedElement != null)
		{
			//draw selection glyph
			int xpos = _statusBgRect.RelativeCenter.X - _transientStatus.Length / 2 + 1;
			_renderer.DrawSprite(_selectedElement.GetSprite(), _statusBgRect.Pos.Sum(xpos, 2));
		}

		if (_editingmode > 0)
		{
			Sprite barsprite = _blueprintBar.GetSprite();
			if (barsprite != null)
			{
				// draw blueprintbar
				int xpos = _menuWidth + _viewport.RelativeCenter.X -barsprite.Size.X / 2;
				int ypos = _viewport.Bottom - 3;
				_renderer.DrawSprite(barsprite, new Vec2(xpos, ypos));
			}
			if (_blueprintBar.SelectedElement != null)
			{
				//draw blueprint selection glyph
				int xpos = _statusBgRect.Left + _statusBgRect.RelativeCenter.X - _persistentStatus.Length / 2 + LM.Get("selected").Length -3;
				int ypos = _statusBgRect.Top + 1;
				_renderer.DrawSprite(_blueprintBar.SelectedElement.GetSprite(), new Vec2(xpos, ypos));
			}
			if (_layerbar != null)
			{
				_renderer.DrawSprite(_layerbar.GetSprite, _layerbar.Pos);
			}
		}
		
	}
	

	new void HandleInput()
	{
		if (Console.KeyAvailable)
		{
			ConsoleKeyInfo key = Console.ReadKey(true);
			while(Console.KeyAvailable){
				key = Console.ReadKey(true);
			}
			if (_menu.Enabled) //menu is active
			{
				switch (key.Key, key.KeyChar)
				{
					case (ConsoleKey.UpArrow, _):
						//menu selection up
						_menu.SelectUp();
						break;
					case (ConsoleKey.DownArrow, _):
						//menu selection down
						_menu.SelectDown();
						break;
					case (ConsoleKey.Enter, _):
						//confirm menu selection
						GameAction action = _menu.MenuItems[_menu.SelectedIndex].Action;
						action.Use(this);
						break;
					case (ConsoleKey.Escape, _):
						//close any modal if modal is open
						CloseModal();
						break;
					default:
						break;
				}
			}
			else if (_cursor != null && _focus == Focus.CURSOR) //cursor is active
			{
				switch (key.Key, key.KeyChar)
				{
					case (ConsoleKey.UpArrow, _):
						//cursor up
						_cursor.Move(new Vec2(0, -1));
						CursorInfo();
						break;
					case (ConsoleKey.DownArrow, _):
						//cursor down
						_cursor.Move(new Vec2(0, 1));
						CursorInfo();
						break;
					case (ConsoleKey.LeftArrow, _):
						//cursor left
						_cursor.Move(new Vec2(-1, 0));
						CursorInfo();
						break;
					case (ConsoleKey.RightArrow, _):
						//cursor right
						_cursor.Move(new Vec2(1, 0));
						CursorInfo();
						break;
					case (ConsoleKey.Spacebar, _):
						//run cursor action
						CursorAction();
						break;
					case (ConsoleKey.Escape, _):
						_menu.Enable();
						_focus = Focus.MENU;
						_editingmode = EditingMode.DISABLED;
						_persistentStatus = LM.Get("status_welcome_editor"); // Welcome
						break;
					case (_ , ']'):
						_blueprintBar.NextRow();
						break;
					case (_ , '['):
						_blueprintBar.PrevRow();
						break;
					case (ConsoleKey.Tab, _):
						_layerbar.NextLayer();
						break;
					case (ConsoleKey.Delete, _):
						_editingmode = EditingMode.DELETE;
						//TODO: deactivate layerbar
						break;						
					default:
						break;
				}
				//blueprint bar
				if(CharSet.Numbers.Contains(key.KeyChar))
				{
					if (_editingmode == EditingMode.DELETE)
					{
						_editingmode = EditingMode.INSERTSINGLE;
					}
					_blueprintBar.SelectElement(key.KeyChar);
				}
			}
			else if (_currentModal != null) //modal is active
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
		if (_cursor != null && _editingmode != EditingMode.DISABLED)
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

	private void CursorAction()
	{
		if (_cursor != null)
		{
			Vec2 targetpos = Vec2.Add(_cursor.Position, _level.RelativeCenter);
			switch (_editingmode)
			{
				case EditingMode.INSERTSINGLE:
					if (_blueprintBar.SelectedElement != null)
					{
						//set element on cursor
						_level.SetCell(_blueprintBar.SelectedChar, targetpos, _layerbar.ActiveLayer);
						DebugLogger.Log($"CursorAction: SetCell called, element at grid: {_level.Layers[_layerbar.ActiveLayer].Elements[targetpos.X, targetpos.Y]?.Name ?? "null"}");
						_level.Layers[_layerbar.ActiveLayer].UpdateSprite();
					}
					break;
				case EditingMode.DELETE:
					//delete element on cursor
					_level.SetCell('\u0000', targetpos, _layerbar.ActiveLayer);
					break;
				default:
					break;
			}
		}
	}

	private void CursorInfo()
	{
		if (_cursor != null && _editingmode != EditingMode.DISABLED)
		{
			Vec2 worldpos = Vec2.Add(_cursor.Position, _level.RelativeCenter);
			SetTransientStatus("",100);
			LevelElement? e = null;
			for (int i = 2; i >= 0; i--) //top down
			{
				e = _level.Layers[i].Elements[worldpos.X, worldpos.Y];
				if (e != null)
				{
					SetTransientStatus($"[ ]{LM.Get("cursor")}:{e.Name.PadRight(8)}| {LM.Get("pos")}: {e.Position.ToString().PadRight(7)}| {LM.Get("layer")}: {Level.LayerNames[i].PadRight(6)} | [{(e.IsBlocking ? "X" : " ")}] {LM.Get("blocking")}", 20000);
					_selectedElement = e;
					return;
				}
				//if we find nothing
				_persistentStatus = "";
				_selectedElement = null;
			}
		}
	}

	//editor actions
	protected new GameAction SetAction(ActionType type)
	{
		Action<Game> logic; // Change to Action<Game>
		switch (type)
		{
			case ActionType.EDITOR_LOAD:
				logic = (game) =>
				{
					Editor editor = (Editor)game; // Cast to Editor
					//todo
				};
				break;
			case ActionType.EDITOR_SAVE:
				logic = (game) =>
				{
					Editor editor = (Editor)game; // Cast to Editor
					//todo
				};
				break;
			case ActionType.EDITOR_EDIT:
				logic = (game) =>
				{
					Editor editor = (Editor)game; // Cast to Editor
					_editingmode = EditingMode.INSERTSINGLE;
					editor._menu.Disable();
					editor._focus = Focus.CURSOR;
				};
				break;
			case ActionType.EDITOR_QUIT:
				logic = (game) =>
				{
					Editor editor = (Editor)game; // Cast to Editor
					editor._isRunning = false; // Use editor._isRunning
				};
				break;
			case ActionType.EDITOR_HELP:
				logic = (game) =>
				{
					Editor editor = (Editor)game; // Cast to Editor
					editor._editorHelp.UpdatePage(editor._viewport.Size); // Use editor._editorHelp
					editor._currentModal = editor._editorHelp; // Assign editor._editorHelp to inherited _currentModal
					editor._menu.Disable(); // Use editor._menu
				};
				break;
			default:
				logic = (game) => { }; // Default action does nothing, already Action<Game>
				break;
		}
		return new GameAction(type, logic);
	}
}

class Cursor: Entity
{
    
    //constructor
    public Cursor()
    {
        _animations = JsonParser.LoadAnimations("CursorSprites.json", 200);
        SetAnimation(AnimDefault);
        _position = new Vec2(0,0);
    }
}

class BlueprintBar
{
	private Dictionary<char, LevelElement> _elements;
	public LevelElement? SelectedElement { get; private set;}
	public char SelectedChar { get; private set;}
	private const int _numSlots = 9; //length of bar in number of slots
	private int _selectedSlot;

	private ScreenCell _bgCell;

	//graphics
	private Sprite _sprite;

	internal BlueprintBar()
	{
		_elements = new();
		SelectedElement = null;
		SelectedChar = '.';
		_selectedSlot = -1;
		_sprite = new Sprite(new Vec2(_numSlots*2+2, 3));
		_bgCell = new ScreenCell(' ', Color.DarkRed, Color.LightGray);
		//prepopulate sprite background
		for (int i = 0; i < _sprite.Size.X; i++)
		{
			for (int j = 0; j < _sprite.Size.Y; j++)
			{
				_sprite.WriteCell(new Vec2(i, j), _bgCell);
			}
		}
		Update();
	}

	void Update(int start = 0)
	{
		_elements = BlueprintManager.GetBlueprintElements(start, _numSlots);
		int i = 0;
		foreach (var entry in _elements)
		{
			var sprite = entry.Value.GetSprite();
			if (sprite != null)
			{
				ScreenCell cell = sprite.Data[0,0];
				_sprite.Data[1,i*2+1] = cell;
				i++;
			}
			else
			{
				_sprite.Data[1,i*2+1] = _bgCell;
			}
		}
	}
	

	public Sprite GetSprite()
	{
		return _sprite;
	}

	public void NextRow(){
		int start = Math.Clamp(_selectedSlot + _numSlots, 0, BlueprintManager.NumItems);
		Update(start);
	}

	public void PrevRow(){
		int start = Math.Clamp(_selectedSlot - _numSlots, 0, BlueprintManager.NumItems);
		Update(start);
	}

	public LevelElement? SelectElement(char key) //key here is numeric index of the blueprint bar
	{
		int index = (int)char.GetNumericValue(key) - 1;
		if (index >= 0 && index < _elements.Count)
		{
			var entry = _elements.ElementAt(index);
			SelectedChar = entry.Key;
			SelectedElement = entry.Value;
		}
		//update selection marker
		for (int i = 0; i < _numSlots; i++)
		{
			if (i == index)
			{
				_sprite.Data[0,i*2+1].Character = 'v';
				_sprite.Data[2,i*2+1].Character = '^';
			}else{
				_sprite.Data[0,i*2+1].Character = ' ';
				_sprite.Data[2,i*2+1].Character = ' ';
			}
		}
		return SelectedElement;
	}
}

class LayerBar : Rect
{
	public int ActiveLayer { get; set;}
	private int _maxLayers;
	public new Sprite GetSprite { get; private set; }

	//constructor
	public LayerBar(int maxlayers, Vec2 pos, Vec2 size) : base(pos, size, 1, 1, Color.Black, Color.LightGray, Color.White, ' ')
	{
		ActiveLayer = 1;
		_maxLayers = maxlayers;
		GetSprite = base.GetSprite();
	}

	public void NextLayer()
	{
		ActiveLayer = (ActiveLayer + 1) % _maxLayers;
		Update();
	}
	
	public void PrevLayer()
	{
		ActiveLayer = (ActiveLayer - 1 + _maxLayers) % _maxLayers;
		Update();
	}

	public void Update()
	{
		GetSprite = base.GetSprite();
		for (int i = 0; i < _maxLayers; i++)
		{
			if (i == ActiveLayer)
			{
				GetSprite.Data[i+1,1].BgColor = Color.Yellow;
			}
			else
			{
				GetSprite.Data[i+1, 1].BgColor = Color.Black;
			}
			GetSprite.Data[i+1,1].Character = Level.LayerNames[i][0];
		}
				
	}
}

class EditorHelp: Modal
{
	private string _helpText = LM.Get("editor_helptext");

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