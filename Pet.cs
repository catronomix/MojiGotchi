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
	
	public Stat Saturation { get; set; }
	public Stat Energy { get; set; }
	public Stat Mood { get; set; }
	public Stat Sleepyness { get; set; }

	//alive or not?
	public bool IsAlive { get; set; }
	//sleep management
	public bool IsSleeping { get; set; }

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
	private Random random;
	private int _wanderDirection;
	private const int _wanderIntervalMs = 50;
	private const int _animationTimeout = 2000;
	private DateTime _lastWander = DateTime.MinValue;

	//constant arrays to choose random colors
	private static readonly Color[] _availableFaceColor = new Color[]
	{
		Color.BabyBrown, Color.BabyPink, Color.BabyWhite, Color.LightBlue, Color.LightCyan,
		Color.LightGray, Color.LightGreen, Color.LightRed, Color.Yellow, Color.White
	};

	private static readonly Color[] _availableBodyColor = new Color[]
	{
		Color.DarkBlue, Color.DarkCyan, Color.DarkGreen, Color.DarkMagenta, Color.DarkRed, Color.DarkYellow,
		Color.DarkGray, Color.Magenta, Color.Red
	};

	//make it talk :)
	public MessageBubble MessageBubble;

	//constructor
	public Pet() : base()
	{
		//go wander
		random = new Random();
		_wanderDirection = random.Next(0, 8);
		
		_name = GenerateRandomName();
		//set birth time
		_birthTime = DateTime.Now;
		//set stats
		Saturation = new Stat(StatType.SATURATION, 60, 5);
		Energy = new Stat(StatType.ENERGY, 60, 5);
		Mood = new Stat(StatType.MOOD, 60, 5);
		Sleepyness = new Stat(StatType.SLEEPYNESS, 25, 0);

		//set timestamps
		_lastFed = DateTime.MinValue;
		_lastPetted = DateTime.MinValue;
		_lastPlayed = DateTime.MinValue;
		_lastWaked = DateTime.MinValue;

		//setup animations list
		_animations = JsonParser.LoadPetAnimations("PetSprites.json", 500);
		SetAnimation(AnimDefault);

		//position in the level, relative to world origin
		_position = new Vec2(0,0);

		//get born :)
		IsAlive = true;

		//wake up
		IsSleeping = false;

		//message bubble
		MessageBubble = new MessageBubble(Color.Black, Color.White);

	}

	private string GenerateRandomName()
	{
		//generate random name from names list
		int index = random.Next(_namelist.Length);
		return _namelist[index];
	}

	public void RandomizePetColor()
	{
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
		Saturation.Raise(); // Or set to a specific value, e.g., _hunger.Reset();
		// _mood.Raise(); //trying to balance the game
		Energy.Raise();
		_lastFed = DateTime.Now;
		_animationState = AnimEating;
		SoundManager.Play("eat.wav");
		MessageBubble.SetMessage("Omnomnom", 1000, true);
		return LM.Get("pet_action_feed", [_name]);
	}

	public string Play()
	{
		Energy.Lower(); // Playing uses energy
		Mood.Raise();   // Playing increases mood
		_lastPlayed = DateTime.Now;
		_animationState = AnimPlaying;
		SoundManager.Play("play.wav");
		MessageBubble.SetMessage("Wheeeee!", 1000, true);
		return LM.Get("pet_action_play", [_name]);
	}

	public string PetPet() // Renamed to avoid conflict with class name 'Pet'
	{
		Mood.Raise(); // Petting increases mood
		Sleepyness.Raise(); // Playing increases sleeping
		_lastPetted = DateTime.Now;
		//not implemented yet
		// _animationState = AnimationState.HAPPY;
		SoundManager.Play("petpet.wav");
		return LM.Get("pet_action_pet", [_name]);
	}

	public string WakeUp() // Renamed for clarity
	{
		Sleepyness.Lower(3); // Waking up reduces sleeping stat
		IsSleeping = false;
		Mood.Lower(3); // Waking up reduces mood
		_lastWaked = DateTime.Now;
		_animationState = AnimWakeup;
		MessageBubble.SetMessage("Ugh..", 1000, true);
		SoundManager.Play("wakeup.wav");
		return LM.Get("pet_action_wake", [_name]);
	}

	public string UpdateAllStats()
	{
		TimeSpan age = DateTime.Now - _birthTime;
		string ageString = DataManager.GetAgeString(age); // Use the new helper method
		//for stat expirations
		
		if (Sleepyness.LastUpdated + TimeSpan.FromSeconds(Sleepyness.UpdateInterval) < DateTime.Now)
		{
			if (IsSleeping)
			{
				Sleepyness.Lower(2);
				//sleep happenings
				Mood.Raise();
				Energy.Raise();
			}
			else
			{
				Sleepyness.Raise();
				//wake happenings
				if (Saturation.LastUpdated + TimeSpan.FromSeconds(Saturation.UpdateInterval) < DateTime.Now)
				{
					Saturation.Lower();
				}
				if (Energy.LastUpdated + TimeSpan.FromSeconds(Energy.UpdateInterval) < DateTime.Now)
				{
					Energy.Lower();
				}
				if (_lastPlayed + TimeSpan.FromSeconds(Mood.UpdateInterval) < DateTime.Now)
				{
					Mood.Lower();
					_lastPlayed = DateTime.Now;
				}
			}
		}

		//check for sleep or wake
		if (Sleepyness.Value == Sleepyness.Max)
		{
			IsSleeping = true;
		}
		if (Sleepyness.Value == Sleepyness.Min)
		{
			IsSleeping = false;
		}

		//check for death conditions
		if (Mood.Value <= Mood.Min)
		{
			//pet has died from depression
			IsAlive = false;
			return LM.Get("death_message_depression", [_name, ageString]);
		}
		if (Energy.Value <= Energy.Min)
		{
			//pet has died from exhaustion
			IsAlive = false;
			return LM.Get("death_message_exhaustion", [_name, ageString]);
		}
		if (Saturation.Value <= Saturation.Min)
		{
			//pet has died from hunger
			IsAlive = false;
			return LM.Get("death_message_hunger", [_name, ageString]);
		}
		if (Saturation.Value >= Saturation.Max)
		{
			//pet has died from overfeeding
			IsAlive = false;
			return LM.Get("death_message_overfeeding", [_name, ageString]);
		}

		return "";
	}

	public void Wander()
	{
		if (!IsSleeping&&  DateTime.Now > _lastFed + TimeSpan.FromMilliseconds(_animationTimeout)
		&& DateTime.Now > _lastPetted + TimeSpan.FromMilliseconds(_animationTimeout)
		&& DateTime.Now > _lastWaked + TimeSpan.FromMilliseconds(_animationTimeout))
		{
			//50% chance to change direction
			if (random.Next(0, 100) < 50)
			{
				_wanderDirection = (_wanderDirection + random.Next(-2, 3) + 8) % 8; //rotate between -90 and 90 degrees
			}

			//80% chance to move
			if (random.Next(0, 100) < 80)
			{
				Move(_wanderDirection);
			}
		}
	}

	public void Communicate()
	{
		//check for hunger
		if (Saturation.Value <= Saturation.Min + 1)
		{
			MessageBubble.SetMessage(LM.Get("pet_say_hungry"), Color.Red, Color.LightYellow);
		}

		//check for too full
		if (Saturation.Value >= Saturation.Max - 1)
		{
			MessageBubble.SetMessage(LM.Get("pet_say_toofull"), Color.Red, Color.LightYellow);
		}

		//check for bad mood
		if (Mood.Value <= Mood.Min + 2)
		{
			MessageBubble.SetMessage(LM.Get("pet_say_badmood"), Color.Red, Color.LightYellow);
		}
		if (Mood.Value <= Mood.Min + 1)
		{
			MessageBubble.SetMessage(LM.Get("pet_say_verybadmood"),Color.Red, Color.LightYellow);
		}

		//check for energy
		if (Energy.Value <= Energy.Min + 1)
		{
			MessageBubble.SetMessage(LM.Get("pet_say_exhausted"),Color.Red, Color.LightYellow);
		}
	}
}