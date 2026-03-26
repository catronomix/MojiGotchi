namespace MojiGotchi;
public enum StatType
{
	SATURATION,
	ENERGY,
	MOOD,
	SLEEPYNESS
}

public class Pet : Entity //Pet inherits from Entity class
{
	// Animation state constants
	public const string AnimEating = "EATING";
	public const string AnimPlaying = "PLAYING";
	public const string AnimSleeping = "SLEEPING";
	public const string AnimWakeup = "WAKEUP";
	public const string AnimHappy = "HAPPY";

	private readonly string[] _namelist = new string[]
	{
		"Adelbert",
		"Bartholomeus",
		"Cunegonde",
		"Diederik",
		"Ermintrude",
		"Filibert",
		"Godefried",
		"Hildegard",
		"Isidoor",
		"Jozefien",
		"Kunibert",
		"Lutgardis",
		"Methusalem",
		"Nicodemus",
		"Othelia",
		"Pancratius",
		"Quintinus",
		"Radboud",
		"Scholastica",
		"Theofiel",
		"Ursmarus",
		"Veneranda",
		"Walburga",
		"Xaverius",
		"Yolente",
		"Zeger",
		"Aethelred",
		"Boudewijn",
		"Clothilde",
		"Dagobert",
		"Eustachius",
		"Fredegonde",
		"Gisela",
		"Hlodowig",
		"Irmgard",
		"Lotharius",
		"Sophistica",
		"Balthazar",
		"Filemon",
		"Amandus",
		"Bonifacius",
		"Cornelius",
		"Desiderius",
		"Evarist",
		"Gommarus",
		"Hyacinthus",
		"Idesbald",
		"Leopoldine",
		"Modestus",
		"Philemon",
		"Rosalia",
		"Seraphinus",
		"Trudo",
		"Urbanus",
		"Vitalis",
		"Wivina",
		"Zenon",
		"Amelberga",
		"Bavo"
	};
	
	private Stat _saturation;
	public Stat Saturation
	{
		get
		{
			return _saturation;
		}
		set
		{
			_saturation = value;
		}
	}
	private Stat _energy;
	public Stat Energy
	{
		get
		{
			return _energy;
		}
		set
		{
			_energy = value;
		}
	}
	private Stat _mood;
	public Stat Mood
	{
		get
		{
			return _mood;
		}
		set
		{
			_mood = value;
		}
	}
	private Stat _sleepyness;
	public Stat Sleepyness
	{
		get
		{
			return _sleepyness;
		}
		set
		{
			_sleepyness = value;
		}
	}

	//alive or not?
	private bool _isAlive;
	public bool IsAlive
	{
		get
		{
			return _isAlive;
		}
		set
		{
			_isAlive = value;
		}
	}
	//sleep management
	private bool _isSleeping;
	public bool IsSleeping
	{
		get
		{
			return _isSleeping;
		}
		set
		{
			_isSleeping = value;
		}
	}


	//timestamps for actions
	private DateTime _lastFed;
	public DateTime LastFed { get => _lastFed; set => _lastFed = value; }
	private DateTime _lastPlayed;
	public DateTime LastPlayed { get => _lastPlayed; set => _lastPlayed = value; }
	private DateTime _lastPetted;
	public DateTime LastPetted { get => _lastPetted; set => _lastPetted = value; }
	private DateTime _lastWaked;
	public DateTime LastWaked { get => _lastWaked; set => _lastWaked = value; }

	//birth time
	private DateTime _birthTime;
	public DateTime BirthTime { get => _birthTime; set => _birthTime = value; }

	//save time
	private DateTime _saveTime;
	public DateTime SaveTime { get => _saveTime; set => _saveTime = value; }

	//colors
	private Color _faceColor;
	public Color FaceColor { get => _faceColor; set => _faceColor = value; }
	private Color _bodyColor;
	public Color BodyColor { get => _bodyColor; set => _bodyColor = value; }

