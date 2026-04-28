namespace MojiGotchi;

class Game
{
	protected const int FPS = 30;
	protected const int sleepDelta = 1000 / FPS;
	protected const int statUpdateIntervalMs = 250;

	protected static Color programBgColor = Color.Black;
	protected Renderer _renderer; // Changed to instance field
	protected LoopResult _loopresult = LoopResult.CONTINUE;

	// Declare Rect objects as instance fields to cache them
	protected Rect _menuBgRect;
	protected Rect _statusBgRect;
	protected Rect _viewport;

	// Set area sizes
	protected int _menuWidth = 21;
	protected int _statusHeight = 7;

	//have a menu
	protected Menu _menu;

	//have a pet
	protected Pet? _pet;

	//have a level
	protected Level _level;

	//can have a race
	protected Race? _race;

	//have a camera
	protected Camera _camera;

	//have a status
	protected string _persistentStatus;
	protected string _transientStatus = "";
	protected DateTime _transientStatusExpires = DateTime.MinValue;
	protected DateTime _lastStatUpdate = DateTime.MinValue;

	//have sounds
	protected Sounds _sounds;

	//modals
	protected Modal? _currentModal;
	protected Help _help;
	protected HighScores _highScores;
	protected LanguageChoice _languageChoice;

	// Initialize the game
	public Game(bool devmode)
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
		_highScores = new HighScores(this);
		_languageChoice = new LanguageChoice();
		_currentModal = null;

		//gameplay
		_menu.AddItem(LM.Get("menu_feed"), SetAction(ActionType.FEED), Color.WoodDark, false);
		_menu.AddItem(LM.Get("menu_play"), SetAction(ActionType.PLAY), Color.WoodDark, false);
		_menu.AddItem(LM.Get("menu_pet"), SetAction(ActionType.PET), Color.WoodDark, false);
		_menu.AddItem(LM.Get("menu_wake"), SetAction(ActionType.WAKE), Color.WoodDark, false);

		//management
		_menu.AddItem(LM.Get("menu_newgame"), SetAction(ActionType.NEWPET), Color.DarkGreen);
		_menu.AddItem(LM.Get("menu_topscore"), SetAction(ActionType.TOPSCORE), Color.DarkGreen);
		_menu.AddItem(LM.Get("menu_help"), SetAction(ActionType.HELP), Color.DarkGreen);
		_menu.AddItem(LM.Get("menu_editor"), SetAction(ActionType.EDITOR_START), Color.DarkYellow);
		_menu.AddItem(LM.Get("menu_quit"), SetAction(ActionType.QUIT), Color.DarkGreen);

		_menu.SelectFirstEnabled();
		//load saved pet from file
		_pet = DataManager.LoadPet();
		BlueprintManager.Initialize(); // Initialize blueprints before loading a level
		_level = new Level(); // This is now empty, we need to load a level
		_level.LoadFromFile("level1.json"); // Load the level from the text file
		SimpleRect deadzone = new SimpleRect(new Vec2(-4, -2), new Vec2(8, 4));
		if (_pet != null)
		{
			UpdateMenuAvailability([ActionType.NEWPET], false);
			_menu.SelectFirstEnabled();
		}
		
		_camera = new Camera(_level, _pet, deadzone, _viewport); 

		_persistentStatus = LM.Get("status_welcome"); // Welcome

		//call CheckWindow once forced to ensure the window and buffer size are synced.
		CheckWindow(true);

		//apply padding to the level to keep the pet contained and for visual style
		_level.PadLevel(_viewport.Size.X / 2, _viewport.Size.Y / 2);

