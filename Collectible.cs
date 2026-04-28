namespace MojiGotchi;

//this file just holds the abstract class to enable collectibles in the game
enum CollectibleType
{
	RACE,
	FOOD
}

public static class CollectAnims
{
	// Animation state constants
	public const string Default = "DEFAULT"; //should not be used in-game
	public const string AnimRaceDefault = "RACEDEFAULT";
	public const string AnimRaceCollected = "RACECOLLECTED";
	public const string AnimFoodApple = "FOODAPPLE";
	public const string AnimFoodBerry = "FOODBERRY";
}

abstract class Collectible: Entity
{
	private CollectibleType _type;

	//static animations dict for all collectibles
	protected static Dictionary<string, Animation>? _sharedAnimations = JsonParser.LoadAnimations("CollectibleSprites.json");

	//override base class's animations source to point to the static one
	protected override Dictionary<string, Animation>? _animationsSource
	{
		get => _sharedAnimations;
	}

	public CollectibleType Type
	{
		get
		{
			return _type;
		}
	}

	public Collectible(CollectibleType type): base()
	{
		_type = type;
		_animations = JsonParser.LoadAnimations("CollectibleSprites.json");
		SetAnimation(AnimDefault);
	}
}