	//pet can wander
	private Random _wanderRandom;
	private int _wanderDirection;
	private const int _wanderIntervalMs = 50;
	private const int _animationTimeout = 2000;
	private DateTime _lastWander = DateTime.MinValue;

	//constant arrays to choose random colors
	private static readonly Color[] _availableFaceColor = new Color[]
	{
		Color.Yellow, Color.Cyan, Color.Magenta, Color.Green, Color.Red,
		new Color(255, 200, 150), // Peach
		new Color(200, 255, 100)  // Lime
	};

	private static readonly Color[] _availableBodyColor = new Color[]
	{
		Color.DarkBlue, Color.DarkCyan, Color.DarkGreen, Color.DarkMagenta, Color.DarkRed, Color.DarkYellow,
		new Color(60, 60, 60),    // Charcoal
		new Color(100, 50, 0)     // Brown
	};

	//constructor
	public Pet() : base()
	{
		//go wander
		_wanderRandom = new Random();
		_wanderDirection = _wanderRandom.Next(0, 8);
		
		_name = GenerateRandomName();
		//set birth time
		_birthTime = DateTime.Now;
		//set stats
		_saturation = new Stat(StatType.SATURATION, 60, 5);
		_energy = new Stat(StatType.ENERGY, 60, 5);
		_mood = new Stat(StatType.MOOD, 60, 5);
		_sleepyness = new Stat(StatType.SLEEPYNESS, 25, 0);

		//set timestamps
		_lastFed = DateTime.MinValue;
		_lastPetted = DateTime.MinValue;
		_lastPlayed = DateTime.MinValue;
		_lastWaked = DateTime.MinValue;

		//setup animations list
		_animations = JsonParser.LoadPetAnimations("PetSprites.json", 500);
		SetAnimation(AnimDefault);

		//position in the level, relative to game area center
		_position = new Vec2(0,0);

		//get born :)
		_isAlive = true;

		//wake up
		_isSleeping = false;

	}

	private string GenerateRandomName()
	{
		//generate random name from names list
		Random random = new Random();
		int index = random.Next(_namelist.Length);
		return _namelist[index];
	}

	public void RandomizePetColor()
	{
		Random random = new Random();
		_faceColor = _availableFaceColor[random.Next(_availableFaceColor.Length)];
		_bodyColor = _availableBodyColor[random.Next(_availableBodyColor.Length)];
		ApplyColorToAnimations();
	}

	public void ApplyColorToAnimations()
	{
		if (_animations == null)
		{
			return;
		}
		foreach (var animation in _animations.Values)
		{
			if (animation.Frames == null) continue;
			foreach (var frame in animation.Frames)
			{
				for (int y = 0; y < frame.Size.Y; y++)
				{
					for (int x = 0; x < frame.Size.X; x++)
					{
						// Detect "marker" colors from the base spritesheet and swap them
						if (Color.Equals(frame.Data[y, x].BgColor, Color.Black))
						{
							frame.Data[y, x].BgColor = _faceColor;
						}
						if (Color.Equals(frame.Data[y, x].BgColor, Color.DarkGray))
						{
							frame.Data[y, x].BgColor = _bodyColor;
						}
					}
				}
			}
		}
	}

	// Action methods for the pet
	public string Feed()
	{
		// Example: Feeding reduces hunger and might slightly increase mood
		_saturation.Raise(); // Or set to a specific value, e.g., _hunger.Reset();
		// _mood.Raise(); //trying to balance the game
		_energy.Raise();
		_lastFed = DateTime.Now;
		_animationState = AnimEating;
		SoundManager.Play("eat.wav");
		return "Je hebt " + _name + " eten gegeven.";
	}

	public string Play()
	{
		_energy.Lower(); // Playing uses energy
		_mood.Raise();   // Playing increases mood
		_lastPlayed = DateTime.Now;
		_animationState = AnimPlaying;
		SoundManager.Play("play.wav");
		return "Je hebt met " + _name + " gespeeld.";
	}

