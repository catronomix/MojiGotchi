namespace MojiGotchi;

public class Entity
{
    protected string _name;
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}
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
	protected Vec2 _position;
    public Vec2 Position //relative to the center of the game area
	{
		get
		{
			return _position;
		}
		set
		{
			//check for out of bounds
			// _position.X = Math.Clamp(value.X, 0, Console.WindowWidth);
			// _position.Y = Math.Clamp(value.Y, 0, Console.WindowHeight);
			//entities can exist off-screen
			_position = value;
		}
	}
    protected string _animationState;

    //constructor
    public Entity()
    {
        _name = "";
		//setup animations list
		_animations = null;
		_animationState = AnimDefault;
		_animOffset = AnimRandom.GetRandom(0, 16);

		//position in the level
		_position = new Vec2(-100,-100); //invalid value to have new entities be hidden by default
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
        _position.X += amount.X;
        _position.Y += amount.Y;
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