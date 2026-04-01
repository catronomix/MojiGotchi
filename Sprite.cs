namespace MojiGotchi;

internal class Sprite
{
	private Vec2 _size;
	internal Vec2 Size
	{
		get
		{
			return _size; 
		}
	}

	private ScreenCell[,] _data = new ScreenCell[0,0];
	internal ScreenCell[,] Data
	{
		get
		{
			return _data;
		}
		set
		{
			_data = value;
		}
	}

	internal Sprite(Vec2 size)
	{
		_size = size;
		Data = new ScreenCell[size.Y, size.X];
	}

	internal bool WriteCell(Vec2 pos, ScreenCell cell)
	{
		if (pos.X >= 0 && pos.X < _size.X && pos.Y >= 0 && pos.Y < _size.Y)
		{
			Data[pos.Y, pos.X] = cell;
			return true;
		}
		else
		{
			return false;
		}
	}
	
}

internal class Animation
{
	private List<Sprite>? _frames = null;
	internal List<Sprite>? Frames
	{
		get
		{
			return _frames;
		}
	}
	private int _currentFrame;
	private int _frameDurationMs;
	private DateTime _lastFrameTime;

	//constructor
	internal Animation(int framedurationms)
	{
		_frameDurationMs = framedurationms;
		_currentFrame = 0;
		_lastFrameTime = DateTime.Now;
	}

	internal void OffsetTime(float offset)
	{
		_lastFrameTime -= TimeSpan.FromMilliseconds(offset);
	}

	internal void addFrame(Sprite frame)
	{
		//check if this is the first time a frame is added
		if (_frames == null)
		{
			_frames = [frame];
		}
		else if(Vec2.Equals(frame.Size, _frames[0].Size)) //check if the sizes match
		{
			_frames.Add(frame);
		}
	}

	internal Sprite? GetSprite(int offset = 0) // Changed return type to nullable Sprite?
	{
		// If _frames is null or empty, there are no frames to return.
		if (_frames == null || _frames.Count == 0)
		{
			return null;
		}
		//check if we need to advance the animation
	TimeSpan frameDuration = TimeSpan.FromMilliseconds(_frameDurationMs);
	if (DateTime.Now - _lastFrameTime > frameDuration)
		{
			_currentFrame++;
			if (_currentFrame >= _frames.Count)
			{
				_currentFrame = 0;
			}
			_lastFrameTime += frameDuration;
		}
		int offsetcurrent = (_currentFrame + offset) % _frames.Count;
		return _frames[offsetcurrent];
	}

	internal void AdvanceFrames(int numframes)
	{
		if(_frames != null)
		{
			_currentFrame = (_currentFrame + numframes) % _frames.Count;
		}
	}

	internal Animation Clone()
    {
        return (Animation)this.MemberwiseClone();
    }
}



	
