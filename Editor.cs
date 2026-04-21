using System.Data;
using System.Runtime.Intrinsics.X86;

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
	SELECT,
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
	private FillRect _editrect;
	private LevelElement? _selectedElement;

	//have modals
	protected EditorHelp _editorHelp; // Help page
	protected EditorConfig _editorConfig; // Level configuration and any other editor options

	// have layer management
	private LayerBar _layerbar;
	private bool _shade;
	private const float _shadeAmount = 0.66f;

	//have a blueprint bar
	BlueprintBar _blueprintBar;

	//be able to focus controls
	private Focus _focus;

	// Initialize the editor
	public Editor(bool devmode) : base(devmode)
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
		_editorHelp = new EditorHelp("Editor Help", Color.DarkGray, Color.White); // Initialize editor-specific help
		_editorConfig = new EditorConfig(this); // Initialize editor-specific configuration

		//menu

		//editor
		_menu.AddItem(LM.Get("editor_menu_edit"), SetEditorAction(ActionType.EDITOR_EDIT), Color.WoodDark);
		_menu.AddItem(LM.Get("editor_menu_config"), SetEditorAction(ActionType.EDITOR_CONFIG), Color.WoodDark);
		_menu.AddItem(LM.Get("editor_menu_save"), SetEditorAction(ActionType.EDITOR_SAVE), Color.WoodDark);
		_menu.AddItem(LM.Get("editor_menu_revert"), SetEditorAction(ActionType.EDITOR_LOAD), Color.WoodDark);
		
		//modals
		_menu.AddItem(LM.Get("menu_help"), SetEditorAction(ActionType.EDITOR_HELP), Color.DarkGreen);
		_menu.AddItem(LM.Get("editor_menu_quit"), SetEditorAction(ActionType.EDITOR_QUIT), Color.DarkGreen);

		_menu.SelectFirstEnabled();
		
		//has cursor
		_cursor = new Cursor();
		_cursor.SetAnimation("SELECT");
		_selectedElement = null;
		_focus = Focus.MENU;
		_editrect = new FillRect(_cursor.Pos, new Vec2(0,0));
		//has blueprint bar
		_blueprintBar = new BlueprintBar();
		_shade = false;

		//set active layer
		_layerbar = new LayerBar(_level.Layers.Length, new Vec2(-5, 3).Sum(_viewport.Right, _viewport.Top), new Vec2(3, 5));
		_layerbar.Update();

		BlueprintManager.Initialize(); // Initialize blueprints before loading a level
		_level = new Level(); // This is now empty, we need to load a level
		_level.LoadFromFile("level1.json"); // Load the level from the text file

		_persistentStatus = LM.Get("editor_status_welcome"); // Welcome
		// Call CheckWindow once at the end of the constructor to ensure all
		// dimensions are correctly set for the first render.

		// has camera
		SimpleRect deadzone = new SimpleRect(new Vec2(-4, -2), new Vec2(8, 4));
		_camera = new Camera(_level, _cursor, deadzone, _viewport);

		CheckWindow(true);
		_editingmode = EditingMode.DISABLED;
	}

	public new LoopResult Step()
	{
		//Check window resized
		CheckWindow();

		/*--------------------DRAWING--------------------*/
		// don't clear renderer when not needed
		// _renderer.ClearBuffer();
		//draw area Rects to renderer
		DrawRects();
		DrawMenuItems();
		DrawModalOptions();

		//update camera before drawing editor
		_camera.UpdateCamera();

		foreach(LevelLayer layer in _level.Layers)
		{
			if (layer != _level.Layers[_layerbar.ActiveLayer] && _shade)
			{
				DrawLevelLayer(layer, true, _shadeAmount);
			}
			else if (layer == _level.Layers[_layerbar.ActiveLayer])
			{
				DrawLevelLayer(layer);
				//draw edit rect if needed
				if (_editingmode == EditingMode.INSERTRECTSTART && _cursor != null)
				{
					_renderer.DrawSprite(_editrect.GetSprite(), Vec2.Add(_editrect.Pos, _camera.GetAbsCenter()), _viewport);
				}
			}
			else
			{
				DrawLevelLayer(layer);
			}
			
		}

		DrawStatus();
		DrawCursor();
		DrawGlyphs();
		_renderer.RenderScreen();

		/*--------------------INPUT--------------------*/
		HandleInput();
		Thread.Sleep(sleepDelta);
		return _loopresult;
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
		if (_editingmode > 0 && _blueprintBar.SelectedElement != null){ //when editor is active
			String persistentstring = "[ ] " + LM.Get("selected")+": " + (_blueprintBar.SelectedElement.Name + (_blueprintBar.SelectedElement.IsBlocking ? $" ({LM.Get("blocking")})" : "")).PadRight(18) +" | ";

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
			_persistentStatus = persistentstring;
		}
		else if(_editingmode == EditingMode.DELETE)
		{
			_persistentStatus = LM.Get("editor_editmode") + ": " + LM.Get("editor_editmode_delete");
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
				int xpos = _statusBgRect.Left + _statusBgRect.RelativeCenter.X - _persistentStatus.Length / 2 +1;
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
						DebugLogger.Log("editor option action chosen");
						break;
					case (ConsoleKey.Escape, _):
						//close any modal if modal is open
						CloseModal();
						break;
					default:
						break;
				}
			}
			else if (_editingmode == EditingMode.DISABLED && _currentModal != null) //editor is not active
			{
				switch (key.Key)
				{
				case ConsoleKey.Escape:
					//close any modal if modal is open
					CloseModal();
					break;
				case ConsoleKey.LeftArrow:
					_currentModal.SelectLeft();
					break;
				case ConsoleKey.RightArrow:
					_currentModal.SelectRight();
					break;
				case ConsoleKey.Enter:
					GameAction action = _currentModal.Options[_currentModal.SelectedIndex].Action;
					action.Use(this);
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
						if (_cursor.Pos.Y > -(_level.Size.Y -_level.RelativeCenter.Y))
						{
							_cursor.Move(new Vec2(0, -1));
							CursorInfo();
							EditRect();
						}
						break;
					case (ConsoleKey.DownArrow, _):
						//cursor down
						if (_cursor.Pos.Y < (_level.Size.Y -_level.RelativeCenter.Y -1))
						{
							_cursor.Move(new Vec2(0, 1));
							CursorInfo();
							EditRect();
						}
						break;
					case (ConsoleKey.LeftArrow, _):
						//cursor left
						if (_cursor.Pos.X > -(_level.Size.X -_level.RelativeCenter.X))
						{
							_cursor.Move(new Vec2(-1, 0));
							CursorInfo();
							EditRect();
						}
						break;
					case (ConsoleKey.RightArrow, _):
						//cursor right
						if (_cursor.Pos.X < (_level.Size.X -_level.RelativeCenter.X -1))
						{
							_cursor.Move(new Vec2(1, 0));
							CursorInfo();
							EditRect();
						}
						break;
					case (ConsoleKey.Spacebar, _):
						//run cursor action
						CursorAction();
						break;
					case (ConsoleKey.Escape, _):
						if (_editingmode == EditingMode.INSERTRECTSTART)
						{
							_editingmode = EditingMode.INSERTSINGLE;
							_editrect.Init(_cursor.Pos);
							_cursor.SetAnimation("EDIT");
							break;
						}
						if (_editingmode == EditingMode.SELECT)
						{
							_menu.Enable();
							_focus = Focus.MENU;
							_persistentStatus = LM.Get("editor_status_welcome"); // Welcome
							_blueprintBar.SelectElement('0');
							_editingmode = EditingMode.DISABLED;
						}else{
							_editingmode = EditingMode.SELECT;
							_cursor.SetAnimation("SELECT");
						}
						break;
					case (_ , ']'):
						// _blueprintBar.SelectElement('0');
						_blueprintBar.NextRow();
						break;
					case (_ , '['):
						// _blueprintBar.SelectElement('0');
						_blueprintBar.PrevRow();
						break;
					case (ConsoleKey.Tab, _):
						_layerbar.NextLayer();
						break;
					case (ConsoleKey.S, _):
						//toggle layer shading
						_shade = !_shade;
						break;
					case (_, '0'):
						_cursor.SetAnimation("DELETE");
						_blueprintBar.SelectElement('0');
						_editingmode = EditingMode.DELETE;
						//TODO: deactivate layerbar
						break;
					case (ConsoleKey.R, _):
						//rectangle mode
						if (_editingmode == EditingMode.INSERTSINGLE || _editingmode == EditingMode.DELETE)
						{
							if (_editingmode == EditingMode.DELETE)
							{
								_cursor.SetAnimation("RECTDELETE");
							}
							else
							{
							_cursor.SetAnimation("RECT");
							}
							_editingmode = EditingMode.INSERTRECTSTART;
							_editrect.Init(_cursor.Pos);
							
						}
						break;
					case (ConsoleKey.F, _):
						//fill once
						PaintFill();
						break;
					default:
						break;
				}
				//blueprint bar
				if(CharSet.Numbers.Contains(key.KeyChar))
				{
					_editingmode = EditingMode.INSERTSINGLE;
					_blueprintBar.SelectElement(key.KeyChar);
					_cursor.SetAnimation("EDIT");
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
				Vec2 drawPos = Vec2.Add(_camera.GetAbsCenter(), _cursor.Pos.Sum(-1,-1));
				_renderer.DrawSprite(cursorSprite, drawPos, _viewport);
			}
		}
	}

	private void CursorAction()
	{
		if (_cursor != null)
		{
			Vec2 targetpos = Vec2.Add(_cursor.Pos, _level.RelativeCenter);
			switch (_editingmode)
			{
				case EditingMode.INSERTSINGLE:
					if (_blueprintBar.SelectedElement != null)
					{
						//set element on cursor
						_level.SetCell(_blueprintBar.SelectedKey, targetpos, _layerbar.ActiveLayer);
						// DebugLogger.Log($"CursorAction: SetCell called, element at grid: {_level.Layers[_layerbar.ActiveLayer].Elements[targetpos.X, targetpos.Y]?.Name ?? "null"}");
						_level.Layers[_layerbar.ActiveLayer].UpdateSprite();
					}
					break;
				case EditingMode.DELETE:
					//delete element on cursor
					_level.SetCell("..", targetpos, _layerbar.ActiveLayer);
					_editingmode = EditingMode.DELETE;
					break;
				case EditingMode.INSERTRECTSTART:
					//insert rect end
					_editingmode = EditingMode.INSERTRECTEND;
					PaintRect();
					break;
				default:
					break;
			}
			CursorInfo();
		}
	}

	private void EditRect(){
		if (_cursor != null && _editingmode == EditingMode.INSERTRECTSTART)
		{
			if(_editrect != null)
			{
				Sprite? sprite = _blueprintBar.SelectedElement?.GetSprite();
				ScreenCell cell = new ScreenCell(' ', Color.Black, Color.Black);
				if (sprite != null)
				{
					cell = sprite.Data[0,0];	
				}
					
					_editrect.Update(_cursor.Pos, cell);				
			}
		}
	}

	private void PaintRect()
	{
		if (_editrect != null && _editingmode == EditingMode.INSERTRECTEND && _cursor != null)
		{
			for (int x = 0; x < _editrect.Size.X; x++)
			{
				for (int y = 0; y < _editrect.Size.Y; y++)
				{
					_level.SetCell(_blueprintBar.SelectedKey, Vec2.Add(_editrect.Pos.Sum(x, y), _level.RelativeCenter), _layerbar.ActiveLayer);
				}
			}
			_level.Layers[_layerbar.ActiveLayer].UpdateSprite();
			_editrect.Init(_cursor.Pos);
			_editingmode = EditingMode.INSERTSINGLE;
			_cursor.SetAnimation("EDIT");
		}
	}

	private void PaintFill()
	{
		if (_cursor != null && (_editingmode == EditingMode.INSERTSINGLE || _editingmode == EditingMode.DELETE))
		{
			Vec2 origin = Vec2.Add(_cursor.Pos, _level.RelativeCenter);
			LevelElement?[,] grid = _level.Layers[_layerbar.ActiveLayer].Elements;
			LevelElement? fillElement = _blueprintBar.SelectedElement;

			int rows = grid.GetLength(0);
			int cols = grid.GetLength(1);

			// Boundary check for the starting point
			if (origin.X < 0 || origin.X >= rows || origin.Y < 0 || origin.Y >= cols)
				return;

			string targetKey = grid[origin.X, origin.Y]?.Key?? "..";
			string? replaceKey = _blueprintBar.SelectedKey;

			// If the target character is already the replacement character, no work is needed
			if (targetKey == replaceKey)
				return;

			Stack<Vec2> stack = new Stack<Vec2>();
			stack.Push(origin);

			while (stack.Count > 0)
			{
				Vec2 pos = stack.Pop();

				// Check boundaries
				if (pos.X < 0 || pos.X >= rows || pos.Y < 0 || pos.Y >= cols)
					continue;

				// Check if the current cell matches the starting character
				LevelElement? currentElement = grid[pos.X, pos.Y];
				string currentKey = currentElement?.Key ?? "..";
				if (currentKey == targetKey)
				{
					// Replace the cell (creating a new instance to avoid reference issues)
					_level.SetCell(replaceKey, pos, _layerbar.ActiveLayer);

					// Add neighbors (4-way connectivity)
					stack.Push(new Vec2(pos.X + 1, pos.Y));
					stack.Push(new Vec2(pos.X - 1, pos.Y));
					stack.Push(new Vec2(pos.X, pos.Y + 1));
					stack.Push(new Vec2(pos.X, pos.Y - 1));
				}
			}
		}
	}
	

	private void CursorInfo()
	{
		if (_cursor != null && _editingmode != EditingMode.DISABLED)
		{
			Vec2 worldpos = Vec2.Add(_cursor.Pos, _level.RelativeCenter);
			//clamp worldpos to level dimension
			worldpos.X = Math.Clamp(worldpos.X, 0, _level.Size.X - 1);
			worldpos.Y = Math.Clamp(worldpos.Y, 0, _level.Size.Y - 1);

			SetTransientStatus("",100);
			LevelElement? e = null;
			for (int i = 2; i >= 0; i--) //top down
			{
				e = _level.Layers[i].Elements[worldpos.X, worldpos.Y];
				if (e != null)
				{
					SetTransientStatus($"[ ]{LM.Get("cursor")}:{(e.Name + (e.IsBlocking ? $" ({LM.Get("blocking")})" : "")).PadRight(18)}| {LM.Get("pos")}: {e.Pos.ToString().PadRight(7)}| {LM.Get("layer")}: {Level.LayerNames[i].PadRight(6)} |", 20000);
					_selectedElement = e;
					return;
				}
				//if we find nothing
				_transientStatus = "";
				_selectedElement = null;
			}
		}
	}

	private void ResizeLevel(int h, int v){
		_level.Resize(_level.Size.X+h*2, _level.Size.Y+v*2);
	}

	//editor actions
	internal GameAction SetEditorAction(ActionType type)
	{
		Action<Game> logic; // Change to Action<Game>
		switch (type)
		{
			case ActionType.EDITOR_LOAD:
				logic = (game) =>
				{
					Editor editor = (Editor)game; // Cast to Editor
					if (editor._level != null)
					{
						editor._level.LoadFromFile("level1.json");
					}
				};
				break;
			case ActionType.EDITOR_SAVE:
				logic = (game) =>
				{
					Editor editor = (Editor)game; // Cast to Editor
					if (editor._level != null)
					{
						editor._level.SaveToFile("level1.json");
					}
				};
				break;
			case ActionType.EDITOR_EDIT:
				logic = (game) =>
				{
					Editor editor = (Editor)game; // Cast to Editor
					_editingmode = EditingMode.SELECT;
					editor._menu.Disable();
					editor._focus = Focus.CURSOR;
				};
				break;
			case ActionType.EDITOR_CONFIG:
				logic = (game) =>
				{
					Editor editor = (Editor)game; // Cast to Editor
					editor._editorConfig.UpdatePage(editor._viewport.Size); // Use editor._editorHelp
					editor._currentModal = editor._editorConfig; // Assign editor._editorHelp to inherited _currentModal
					editor._menu.Disable(); // Use editor._menu
				};
				break;
			case ActionType.EDITOR_QUIT:
				logic = (game) =>
				{
					Editor editor = (Editor)game; // Cast to Editor
					editor._loopresult = LoopResult.GOTOGAME; // Use editor._isRunning
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
			//resize level actions
			case ActionType.EDITOR_SIZEHPLUS:
				logic = (game) =>
				{
					Editor editor = (Editor)game; // Cast to Editor
					editor.ResizeLevel(1, 0);
				};
				break;
			case ActionType.EDITOR_SIZEHMIN:
				logic = (game) =>
				{
					Editor editor = (Editor)game; // Cast to Editor
					editor.ResizeLevel(-1, 0);
				};
				break;
			case ActionType.EDITOR_SIZEVPLUS:
				logic = (game) =>
				{
					Editor editor = (Editor)game; // Cast to Editor
					editor.ResizeLevel(0, 1);
				};
				break;
			case ActionType.EDITOR_SIZEVMIN:
				logic = (game) =>
				{
					Editor editor = (Editor)game; // Cast to Editor
					editor.ResizeLevel(0, -1);
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
		_animations = JsonParser.LoadAnimations("CursorSprites.json");
		SetAnimation("SELECT");
		_pos = new Vec2(0,0);
	}
}

class BlueprintBar
{
	private Dictionary<string, LevelElement> _elements;
	public LevelElement? SelectedElement { get; private set;}
	public string SelectedKey { get; private set;}
	private const int _numSlots = 9; //length of bar in number of slots
	private int _selectedSlot;

	private ScreenCell _bgCell;

	//graphics
	private Sprite _sprite;

	internal BlueprintBar()
	{
		_elements = new();
		SelectedElement = null;
		SelectedKey = "..";
		_selectedSlot = -1;
		_sprite = new Sprite(new Vec2(_numSlots*2+3, 3));
		_bgCell = new ScreenCell('V', Color.BabyWhite, Color.BabyWhite);
		//prepopulate sprite background
		for (int i = 0; i < _sprite.Size.X; i++)
		{
			for (int j = 0; j < _sprite.Size.Y; j++)
			{
				_sprite.WriteCell(new Vec2(i, j), _bgCell);
			}
		}

		// prev/next indication
		ScreenCell prevcell = new ScreenCell('[', Color.Black, Color.BabyWhite);
		_sprite.WriteCell(new Vec2(0,1), prevcell);
		ScreenCell nextcell = new ScreenCell(']', Color.Black, Color.BabyWhite);
		_sprite.WriteCell(new Vec2(_numSlots*2+2,1), nextcell);

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
				_sprite.Data[1,i*2+2] = cell;
				_sprite.Data[2,i*2+2].Character = (i+1).ToString()[0];
				_sprite.Data[2,i*2+2].Color = Color.Black;
				i++;
			}
		}
		for (int j = i; j < _numSlots; j++)
		{
			_sprite.Data[1,j*2+2] = _bgCell;
		}
	}
	

	public Sprite GetSprite()
	{
		return _sprite;
	}

	public void NextRow(){
		_selectedSlot = Math.Clamp(_selectedSlot + _numSlots, 0, BlueprintManager.NumItems-_numSlots);
		Update(_selectedSlot);
	}

	public void PrevRow(){
		_selectedSlot = Math.Clamp(_selectedSlot - _numSlots, 0, BlueprintManager.NumItems-_numSlots);
		Update(_selectedSlot);
	}

	public LevelElement? SelectElement(char key) //key here is numeric index of the blueprint bar
	{
		bool changed = false;
		int index = (int)char.GetNumericValue(key) - 1;
		if (index >= 0 && index < _elements.Count)
		{
			var entry = _elements.ElementAt(index);
			SelectedKey = entry.Key;
			SelectedElement = entry.Value;
			changed = true;
		}
		else if (index < 0)
		{
			SelectedKey = "..";
			SelectedElement = null;
			changed = true;
		}
		if (changed)
		{
			//update selection marker
			for (int i = 0; i < _numSlots; i++)
			{
				if (i == index)
				{
					_sprite.Data[0,i*2+2].Color = Color.Black;
				}else{
					_sprite.Data[0,i*2+2] = _bgCell;
				}
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
	public LayerBar(int maxlayers, Vec2 pos, Vec2 size) : base(pos, size, 1, 1, Color.Black, Color.BabyWhite, Color.BabyWhite, ' ')
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
				GetSprite.Data[_maxLayers - i,1].BgColor = Color.Yellow;
			}
			else
			{
				GetSprite.Data[_maxLayers - i, 1].BgColor = Color.Black;
			}
			GetSprite.Data[_maxLayers - i,1].Character = Level.LayerNames[i][0];
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

class EditorConfig: Modal
{
	private string _configIntroText = LM.Get("editor_config_introtext");

	//constructor
	public EditorConfig(Editor editor) : base("Editor Config", Color.DarkYellow, Color.Yellow)
	{
		base.AddMenuItem("Horizontal +", editor.SetEditorAction(ActionType.EDITOR_SIZEHPLUS), Color.WoodDark);
		base.AddMenuItem("Horizontal -", editor.SetEditorAction(ActionType.EDITOR_SIZEHMIN), Color.WoodDark);
		base.AddMenuItem("Vertical +", editor.SetEditorAction(ActionType.EDITOR_SIZEVPLUS), Color.WoodDark);
		base.AddMenuItem("Vertical -", editor.SetEditorAction(ActionType.EDITOR_SIZEVMIN), Color.WoodDark);
	}

	public void UpdatePage(Vec2 size)
	{
		SetSpriteBg(size);
		ClearContentSprite(size);
		string[] lines = _configIntroText.Split('\n');

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