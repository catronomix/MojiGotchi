namespace MojiGotchi;
using System.Text.Json.Serialization;

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
			//check for out of bounds
			// _pos.X = Math.Clamp(value.X, 0, Console.WindowWidth);
			// _pos.Y = Math.Clamp(value.Y, 0, Console.WindowHeight);
			//entities can exist off-screen
			_pos = value;
		}
	}
    protected string _animationState; // Keep the protected field as backing

	 // Apply JsonInclude to the public property
	public string AnimationState // New public property for serialization
	{
		get
		{
			return _animationState;
		}
		set
		{
			_animationState = value;
		}
	}

	//hitbox
	[JsonIgnore]
	public Vec2[,]? Hitbox { get; private set;}

    //constructor
    public Entity()
    {
        _name = "";
		//setup animations list
		_animations = null;
		_animationState = AnimDefault;
		_animOffset = AnimRandom.GetRandom(0, 16);

		//position in the level
		_pos = new Vec2(-9999,-9999); //invalid value to have new entities be hidden by default

		//default hitbox
		Hitbox = null;

    }

    public Sprite? GetSprite()
	{
		if (_animations != null && _animations.TryGetValue(_animationState, out var animation))
		{
			Sprite? sprite = animation.GetSprite(_animOffset);
			if (sprite != null)
			{
				Hitbox = new Vec2[sprite.Size.X, sprite.Size.Y];
				for (int y = 0; y < sprite.Size.Y; y++)
				{
					for (int x = 0; x < sprite.Size.X; x++)
					{
						Hitbox[x, y] = new Vec2(x, y);
					}
				}
			}
			else
			{
				Hitbox = null;
			}
			return sprite;
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