namespace MojiGotchi;

internal class Stat
{
	//type
	private StatType _type;
	internal StatType Type
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
	internal int Value
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
	internal int Min
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
	internal int Max
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
	internal int Default
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
	internal DateTime LastUpdated
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
	internal int UpdateInterval
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
	internal Stat(){}
	internal Stat(StatType type,int updateinterval = 10, int defaultValue = 5, int maxValue = 10, int minValue = 0)
	{
		_type = type;
		_value = defaultValue;
		_defaultValue = defaultValue;
		_minValue = minValue;
		_maxValue = maxValue;
		_lastUpdated = DateTime.Now;
		_updateInterval = updateinterval;
	}

	internal void Raise(int amount = 1)
	{
		if (_value < _maxValue)
		{
			_value = _value + amount;
			_lastUpdated = DateTime.Now;
		}
	}

	internal void Lower(int amount = 1)
	{
		if (_value > _minValue)
		{
			_value = _value - amount;
			_lastUpdated = DateTime.Now;
		}
	
	}
	internal void Reset()
	{
		_value = _defaultValue;
		_lastUpdated = DateTime.Now;
	}
}