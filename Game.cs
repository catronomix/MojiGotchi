namespace MojiGotchi;

//actions enum, for player actions only
public enum ActionType
{
	NEWPET,
	QUIT,
	TOPSCORE,
	HELP,
	FEED,
	PLAY,
	PET,
	WAKE
}

class Game
{
	private const int FPS = 30;
	private const int sleepDelta = 1000 / FPS;
	private const int statUpdateIntervalMs = 250;

	private static Color programBgColor = Color.Black;
	private Renderer _renderer; // Changed to instance field
	private bool _isRunning = true;

	// Declare Rect objects as instance fields to cache them
	private Rect _menuBgRect;
	private Rect _statusBgRect;
	private SimpleRect _viewport;


	// Set area sizes
	private int _menuWidth = 21;
	private int _statusHeight = 7;

	//have a menu
	private Menu _menu;

	//have a pet
	private Pet? _pet;

	//have a level
	private Level _level;

	//have a camera
	private Camera _camera;


	//have a status
	private string _persistentStatus;
	private string _transientStatus = "";
	private DateTime _transientStatusExpires = DateTime.MinValue;
	private DateTime _lastStatUpdate = DateTime.MinValue;

	//modals
	private Modal? _currentModal;
	private Help _help;
	private HighScores _highScores;

	// Initialize the game
	public Game()
	{
		_renderer = new Renderer(programBgColor);
		// Initialize Rects for drawing the game's menu, status bar and play area
		_menuBgRect = new Rect(new Vec2(0, 0), new Vec2(_menuWidth, 1), 1, 1, Color.Yellow, Color.DarkYellow, new Color("#17303b"), '.');
		_statusBgRect = new Rect(new Vec2(_menuWidth, 0), new Vec2(1, _statusHeight), 0, 1, Color.Yellow, Color.DarkYellow, new Color("#2a34b8"), '.');
		_viewport = new Rect(new Vec2(_menuWidth, _statusHeight), new Vec2(1, 1), 0, 0, Color.White, Color.Black, Color.Black, ' ');

		//init menu
		_menu = new Menu(_menuWidth, Color.DarkGreen, Color.Green);

		//init modals
		_help = new Help();
		_highScores = new HighScores();
		_currentModal = null;

		//gameplay
		_menu.AddItem(LM.Get("menu_feed"), SetAction(ActionType.FEED), false);
		_menu.AddItem(LM.Get("menu_play"), SetAction(ActionType.PLAY), false);
		_menu.AddItem(LM.Get("menu_pet"), SetAction(ActionType.PET), false);
		_menu.AddItem(LM.Get("menu_wake"), SetAction(ActionType.WAKE), false);

		//management
		_menu.AddItem(LM.Get("menu_newgame"), SetAction(ActionType.NEWPET));
		_menu.AddItem(LM.Get("menu_quit"), SetAction(ActionType.QUIT));
		_menu.AddItem(LM.Get("menu_topscore"), SetAction(ActionType.TOPSCORE));
		_menu.AddItem(LM.Get("menu_help"), SetAction(ActionType.HELP));

		_menu.SelectFirstEnabled();
		//load saved pet from file
		_pet = DataManager.LoadPet();
		BlueprintManager.Initialize(); // Initialize blueprints before loading a level
		_level = new Level(); // This is now empty, we need to load a level
		_level.LoadFromFile("level1.txt"); // Load the level from the text file
		SimpleRect deadzone = new SimpleRect(new Vec2(-4, -2), new Vec2(8, 4));
		if (_pet != null)
		{
			UpdateMenuAvailability([ActionType.NEWPET], false);
			_menu.SelectFirstEnabled();
		}
		
		_camera = new Camera(_level, _pet, deadzone, _viewport); 

		_persistentStatus = LM.Get("status_welcome"); // Welcome
		// Call CheckWindow once at the end of the constructor to ensure all
		// dimensions are correctly set for the first render.
		CheckWindow(true);
	}