		//load sounds
		_sounds = new();
		_sounds.AddSound("death", "death.wav");
		_sounds.AddSound("newgame", "newgame.wav");
	}

	public Level GetLevel() { return _level; }

	public void ChooseLanguage()
	{
		_languageChoice.UpdatePage(_viewport.Size);
		_currentModal = _languageChoice;
		_menu.Disable();
	}

	public LoopResult Step()
	{
		//Check window resized
		CheckWindow();
		//Game logic
		CheckStats();

		//Race minigame
		CheckRace();

		PetMove();

		/*--------------------DRAWING--------------------*/
		// don't clear renderer when not needed
		// _renderer.ClearBuffer();
		//draw area Rects to renderer
		DrawRects();
		DrawMenuItems();
		DrawModalOptions();

		//update camera before drawing game
		_camera.UpdateCamera();

		DrawLevelLayer(_level.Layers[0], true);
		DrawPet();
		DrawLevelLayer(_level.Layers[1], true);
		DrawRace();
		DrawLevelLayer(_level.Layers[2], true);
		DrawPetBubble();
		DrawStatus();
		_renderer.RenderScreen();

		/*--------------------INPUT--------------------*/
		HandleInput();
		Thread.Sleep(sleepDelta);
		return _loopresult;
	}

	// Check if window has been resized
	protected bool CheckWindow(bool force = false)
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
			ConsoleHelper.HideCursor();
		}

		return resized;
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
		if (Console.KeyAvailable)
		{
			ConsoleKeyInfo key = Console.ReadKey(true);
			//only process last key to prevent buffer from filling
			while(Console.KeyAvailable){
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
			else if(_currentModal != null) // a modal is active
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
			else if(_race != null && _pet != null) //race is happening
			{
				Vec2 lastpetpos = _pet.Pos;
				switch (key.Key)
				{
					case ConsoleKey.UpArrow:
						_pet.Move(new Vec2(0,-1));
						break;
					case ConsoleKey.DownArrow:
						_pet.Move(new Vec2(0,1));
						break;
					case ConsoleKey.LeftArrow:
						_pet.Move(new Vec2(-1,0));
						break;
					case ConsoleKey.RightArrow:
						_pet.Move(new Vec2(1,0));
						break;
					default:
						break;
				}
				KeepInLevel(_pet, lastpetpos);
			}
		}
	}

	protected void DrawRects()
	{
		// make sprites
		Sprite menuBgSprite = _menuBgRect.GetSprite();
		Sprite statusBgSprite = _statusBgRect.GetSprite();
		Sprite viewportSprite = _viewport.GetSprite();
		
		_renderer.DrawSprite(menuBgSprite, _menuBgRect.Pos);
		_renderer.DrawSprite(statusBgSprite, _statusBgRect.Pos);
		_renderer.DrawSprite(viewportSprite, _viewport.Pos);

		//check if modal is active
		if (_currentModal != null)
		{
			_renderer.DrawSprite(_currentModal.GetSpriteBg(), _viewport.Pos);
			_renderer.DrawSprite(_currentModal.GetSpriteContent(), _viewport.Pos);
		}
	}

	protected void DrawModalOptions()
	{
		if (_currentModal != null)
		{
			//draw modal buttons
			int totaloptions = _currentModal.Options.Count;
			if (totaloptions > 0)
			{
				int totalWidth = 0;
				int gap = 4;

				// Calculate total width of the set
				foreach (MenuItem o in _currentModal.Options)
				{
					totalWidth += o.Sprite.Size.X + gap;
				}
				if (totalWidth > 0) totalWidth -= gap;

				// Calculate the x start
				int startX = (_viewport.Size.X - totalWidth) / 2;
				int currentXOffset = 0;

				// Draw buttons
				for (int i = 0; i < totaloptions; i++)
				{
					MenuItem option = _currentModal.Options[i];
					Sprite optionSprite = option.Sprite;

					//add markers around active option
					if (i == _currentModal.SelectedIndex)
					{
						optionSprite.Data[1, 1].Character = '>';
						if (optionSprite.Size.X >= 2) optionSprite.Data[1, optionSprite.Size.X - 2].Character = '<';
					}
					else
					{
						optionSprite.Data[1, 1].Character = ' ';
						if (optionSprite.Size.X >= 2) optionSprite.Data[1, optionSprite.Size.X - 2].Character = ' ';
					}
					Vec2 buttonPos = new Vec2(_viewport.Left + startX + currentXOffset, _viewport.Bottom - 5);
					_renderer.DrawSprite(optionSprite, buttonPos);
					currentXOffset += optionSprite.Size.X + gap;
				}
			}
		}
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
				_renderer.DrawSprite(mySprite, new Vec2(_menuBgRect.Pos.X + 1, _menuBgRect.Pos.Y + index * 3 + 1));
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
				// 2. Calculate Top-Left based on Pet's World Position and its Pivot (center)
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
		int persistentX = _statusBgRect.AbsCenter.X - _persistentStatus.Length / 2;
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
		}
	}

	void CheckRace()
	{
		if (_race != null && _pet != null)
		{
			if (!_race.Tick(_pet.Pos))
			{
				//query race for updates
				if (_race.HasUpdate)
				{
					int numcollected = 0;
					foreach(RaceCollectible item in _race.Collectibles)
					{
						numcollected += item.Collected ? 1 : 0;
					}
					string updatestring = $"{LM.Get("pet_race_running_prefix")} {numcollected}/{_race.Collectibles.Count}";
					SetTransientStatus(updatestring, 9999);
					_race.HasUpdate = false;
				}

				//race is over
				if (_race.HasWon())
				{
					_pet.Play(2000);
					SetTransientStatus(LM.Get("pet_race_win"), 2000);
				}
				else
				{
					_pet.WakeUp(2000);
					SetTransientStatus(LM.Get("pet_race_lose"), 2000);
				}
				_race = null;
				_menu.Enable();
			}
		}
	}

	void PetMove()
	{
		if (_race == null){
			//update menu availability
			if (_pet != null && _pet.IsSleeping)
			{
				UpdateMenuAvailability([ActionType.WAKE], true);
				UpdateMenuAvailability([ActionType.FEED, ActionType.PLAY, ActionType.PET], false);
				_pet.SetAnimation(Pet.AnimSleeping);
			}
			else if (_pet != null)
			{
				UpdateMenuAvailability([ActionType.WAKE], false);
				UpdateMenuAvailability([ActionType.FEED, ActionType.PLAY, ActionType.PET], true);
				Vec2 lastpetpos = _pet.Pos;
				_pet.Wander();
				KeepInLevel(_pet, lastpetpos);
			}
		}
	}

	void KeepInLevel(Entity entity, Vec2 lastpos)
	{
		//keep entity inside level
		int x = Math.Clamp(entity.Pos.X, -_level.RelativeCenter.X, _level.RelativeCenter.X);
		int y = Math.Clamp(entity.Pos.Y, -_level.RelativeCenter.Y, _level.RelativeCenter.Y);
		entity.Pos = new Vec2(x, y);

		//keep pet from moving into solid objects
		if (_pet != null && _level != null)
		{
			foreach(LevelElement? element in _level.Layers[1].Elements)
			{
				if (element == null) continue; //skip empty cells
				if (element.IsBlocking)
				{
					// check if colliding
					// The pet's collision point is its center cell, which is _pet.Pos + (1,1)
					if(Vec2.Chebyshev(ElementPosInWorldSpace(element), entity.Pos) < 2)
					{
						_pet.Pos = lastpos;
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
		SoundManager.PlaySound(_sounds.GetSound("death"));
		SetTransientStatus("Begin een nieuw spel om een nieuwe MojiGotchi te krijgen.");
	}

	//game actions
	internal GameAction SetAction(ActionType type)
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
					SoundManager.PlaySound(_sounds.GetSound("newgame"));
				};
				break;
			case ActionType.FEED:
				logic = (game) =>
				{
					if (game._pet != null)
					{
						game.SetTransientStatus(game._pet.Feed(2000), 2000);
					}
				};
				break;
			case ActionType.PLAY:
				logic = (game) =>
				{
					if (game._pet != null)
					{
						// game.SetTransientStatus(game._pet.Play(1500), 1500);
						//replace with race game
						game.StartRace(3, 60);
					}
				};
				break;
			case ActionType.PET:
				logic = (game) =>
				{
					if (game._pet != null)
					{
						game.SetTransientStatus(game._pet.PetPet(2000), 2000);
					}
				};
				break;
			case ActionType.WAKE:
				logic = (game) =>
				{
					if (game._pet != null)
					{
						game.SetTransientStatus(game._pet.WakeUp(3000),3000);
					}
				};
				break;
			case ActionType.QUIT:
				logic = (game) =>
				{
					if (game._pet != null) { DataManager.SavePet(game._pet); } else { DataManager.DeleteSave(); }
					game._loopresult = LoopResult.QUIT;
				};
				break;
			case ActionType.EDITOR_START:
				logic = (game) =>
				{
					if (game._pet != null) { DataManager.SavePet(game._pet); } else { DataManager.DeleteSave(); }
					game._loopresult = LoopResult.GOTOEDITOR;
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
			case ActionType.HIGHSCORES_CLEAR:
				logic = (game) =>
				{
					DataManager.ClearHighScores();
					game._highScores.UpdatePage(_viewport.Size);
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

	protected void DrawLevelLayer(LevelLayer layer, bool update = true, float shade = 0.0f)
	{
		if (_level == null || layer == null || _currentModal != null) return;

		// Level.SetSprite fills the layerSprite (which is level-sized)
		if (update)
		{
			layer.UpdateSprite(shade);
		}		
		Vec2 drawPos = Vec2.Subtract(_camera.GetAbsCenter(), _level.RelativeCenter);
		_renderer.DrawSprite(layer.Sprite, drawPos, _viewport);
	}

	protected void DrawRace()
	{
		if(_race != null && _currentModal == null)
		{
			foreach(RaceCollectible rc in _race.Collectibles)
			{
				Vec2 drawPos = Vec2.Add(_camera.GetAbsCenter(), rc.Pos.Sum(-1, -1));
				_renderer.DrawSprite(rc.GetSprite(), drawPos, _viewport);
			}
		}
	}

	protected Vec2 ElementPosInWorldSpace(LevelElement element)
	{
		if (_level == null) return new Vec2(0, 0);
		return Vec2.Subtract(element.Pos, _level.RelativeCenter);
	}

	protected void StartRace(int count = 4, int duration = 20)
	{
		_race = new Race(_level, count, duration);
		_menu.Disable();
		SetTransientStatus($"{LM.Get("status_race_started_prefix")} {_pet?.Name} {LM.Get("status_race_started_suffix")}", 9999);
	}
}