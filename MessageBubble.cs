namespace MojiGotchi;

public class MessageBubble
{	
	public string Text { get; set; }
	private Color _textColor;
	private Color _bgColor;
	private DateTime _lastSet;
	private DateTime _expires;
	private bool _active;

	private Sprite? _mySprite;
	
	public MessageBubble(Color fgcolor, Color bgcolor)
	{
		Text = "";
		_textColor = fgcolor;
		_bgColor = bgcolor;
		_lastSet = DateTime.Now;
		_expires = DateTime.MinValue;
		_active = false;
		_mySprite = null;
	}

	public void SetMessage(string message, int expireMs = 2000, bool force = false)
	{
		SetMessage(message, _textColor, _bgColor, expireMs, force);
	}	

	public void SetMessage(string message, Color fgcolor, Color bgcolor,int expireMs = 2000, bool force= false)
	{
		if (Text != message || force) //only use force on single fires!
		{
			Text = message;
			_expires = DateTime.Now + TimeSpan.FromMilliseconds(expireMs);
			_lastSet = DateTime.Now;
			
			_mySprite = new Sprite(new Vec2(Text.Length, 1));
			//write text to sprite
			for (int i = 0; i < Text.Length; i++)
			{
				_mySprite.WriteCell(new Vec2(i, 0), new ScreenCell(Text[i], fgcolor, bgcolor));
			}
			_active = true;
		}
	}

	public Sprite? GetSprite()
	{
		if (_active){
			bool available = _expires > DateTime.Now;
			if (available)
			{
				return _mySprite;
			}
			else
			{
				_active = false;
			}
		}
		return null;
	}	
}