	public bool Step()
	{
		//Check window resized
		CheckWindow();
		//Game logic
		CheckStats();

		/*--------------------DRAWING--------------------*/
		// don't clear renderer when not needed
		// _renderer.ClearBuffer();
		//draw area Rects to renderer
		DrawRects();
		DrawMenuItems();

		//update camera before drawing game
		_camera.UpdateCamera();

		DrawLevelLayer(_level.BottomLayer, _level.BottomSprite);
		DrawPet();
		DrawLevelLayer(_level.MidLayer, _level.MidSprite);
		DrawLevelLayer(_level.TopLayer, _level.TopSprite);
		DrawPetBubble();
		DrawStatus();
		_renderer.RenderScreen();

		/*--------------------INPUT--------------------*/
		HandleInput();
		Thread.Sleep(sleepDelta);
		return _isRunning;
	}

	// Check if window has been resized
	private void CheckWindow(bool force = false)
	{
		Vec2 consoleSize = ConsoleHelper.GetWindowSize();
		bool resized = false;
		
		if (force)
		{
			resized = _renderer.Resize(consoleSize.X, consoleSize.Y, true);
		}
		else
		{
			resized = _renderer.Resize(consoleSize.X, consoleSize.Y);
		}

		if (resized)
		{
			// Always update the Rect properties that depend on console dimensions.
			// The Rect's setters will handle marking them as dirty if values actually change.
			// This ensures they are correctly sized on initial setup and subsequent resizes.
			_menuBgRect.Height = consoleSize.Y; // Crucial: Update menu background height

			_statusBgRect.Pos = new Vec2(_menuWidth, 0); // Ensure position is correct
			_statusBgRect.Width = consoleSize.X - _menuWidth;

			_viewport.Pos = new Vec2(_menuWidth, _statusHeight); // Ensure position is correct
			_viewport.Width = consoleSize.X - _menuWidth;
			_viewport.Height = consoleSize.Y - _statusHeight;
			_renderer.ClearBuffer();

			//update modals too
			_help.UpdatePage(_viewport.Size);
			_highScores.UpdatePage(_viewport.Size);
			_camera.UpdateCamera();
		}
	}

	void SetTransientStatus(string message, int durationMs = 5000)
	{
		if (!string.IsNullOrEmpty(message))
		{
			_transientStatus = message;
			_transientStatusExpires = DateTime.Now + TimeSpan.FromMilliseconds(durationMs);
		}
	}

	void UpdateMenuAvailability(ActionType[] actions, bool enabled)
	{
		//compare menu items's action types to provided list of action types
		foreach (MenuItem item in _menu.MenuItems)
		{
			foreach (ActionType action in actions)
			{
				if (item.Action.MyActionType == action)
				{
					if (enabled) item.Enable();
					else item.Disable();
				}
			}
		}
	}

	void HandleInput()
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

	void DrawRects()
	{
		// make sprites
		Sprite menuBgSprite = _menuBgRect.GetSprite();
		Sprite statusBgSprite = _statusBgRect.GetSprite();
		
		//check if modal is active
		if (_currentModal != null)
		{
			_renderer.DrawSprite(_currentModal.GetSpriteBg(), _viewport.Pos);
			_renderer.DrawSprite(_currentModal.GetSpriteContent(), _viewport.Pos);
		}
		
		_renderer.DrawSprite(menuBgSprite, _menuBgRect.Pos);
		_renderer.DrawSprite(statusBgSprite, _statusBgRect.Pos);
		
	}

	void DrawMenuItems()
	{
		int index = 0;
		int selected = _menu.SelectedIndex;
		foreach (MenuItem item in _menu.MenuItems)
		{
			Sprite mySprite = item.Sprite;
			// add > and < markers around active menu item by updating char in sprite data
			if (index == selected)
			{
				mySprite.Data[1, 1].Character = '>';
				if (mySprite.Size.X >= 2) mySprite.Data[1, mySprite.Size.X - 2].Character = '<';
			}
			else
			{
				mySprite.Data[1, 1].Character = ' ';
				if (mySprite.Size.X >= 2) mySprite.Data[1, mySprite.Size.X - 2].Character = ' ';
			}
			_renderer.DrawSprite(mySprite, new Vec2(_menuBgRect.Pos.X + 1, _menuBgRect.Pos.Y + index * 4 + 1));
			index++;
		}
	}

