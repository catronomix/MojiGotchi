namespace MojiGotchi;

public class Entity
{
	public string Name {get; set;}
	
	// Animation state constants
	public const string AnimDefault = "DEFAULT";
	private int _animOffset;

	//Animations list
	protected Dictionary<string, Animation>? _animations;
	[System.Text.Json.Serialization.JsonIgnore]
	public Dictionary<string, Animation>? Animations
	{
		get
		{
			return _animations;
		}
		set
		{
			_animations = value;
		}
	}
	protected Vec2 _pos;
	public Vec2 Pos //relative to the center of the game area
	{
		get
		{
			return _pos;
		}
		set
		{
			_pos = value;
		}
	}
	protected string _animationState;

	//constructor
	public Entity()
	{
		Name = "";
		//setup animations list
		_animations = null;
		_animationState = AnimDefault;
		_animOffset = AnimRandom.GetRandom(0, 16);

		//position in the level
		_pos = new Vec2(-100,-100); //invalid value to have new entities be hidden by default
	}

	public Sprite? GetSprite()
	{
		if (_animations != null && _animations.TryGetValue(_animationState, out var animation))
		{
			return animation.GetSprite(_animOffset);
		}
		return null;
	}
   
	public void Move(Vec2 amount)
	{
		_pos.X += amount.X;
		_pos.Y += amount.Y;
	}

	public void Move(int direction)
	{
		Vec2 amount = new Vec2(0,0);
		//move clockwise from 0 to 7
		switch (direction)
		{
			case 0: amount.X = 0; amount.Y = -1; break; //up
			case 1: amount.X = 1; amount.Y = -1; break; //upright
			case 2: amount.X = 1; amount.Y = 0; break; //right
			case 3: amount.X = 1; amount.Y = 1; break; //downright
			case 4: amount.X = 0; amount.Y = 1; break; //down
			case 5: amount.X = -1; amount.Y = 1; break; //
			case 6: amount.X = -1; amount.Y = 0; break; //
			case 7: amount.X = -1; amount.Y = -1; break; //
			default: break;
		}
		Move(amount);
	}

	public void SetAnimation(string state)
	{
		_animationState = state;
	}


}