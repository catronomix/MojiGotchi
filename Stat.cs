namespace MojiGotchi;

public class Stat
{
	//type
	private StatType _type;
	public StatType Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}
	//value
	private int _value;
	public int Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	private int _minValue;
	public int Min
	{
		get
		{
			return _minValue;
		}
		set
		{
			_minValue = value;
		}
	}

	private int _maxValue;
	public int Max
	{
		get
		{
			return _maxValue;
		}
		set
		{
			_maxValue = value;
		}
	}
	private int _defaultValue;
	public int Default
	{
		get
		{
			return _defaultValue;
		}
		set
		{
			_defaultValue = value;
		}
	}
	//last updated timestamp
	private DateTime _lastUpdated;
	public DateTime LastUpdated
	{
		get
		{
			return _lastUpdated;
		}
		set
		{
			_lastUpdated = value;
		}
	}

	private int _updateInterval; // in seconds
	public int UpdateInterval
	{
		get
		{
			return _updateInterval;
		}
		set
		{
			_updateInterval = value;
		}
	}

	//constructors
	public Stat(){}
	public Stat(StatType type,int updateinterval = 10, int defaultValue = 5, int maxValue = 10, int minValue = 0)
	{
		_type = type;
		_value = defaultValue;
		_defaultValue = defaultValue;
		_minValue = minValue;
		_maxValue = maxValue;
		_lastUpdated = DateTime.Now;
		_updateInterval = updateinterval;
	}

	public void Raise(int amount = 1)
	{
		if (_value < _maxValue)
		{
			_value = _value + amount;
			_lastUpdated = DateTime.Now;
		}
	}

	public void Lower(int amount = 1)
	{
		if (_value > _minValue)
		{
			_value = _value - amount;
			_lastUpdated = DateTime.Now;
		}
	
	}
	public void Reset()
	{
		_value = _defaultValue;
		_lastUpdated = DateTime.Now;
	}
}