	void DrawPet()
	{
		if (_pet != null && _currentModal == null)
		{
			_pet.Communicate();
			Sprite? petSprite = _pet.GetSprite(); // Capture the sprite once
			if (petSprite != null)
			{
				// 2. Calculate Top-Left based on Pet's World Position and its Pivot (center)
				// Pet position (0,0) is world center.
				// We subtract petSprite.Size / 2 to make (0,0) the center of the pet.
				Vec2 drawPos = Vec2.Add(_camera.GetAbsCenter(), _pet.Position.Sum(-1,-1));
				_renderer.DrawSprite(petSprite, drawPos, _viewport);
				//draw message bubble	
			}
		}
	}

	void DrawPetBubble()
	{
		if (_pet != null && _currentModal == null)
		{
			Sprite? bubble = _pet.MessageBubble.GetSprite();
			if (bubble != null)
				{
					Vec2 drawPos = Vec2.Add(_camera.GetAbsCenter(), _pet.Position.Sum(-1,-1));
					_renderer.DrawSprite(bubble, drawPos.Sum(-bubble.Size.X / 2+1, -3), _viewport);
				}
		}
	}

	void DrawStatus()
	{
		// Clear transient status if it has expired
		if (DateTime.Now > _transientStatusExpires)
		{
			_transientStatus = "";
			if (_pet != null)
			{
				if (!_pet.IsSleeping)
				{
					_pet.SetAnimation(Pet.AnimDefault);
				}
			}
		}

		// Draw persistent status on line 2.
		// The ANSI codes embedded in _persistentStatus will handle coloring.
		int persistentY = _statusBgRect.Pos.Y + 2;
		int persistentX = _statusBgRect.Pos.X + _statusBgRect.Width / 2 - _persistentStatus.Length / 2;
		_renderer.DrawText(_persistentStatus, new Vec2(persistentX, persistentY), Color.White);

		// Draw transient status on line 4.
		if (!string.IsNullOrEmpty(_transientStatus))
		{
			int transientY = _statusBgRect.Pos.Y + 4;
			int transientX = _statusBgRect.Pos.X + _statusBgRect.Width / 2 - _transientStatus.Length / 2;
			_renderer.DrawText(_transientStatus, new Vec2(transientX, transientY), Color.LightYellow);
		}
	}

	void CheckStats()
	{
		if (_pet != null && _lastStatUpdate + TimeSpan.FromMilliseconds(statUpdateIntervalMs) < DateTime.Now)
		{
			string deathMessage = _pet.UpdateAllStats();
			if (deathMessage != "")
			{
				//add pet to high scores
				DataManager.AddHighScore(_pet); // Add high score
				_persistentStatus = deathMessage;
				KillPet();
				return;
			}
			else // Pet is alive, update stats display
			{
				_persistentStatus = LM.Get("status_caring", [_pet.Name]) + " | "
				+ LM.Get("status_stats", [_pet.Saturation.Value, _pet.Saturation.Max,
				_pet.Mood.Value, _pet.Mood.Max, _pet.Energy.Value, _pet.Energy.Max,
				_pet.Sleepyness.Value, _pet.Sleepyness.Max,]) + 
				(_pet.IsSleeping ? " | " + LM.Get("status_sleeping") : "");
			}
			_lastStatUpdate = DateTime.Now;

			//update menu availability
			if (_pet.IsSleeping)
			{
				UpdateMenuAvailability([ActionType.WAKE], true);
				UpdateMenuAvailability([ActionType.FEED, ActionType.PLAY, ActionType.PET], false);
				_pet.SetAnimation(Pet.AnimSleeping);
			}
			else
			{
				UpdateMenuAvailability([ActionType.WAKE], false);
				UpdateMenuAvailability([ActionType.FEED, ActionType.PLAY, ActionType.PET], true);
				Vec2 lastpetpos = _pet.Position;
				_pet.Wander();
				
				//keep pet inside level
				int x = Math.Clamp(_pet.Position.X, -_level.RelativeCenter.X, _level.RelativeCenter.X);
				int y = Math.Clamp(_pet.Position.Y, -_level.RelativeCenter.Y, _level.RelativeCenter.Y);
				_pet.Position = new Vec2(x, y);

				//keep pet from moving into solid objects
				if (_level != null)
				{
					foreach(LevelElement element in _level.MidLayer)
					{
						if (element.IsBlocking)
						{
							// check if colliding
							// The pet's collision point is its center cell, which is _pet.Position + (1,1)
							if(Vec2.Chebyshev(ElementPosInWorldSpace(element), _pet.Position) < 2)
							{
								_pet.Position = lastpetpos;
							}
						}
					}
				}
			}
		}
	}

