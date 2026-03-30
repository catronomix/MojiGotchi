namespace MojiGotchi;

class Camera
{
	private readonly SimpleRect _deadzone;
	private Pet? _pet;
	private Level _level;
	private Vec2 _camPos;
	public Vec2 CamPos
	{
		get
		{
			return _camPos;
		}
	}
	private SimpleRect _viewport;

	public Camera(Level level, Pet? pet, SimpleRect deadzone, SimpleRect viewport)
	{
		_deadzone = deadzone; //in worldspace
		_camPos = new Vec2(0, 0); //in worldspace
		_pet = pet;
		_level = level;
		_viewport = viewport;
	}

	public void SetPet (Pet? pet)
	{
		_pet = pet;
	}

	public void UpdateCamera()
	{
		if (_pet != null && _level != null)
		{
			Vec2 offset = new Vec2(0,0);
			//check if pet is outside of deadzone
			if (_pet.Position.X < _deadzone.Left)
			{
				offset.X --;
			}
			else if (_pet.Position.X > _deadzone.Right)
			{
				offset.X ++;
			}
			if (_pet.Position.Y < _deadzone.Top)
			{
				offset.Y --;
			}
			else if (_pet.Position.Y > _deadzone.Bottom)
			{
				offset.Y ++;
			}
			_camPos = Vec2.Add(_camPos, offset);
			_deadzone.Pos = Vec2.Subtract(_camPos, _deadzone.Size.Divide(2));
		}
	}
	
	public Vec2 GetAbsCenter()
	{
		return Vec2.Subtract(_viewport.AbsCenter, _camPos);
	}
}