	public string PetPet() // Renamed to avoid conflict with class name 'Pet'
	{
		_mood.Raise(); // Petting increases mood
		_sleepyness.Raise(); // Playing increases sleeping
		_lastPetted = DateTime.Now;
		//not implemented yet
		// _animationState = AnimationState.HAPPY;
		SoundManager.Play("petpet.wav");
		return "Je hebt " + _name + " geaaid. Lief!";
	}

	public string WakeUp() // Renamed for clarity
	{
		_sleepyness.Lower(3); // Waking up reduces sleeping stat
		_isSleeping = false;
		_mood.Lower(3); // Waking up reduces mood
		_lastWaked = DateTime.Now;
		_animationState = AnimWakeup;
		SoundManager.Play("wakeup.wav");
		return "Je hebt " + _name + " wakker gemaakt.";
	}

	public string UpdateAllStats()
	{
		TimeSpan age = DateTime.Now - _birthTime;
		string ageString = DataManager.GetAgeString(age); // Use the new helper method
		//for stat expirations
		
		if (_sleepyness.LastUpdated + TimeSpan.FromSeconds(_sleepyness.UpdateInterval) < DateTime.Now)
		{
			if (_isSleeping)
			{
				_sleepyness.Lower(2);
				//sleep happenings
				_mood.Raise();
				_energy.Raise();			}
			else
			{
				_sleepyness.Raise();
				//wake happenings
				if (_saturation.LastUpdated + TimeSpan.FromSeconds(_saturation.UpdateInterval) < DateTime.Now)
				{
					_saturation.Lower();
				}
				if (_energy.LastUpdated + TimeSpan.FromSeconds(_energy.UpdateInterval) < DateTime.Now)
				{
					_energy.Lower();
				}
				if (_lastPlayed + TimeSpan.FromSeconds(_mood.UpdateInterval) < DateTime.Now)
				{
					_mood.Lower();
					_lastPlayed = DateTime.Now;
				}
			}
		}

		//check for sleep or wake
		if (_sleepyness.Value == _sleepyness.Max)
		{
			_isSleeping = true;
		}
		if (_sleepyness.Value == _sleepyness.Min)
		{
			_isSleeping = false;
		}

		//check for death conditions
		if (_mood.Value <= _mood.Min)
		{
			//pet has died from depression
			_isAlive = false;
			return _name + " is gestorven van depressie op de leeftijd van " + ageString;
		}
		if (_energy.Value <= _energy.Min)
		{
			//pet has died from exhaustion
			_isAlive = false;
			return _name + " is gestorven van uitputting op de leeftijd van " + ageString;
		}
		if (_saturation.Value <= _saturation.Min)
		{
			//pet has died from hunger
			_isAlive = false;
			return _name + " is gestorven van de honger op de leeftijd van " + ageString;
		}
		if (_saturation.Value >= _saturation.Max)
		{
			//pet has died from overfeeding
			_isAlive = false;
			return _name + " is gestorven door zich te overeten op de leeftijd van " + ageString;
		}

		return "";
	}

	public void Wander()
	{
		if (!IsSleeping&&  DateTime.Now > _lastFed + TimeSpan.FromMilliseconds(_animationTimeout) 
		&& DateTime.Now > _lastFed + TimeSpan.FromMilliseconds(_animationTimeout)
		&& DateTime.Now > _lastPetted + TimeSpan.FromMilliseconds(_animationTimeout)
		&& DateTime.Now > _lastWaked + TimeSpan.FromMilliseconds(_animationTimeout))
		{
			//50% chance to change direction
			if (_wanderRandom.Next(0, 100) < 50)
			{
				_wanderDirection = (_wanderDirection + _wanderRandom.Next(-2, 3) + 8) % 8; //rotate between -90 and 90 degrees
			}

			//80% chance to move
			if (_wanderRandom.Next(0, 100) < 80)
			{
				Move(_wanderDirection);
			}
		}
	}
}