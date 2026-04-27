namespace MojiGotchi;

/// <summary>
/// Defines the type of collectible and its primary effect
/// </summary>
public enum CollectibleType
{
	FOOD,        // Increases saturation
	TOY,         // Increases mood and energy
	FRUIT,       // Balanced stats increase
	TREAT,       // Significant single stat boost
	MYSTERY      // Random stat effects
}

/// <summary>
/// Represents a collectible item that can be placed in the level and collected by the pet
/// Derives from Entity to inherit animation and positioning capabilities
/// </summary>
public class Collectible : Entity
{
	private CollectibleType _type;
	public CollectibleType Type
	{
		get => _type;
		set => _type = value;
	}

	/// <summary>
	/// Defines which stats this collectible affects and by how much
	/// </summary>
	private Dictionary<StatType, int> _statModifiers;
	public Dictionary<StatType, int> StatModifiers
	{
		get => _statModifiers;
		set => _statModifiers = value;
	}

	/// <summary>
	/// Sound effect to play when collected
	/// </summary>
	private string _collectSoundEffect;
	public string CollectSoundEffect
	{
		get => _collectSoundEffect;
		set => _collectSoundEffect = value;
	}

	/// <summary>
	/// Message displayed when pet collects this item
	/// </summary>
	private string _collectMessage;
	public string CollectMessage
	{
		get => _collectMessage;
		set => _collectMessage = value;
	}

	/// <summary>
	/// Visual indicator color for the collectible
	/// </summary>
	private Color _displayColor;
	public Color DisplayColor
	{
		get => _displayColor;
		set => _displayColor = value;
	}

	/// <summary>
	/// Whether this collectible has been collected
	/// </summary>
	private bool _isCollected;
	public bool IsCollected
	{
		get => _isCollected;
		set => _isCollected = value;
	}

	/// <summary>
	/// Time when this collectible was spawned (for despawn logic)
	/// </summary>
	private DateTime _spawnTime;
	public DateTime SpawnTime
	{
		get => _spawnTime;
		set => _spawnTime = value;
	}

	/// <summary>
	/// Lifetime in milliseconds before auto-despawn (-1 = infinite)
	/// </summary>
	private int _lifetimeMs;
	public int LifetimeMs
	{
		get => _lifetimeMs;
		set => _lifetimeMs = value;
	}

	/// <summary>
	/// Whether this collectible can be walked through or blocks movement
	/// </summary>
	private bool _isBlocking;
	public bool IsBlocking
	{
		get => _isBlocking;
		set => _isBlocking = value;
	}

	/// <summary>
	/// Points awarded to high score when collected
	/// </summary>
	private int _pointValue;
	public int PointValue
	{
		get => _pointValue;
		set => _pointValue = value;
	}

	// Constructor
	public Collectible() : base()
	{
		_type = CollectibleType.FOOD;
		_statModifiers = new Dictionary<StatType, int>();
		_collectSoundEffect = "action.wav";
		_collectMessage = "Yum!";
		_displayColor = Color.Yellow;
		_isCollected = false;
		_spawnTime = DateTime.Now;
		_lifetimeMs = -1; // Infinite lifetime by default
		_isBlocking = false;
		_pointValue = 10;
	}

	/// <summary>
	/// Creates a Collectible with specified type and stat effects
	/// </summary>
	public Collectible(CollectibleType type, string name, Dictionary<StatType, int> statMods, 
	                   string collectMsg, Color color, int points = 10, int lifetimeMs = -1) : base()
	{
		_type = type;
		Name = name;
		_statModifiers = statMods ?? new Dictionary<StatType, int>();
		_collectMessage = collectMsg;
		_displayColor = color;
		_pointValue = points;
		_lifetimeMs = lifetimeMs;
		_isCollected = false;
		_spawnTime = DateTime.Now;
		_isBlocking = false;
		_collectSoundEffect = "action.wav";
	}

	/// <summary>
	/// Checks if this collectible has expired based on lifetime
	/// </summary>
	public bool HasExpired()
	{
		if (_lifetimeMs < 0) return false; // Negative lifetime = never expires

		TimeSpan elapsed = DateTime.Now - _spawnTime;
		return elapsed.TotalMilliseconds >= _lifetimeMs;
	}