	void KillPet()
	{
		_pet = null;
		//update menu
		UpdateMenuAvailability([ActionType.FEED, ActionType.PLAY, ActionType.PET, ActionType.WAKE], false);
		UpdateMenuAvailability([ActionType.NEWPET], true);
		_menu.SelectFirstEnabled();
		// The death message is already set in _persistentStatus by CheckStats.
		SoundManager.Play("death.wav");
		SetTransientStatus("Begin een nieuw spel om een nieuwe MojiGotchi te krijgen.");
	}

	//game actions
	private GameAction SetAction(ActionType type)
	{
		Action<Game> logic;
		switch (type)
		{
			case ActionType.NEWPET:
				logic = (game) =>
				{
					game._pet = new Pet();
					//provide new pet to camera
					game._camera.SetPet(game._pet);
					//replace default colors from animation to random colors
					game._pet.RandomizePetColor();
					game.UpdateMenuAvailability([ActionType.FEED, ActionType.PLAY, ActionType.PET, ActionType.WAKE], true);
					game.UpdateMenuAvailability([ActionType.NEWPET], false);
					game._persistentStatus = LM.Get("status_caring", [game._pet.Name]);
					game._menu.SelectFirstEnabled();
					SoundManager.Play("newgame.wav");
				};
				break;
			case ActionType.FEED:
				logic = (game) =>
				{
					if (game._pet != null)
					{
						game.SetTransientStatus(game._pet.Feed());
					}
				};
				break;
			case ActionType.PLAY:
				logic = (game) =>
				{
					if (game._pet != null)
					{
						game.SetTransientStatus(game._pet.Play());
					}
				};
				break;
			case ActionType.PET:
				logic = (game) =>
				{
					if (game._pet != null)
					{
						game.SetTransientStatus(game._pet.PetPet());
					}
				};
				break;
			case ActionType.WAKE:
				logic = (game) =>
				{
					if (game._pet != null)
					{
						game.SetTransientStatus(game._pet.WakeUp());
					}
				};
				break;
			case ActionType.QUIT:
				logic = (game) =>
				{
					if (game._pet != null) { DataManager.SavePet(game._pet); } else { DataManager.DeleteSave(); }
					game._isRunning = false;
				};
				break;
			case ActionType.TOPSCORE:
				logic = (game) =>
				{
					game._highScores.UpdatePage(_viewport.Size);
					game._currentModal = _highScores;
				};
				break;
			case ActionType.HELP:
				logic = (game) =>
				{
					game._help.UpdatePage(_viewport.Size);
					game._currentModal = _help;
				};
				break;
			default:
				logic = (game) => { }; // Default action does nothing
				break;
		}
		return new GameAction(type, logic);
	}

	private void CloseModal()
	{
		_currentModal = null;
	}

	private void DrawLevelLayer(List<LevelElement> layer, Sprite? layerSprite)
	{
		if (_level == null || layerSprite == null || _currentModal != null) return;

		// Level.SetSprite fills the layerSprite (which is level-sized)
		Level.SetSprite(layer, layerSprite);
		Vec2 drawPos = Vec2.Subtract(_camera.GetAbsCenter(), _level.RelativeCenter);
		_renderer.DrawSprite(layerSprite, drawPos, _viewport);
	}

	private Vec2 ElementPosInWorldSpace(LevelElement element)
	{
		if (_level == null) return new Vec2(0, 0);
		return Vec2.Subtract(element.Position, _level.RelativeCenter);
	}
}