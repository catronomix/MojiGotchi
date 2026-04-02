namespace MojiGotchi;

class Game
{
	protected const int FPS = 30;
	protected const int sleepDelta = 1000 / FPS;
	protected const int statUpdateIntervalMs = 250;

	protected static Color programBgColor = Color.Black;
	protected Renderer _renderer; // Changed to instance field
	protected bool _isRunning = true;

	// Declare Rect objects as instance fields to cache them
	protected Rect _menuBgRect;
	protected Rect _statusBgRect;
	protected SimpleRect _viewport;


	// Set area sizes
	protected int _menuWidth = 21;
	protected int _statusHeight = 7;

	//have a menu
	protected Menu _menu;

	//have a pet
	protected Pet? _pet;

	//have a level
	protected Level _level;

	//have a camera
	protected Camera _camera;


	//have a status
	protected string _persistentStatus;
	protected string _transientStatus = "";
	protected DateTime _transientStatusExpires = DateTime.MinValue;
	protected DateTime _lastStatUpdate = DateTime.MinValue;

	//modals
	protected Modal? _currentModal;
	protected Help _help;
	protected HighScores _highScores;
	protected LanguageChoice _languageChoice;

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
		_languageChoice = new LanguageChoice();
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
		_level.SpawnPoint = new Vec2(2, -3);
		
		SimpleRect deadzone = new SimpleRect(new Vec2(-4, -2), new Vec2(8, 4));
		if (_pet != null)
		{
			_pet.Pos = _level.SpawnPoint;
			UpdateMenuAvailability([ActionType.NEWPET], false);
			_menu.SelectFirstEnabled();
		}
		
		_camera = new Camera(_level, _pet, deadzone, _viewport); 

		_persistentStatus = LM.Get("status_welcome"); // Welcome
		// Call CheckWindow once at the end of the constructor to ensure all
		// dimensions are correctly set for the first render.
		CheckWindow(true);
	}

	public void ChooseLanguage()
	{
		_languageChoice.UpdatePage(_viewport.Size);
		_currentModal = _languageChoice;
		_menu.Disable();
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

		DrawLevelLayer(_level.Layers[0]);
		DrawPet();
		DrawLevelLayer(_level.Layers[1]);
		DrawLevelLayer(_level.Layers[2]);
		DrawPetBubble();
		DrawStatus();
		_renderer.RenderScreen();

		/*--------------------INPUT--------------------*/
		HandleInput();
		Thread.Sleep(sleepDelta);
		return _isRunning;
	}

	// Check if window has been resized
	protected void CheckWindow(bool force = false)
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

	protected void SetTransientStatus(string message, int durationMs = 5000)
	{
		if (!string.IsNullOrEmpty(message))
		{
			_transientStatus = message;
			_transientStatusExpires = DateTime.Now + TimeSpan.FromMilliseconds(durationMs);
		}
	}

	protected void UpdateMenuAvailability(ActionType[] actions, bool enabled)
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

	protected void HandleInput()
	{
		if(Console.KeyAvailable)
		{
			ConsoleKeyInfo key = Console.ReadKey(true);
			while (Console.KeyAvailable)
			{
				key = Console.ReadKey(true);
			}
			
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

	protected void DrawRects()
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

	protected void DrawMenuItems()
	{
		if (_menu.Enabled){
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
	}

	void DrawPet()
	{
		if (_pet != null && _currentModal == null)
		{
			_pet.Communicate();
			Sprite? petSprite = _pet.GetSprite(); // Capture the sprite once
			if (petSprite != null)
			{
				// 2. Calculate Top-Left based on Pet's World Pos and its Pivot (center)
				// Pet position (0,0) is world center.
				// We subtract petSprite.Size / 2 to make (0,0) the center of the pet.
				Vec2 drawPos = Vec2.Add(_camera.GetAbsCenter(), _pet.Pos.Sum(-1,-1));
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
					Vec2 drawPos = Vec2.Add(_camera.GetAbsCenter(), _pet.Pos.Sum(-1,-1));
					_renderer.DrawSprite(bubble, drawPos.Sum(-bubble.Size.X / 2+1, -3), _viewport);
				}
		}
	}

	protected void DrawStatus(int spacing = 2)
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
		int persistentY = _statusBgRect.Pos.Y + spacing;
		int persistentX = _statusBgRect.Pos.X + _statusBgRect.Width / 2 - _persistentStatus.Length / 2;
		_renderer.DrawText(_persistentStatus, new Vec2(persistentX, persistentY), Color.White);

		// Draw transient status on line 4.
		if (!string.IsNullOrEmpty(_transientStatus))
		{
			int transientY = _statusBgRect.Pos.Y + 2 * spacing;
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
				Vec2 lastpetpos = _pet.Pos;
				_pet.Wander();
				ConstrainEntity(_pet, _level);
				CheckCollision(_pet, lastpetpos, new Vec2(1,1));				
			}
		}
	}

	void ConstrainEntity(Entity entity, SimpleRect rect)
	{
		
		int x = Math.Clamp(entity.Pos.X, -rect.RelativeCenter.X, rect.RelativeCenter.X);
		int y = Math.Clamp(entity.Pos.Y, -rect.RelativeCenter.Y, rect.RelativeCenter.Y);
		rect.Pos = new Vec2(x, y);
	}

	void CheckCollision(Entity entity, Vec2 lastpos, Vec2? pivot)
	{
		//keep entity from moving into solid objects
		if (_level != null && entity.Hitbox != null)
		{
			foreach (Vec2? cell in entity.Hitbox)
			{
				if (cell != null)
				{
					Vec2 worldpos = Vec2.Add(entity.Pos, _level.RelativeCenter);
					worldpos = Vec2.Add(worldpos, (Vec2)cell);
					if (pivot != null)
					{
						worldpos = Vec2.Subtract(worldpos, (Vec2)pivot);
					}
					LevelElement element = _level.Layers[1].Elements[worldpos.X, worldpos.Y];
					if (element != null && element.IsBlocking)
					{
						entity.Pos = lastpos;
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
	protected GameAction SetAction(ActionType type)
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
					if (game._pet != null)
					{ 
						DataManager.SavePet(game._pet);
					}
					else
					{
						DataManager.DeleteSave();
					}
					game._isRunning = false;
				};
				break;
			case ActionType.TOPSCORE:
				logic = (game) =>
				{
					game._highScores.UpdatePage(_viewport.Size);
					game._currentModal = _highScores;
					_menu.Disable();
				};
				break;
			case ActionType.HELP:
				logic = (game) =>
				{
					game._help.UpdatePage(_viewport.Size);
					game._currentModal = _help;
					_menu.Enable();
				};
				break;
			default:
				logic = (game) => { }; // Default action does nothing
				break;
		}
		return new GameAction(type, logic);
	}

	protected void CloseModal()
	{
		_currentModal = null;
		_menu.Enable();
	}

	protected void DrawLevelLayer(LevelLayer layer, bool update = true)
	{
		if (_level == null || layer == null || _currentModal != null) return;

		// Level.SetSprite fills the layerSprite (which is level-sized)
		if (update)
		{
			layer.UpdateSprite();
		}		
		Vec2 drawPos = Vec2.Subtract(_camera.GetAbsCenter(), _level.RelativeCenter);
		_renderer.DrawSprite(layer.Sprite, drawPos, _viewport);
	}

	protected Vec2 ElementPosInWorldSpace(LevelElement element)
	{
		if (_level == null) return new Vec2(0, 0);
		return Vec2.Subtract(element.Pos, _level.RelativeCenter);
	}
}