using System.Security.Claims;
using Silk.NET.Core.Native;

namespace MojiGotchi;

class RaceCollectible : Collectible
{
	private bool _collected;
	public bool Collected { get => _collected; set => _collected = value; }

	//constructor
	public RaceCollectible() : base(CollectibleType.RACE)
	{
		SetAnimation(AnimRaceDefault);
		_collected = false;
	}

	public void Collect()
	{
		_collected = true;
		SetAnimation(AnimRaceCollected);
	}
}

class Race
{
	private List<RaceCollectible> _collectibles = new List<RaceCollectible>();
	public List<RaceCollectible> Collectibles
	{
		get => _collectibles;
	}
	private DateTime _startTime;
	private TimeSpan _raceDuration;
	private bool _win;
	private Level _level;
	
	public Race(Level level, int count = 4, int duration = 20)
	{
		_startTime = DateTime.Now;
		//convert duration from seconds
		_raceDuration = TimeSpan.FromSeconds(duration);
		_win = false;
		_level = level;

		for (int i = 0; i < count; i++)
		{
			_collectibles.Add(new RaceCollectible());
		}
		DistributeCollectibles(3, 4);
	}

	private void DistributeCollectibles(int space, int spread)
	{
		List<Collectible> added = new List<Collectible>();
		int maxtries = 16;

		// Iterate backwards to allow immediate removal from the list
		for (int i = _collectibles.Count - 1; i >= 0; i--)
		{
			Collectible item = _collectibles[i];
			int tries = 0;
			bool success = false;

			while (tries < maxtries)
			{
				Vec2 trypos = FindAvailablePosition(space);
				bool isOverlap = false;

				foreach (Collectible other in added)
				{
					if (Vec2.Chebyshev(trypos, other.Pos) < spread)
					{
						isOverlap = true;
						break;
					}
				}

				if (!isOverlap)
				{
					item.Pos = trypos;
					added.Add(item);
					success = true;
					break;
				}
				tries++;
			}

			if (!success)
			{
				// remove item from list
				_collectibles.RemoveAt(i);
			}
		}
		//offset the positions to compensate for world center
		foreach(Collectible collectible in _collectibles)
		{
			collectible.Pos = Vec2.Subtract(collectible.Pos, _level.RelativeCenter);
		}
	}

	public bool Tick(Vec2 pos)
	{
		//check for pickup
		foreach (RaceCollectible collectible in _collectibles)
		{
			if (collectible.Collected) continue;
			if (Vec2.Equals(collectible.Pos, pos))
			{
				collectible.Collected = true;
			}
		}
		//check if we have won
			foreach (RaceCollectible collectible in _collectibles)
		{
			if (!collectible.Collected)
			{
				break;
			}
			_win = true;
			return false;
		}
		//or lose if time expired
		if (DateTime.Now > _startTime + _raceDuration)
		{
			_win = false;
			return false;
		}
		return true;
	}

	public bool HasWon()
	{
		return _win;
	}
	
	private Vec2 FindAvailablePosition(int space = 5)
	{
		//check middle layer only
		LevelLayer layer = _level.Layers[1];

		//limit radius for safety:
		space = Math.Clamp(space, 1, 15);

		// Make sure not to go out of bounds of the layer's array
		int minSearchX = space;
		int maxSearchX = layer.Size.X - 1 - space;
		int minSearchY = space;
		int maxSearchY = layer.Size.Y - 1 - space;

		Random rand = Randomizer.R();
		int maxAttempts = 512; // Limit random attempts to prevent excessive computation on dense levels

		for (int i = 0; i < maxAttempts; i++)
		{
			int randomX = rand.Next(minSearchX, maxSearchX + 1); // +1 because Next is exclusive on upper bound
			int randomY = rand.Next(minSearchY, maxSearchY + 1);
			Vec2 potentialPos = new Vec2(randomX, randomY);

			if (IsSpaceOpen(potentialPos, space))
			{
				return potentialPos;
			}
		}

		// If no available position is found after all attempts, log a warning and return a default.
		DebugLogger.Log($"WARNING: No available position found for RaceCollectible with radius {space} after all attempts. Returning (0,0).");
		return new Vec2(0, 0); // Default fallback position
	}

	private bool IsSpaceOpen(Vec2 center, int radius)
	{
		LevelLayer layer = _level.Layers[1]; // Middle layer for checking

		int minX = center.X - radius;
		int maxX = center.X + radius;
		int minY = center.Y - radius;
		int maxY = center.Y + radius;

		for (int y = minY; y <= maxY; y++)
		{
			for (int x = minX; x <= maxX; x++)
			{
				// Check the element at (x, y) to see if it is within a circular radius and occupied:
				LevelElement? element = layer.Elements[x, y];
				if (element != null && element.Key != ".." && Vec2.Chebyshev(center, new Vec2(x, y)) <= radius)
				{
					return false; // space is occupied
				}
			}
		}
		return true; // All cells in the radius are empty or null
	}
}