	/// <summary>
	/// Marks this collectible as collected and triggers collection effects
	/// </summary>
	public void Collect()
	{
		_isCollected = true;
		if (!string.IsNullOrEmpty(_collectSoundEffect))
		{
			SoundManager.PlayFileDirect(_collectSoundEffect);
		}
	}

	/// <summary>
	/// Returns the collection message for display
	/// </summary>
	public string GetCollectionMessage()
	{
		return _collectMessage ?? "Collected!";
	}

	/// <summary>
	/// Factory method to create a food collectible
	/// </summary>
	public static Collectible CreateFood(string name = "Food", int saturationBoost = 5)
	{
		var statMods = new Dictionary<StatType, int> { { StatType.SATURATION, saturationBoost } };
		return new Collectible(
			CollectibleType.FOOD,
			name,
			statMods,
			"Omnomnom!",
			Color.LightYellow,
			points: 15,
			lifetimeMs: 30000 // 30 seconds
		);
	}

	/// <summary>
	/// Factory method to create a toy collectible
	/// </summary>
	public static Collectible CreateToy(string name = "Toy", int moodBoost = 3, int energyBoost = 2)
	{
		var statMods = new Dictionary<StatType, int>
		{
			{ StatType.MOOD, moodBoost },
			{ StatType.ENERGY, energyBoost }
		};
		return new Collectible(
			CollectibleType.TOY,
			name,
			statMods,
			"Wheeeee!",
			Color.LightCyan,
			points: 20,
			lifetimeMs: 45000 // 45 seconds
		);
	}

	/// <summary>
	/// Factory method to create a fruit collectible
	/// </summary>
	public static Collectible CreateFruit(string name = "Fruit", int saturationBoost = 3, 
	                                       int moodBoost = 2, int energyBoost = 1)
	{
		var statMods = new Dictionary<StatType, int>
		{
			{ StatType.SATURATION, saturationBoost },
			{ StatType.MOOD, moodBoost },
			{ StatType.ENERGY, energyBoost }
		};
		return new Collectible(
			CollectibleType.FRUIT,
			name,
			statMods,
			"Delicious!",
			Color.LightRed,
			points: 25,
			lifetimeMs: 40000 // 40 seconds
		);
	}

	/// <summary>
	/// Factory method to create a special treat collectible (rare, high value)
	/// </summary>
	public static Collectible CreateTreat(string name = "Treat", int primaryBoost = 8)
	{
		var statMods = new Dictionary<StatType, int> { { StatType.MOOD, primaryBoost } };
		return new Collectible(
			CollectibleType.TREAT,
			name,
			statMods,
			"Amazing!",
			Color.Magenta,
			points: 50,
			lifetimeMs: 20000 // 20 seconds - rarer, disappears faster
		);
	}

	/// <summary>
	/// Factory method to create a mystery collectible with random effects
	/// </summary>
	public static Collectible CreateMystery(string name = "Mystery Box")
	{
		var random = new Random();
		var possibleStatMods = new List<Dictionary<StatType, int>>
		{
			new() { { StatType.SATURATION, random.Next(3, 6) } },
			new() { { StatType.MOOD, random.Next(2, 5) } },
			new() { { StatType.ENERGY, random.Next(2, 5) } },
			new() 
			{ 
				{ StatType.SATURATION, 2 },
				{ StatType.MOOD, 2 },
				{ StatType.ENERGY, 2 }
			}
		};

		var statMods = possibleStatMods[random.Next(possibleStatMods.Count)];
		return new Collectible(
			CollectibleType.MYSTERY,
			name,
			statMods,
			"Surprise!",
			Color.Yellow,
			points: 30,
			lifetimeMs: 25000 // 25 seconds
		);
	}

	/// <summary>
	/// Gets remaining lifetime in milliseconds (returns -1 if infinite)
	/// </summary>
	public int GetRemainingLifetime()
	{
		if (_lifetimeMs < 0) return -1;
		
		TimeSpan elapsed = DateTime.Now - _spawnTime;
		int remaining = _lifetimeMs - (int)elapsed.TotalMilliseconds;
		return Math.Max(0, remaining);
	}

	/// <summary>
	/// Returns a percentage of remaining lifetime (0-100, useful for visual feedback)
	/// </summary>
	public int GetLifetimePercentage()
	{
		if (_lifetimeMs < 0) return 100; // Infinite = always full
		
		int remaining = GetRemainingLifetime();
		return (int)((remaining / (float)_lifetimeMs) * 100);
	}
}
