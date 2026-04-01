namespace MojiGotchi;

class Camera
{
	private readonly SimpleRect _deadzone;
	private Entity? _target;
	private Level _level;
	private Vec2 _camPos;
	internal Vec2 CamPos
	{
		get
		{
			return _camPos;
		}
	}
	private SimpleRect _viewport;

	internal Camera(Level level, Entity? entity, SimpleRect deadzone, SimpleRect viewport)
	{
		_deadzone = deadzone; //in worldspace
		_camPos = new Vec2(0, 0); //in worldspace
		_target = entity;
		_level = level;
		_viewport = viewport;
	}

	internal void SetPet (Pet? pet)
	{
		_target = pet;
	}

	internal void UpdateCamera()
	{
		if (_target != null && _level != null)
		{
			Vec2 offset = new Vec2(0,0);
			//check if pet is outside of deadzone
			if (_target.Position.X < _deadzone.Left)
			{
				offset.X --;
			}
			else if (_target.Position.X > _deadzone.Right)
			{
				offset.X ++;
			}
			if (_target.Position.Y < _deadzone.Top)
			{
				offset.Y --;
			}
			else if (_target.Position.Y > _deadzone.Bottom)
			{
				offset.Y ++;
			}
			_camPos = Vec2.Add(_camPos, offset);
			_deadzone.Pos = Vec2.Subtract(_camPos, _deadzone.Size.Divide(2));
		}
	}
	
	internal Vec2 GetAbsCenter()
	{
		return Vec2.Subtract(_viewport.AbsCenter, _camPos);
	}
}
