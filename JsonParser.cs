namespace MojiGotchi;

using System;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;

static class JsonParser
{
	/// <summary>
	/// Helper to map color names from JSON strings to our custom Color struct presets.
	/// </summary>
	private static Color ParseColor(string colorName)
	{
		return colorName switch
		{
			"Black" => Color.Black,
			"DarkBlue" => Color.DarkBlue,
			"DarkGreen" => Color.DarkGreen,
			"DarkCyan" => Color.DarkCyan,
			"DarkRed" => Color.DarkRed,
			"DarkMagenta" => Color.DarkMagenta,
			"DarkYellow" => Color.DarkYellow,
			"Gray" => Color.Gray,
			"DarkGray" => Color.DarkGray,
			"Blue" => Color.Blue,
			"Green" => Color.Green,
			"Cyan" => Color.Cyan,
			"Red" => Color.Red,
			"Magenta" => Color.Magenta,
			"Yellow" => Color.Yellow,
			"White" => Color.White,
			"Orange" => Color.Orange,
			_ => Color.White // Fallback for unknown colors
		};
	}

	public static Dictionary<string, Animation>? LoadAnimations(string filepath, int frametime)
	{
		DebugLogger.Log($"LoadAnimations called with path: {filepath}");
		var loadedAnimations = new Dictionary<string, Animation>();

		if (!File.Exists(filepath))
		{
			DebugLogger.Log($"Sprites file not found: {filepath}");
			return loadedAnimations;
		}

		DebugLogger.Log($"Sprites file found: {filepath}");
		try
		{
			using var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var reader = new StreamReader(stream);

			using JsonDocument document = JsonDocument.Parse(reader.ReadToEnd());
			var spriteElement = document.RootElement.GetProperty("Sprite");

			var gridSize = spriteElement.GetProperty("grid_size");
			int width = gridSize.GetProperty("cols").GetInt32();
			int height = gridSize.GetProperty("rows").GetInt32();

			DebugLogger.Log($"Sprite animation grid size: {width}x{height}");
			var frameSprites = new List<Sprite>();
			if (spriteElement.TryGetProperty("Frames", out JsonElement framesArray))
			{
				foreach (JsonElement frameEntry in framesArray.EnumerateArray())
				{
					var sprite = new Sprite(new Vec2(width, height));
					var cells = frameEntry.GetProperty("cells");
					int i = 0;

					foreach (JsonElement cell in cells.EnumerateArray())
					{
						if (i >= width * height) break;

						char c = cell.TryGetProperty("char", out var ch) ? (ch.GetString()?[0] ?? ' ') : ' ';

						// Fix: Replaced Enum.TryParse (which fails on structs) with our custom mapping
						string fgStr = cell.GetProperty("fg").GetString() ?? "White";
						string bgStr = cell.GetProperty("bg").GetString() ?? "Black";

						Color fg = ParseColor(fgStr);
						Color bg = ParseColor(bgStr);

						sprite.WriteCell(new Vec2(i % width, i / width), new ScreenCell(c, fg, bg));
						i++;
					}
					frameSprites.Add(sprite);
				}
			}

			if (spriteElement.TryGetProperty("Animations", out JsonElement animationsArray))
			{
				foreach (JsonElement anim in animationsArray.EnumerateArray())
				{
					string? state = anim.GetProperty("state").GetString();
					if (!string.IsNullOrEmpty(state))
					{
						var animation = new Animation(frametime);
						foreach (JsonElement idx in anim.GetProperty("frames").EnumerateArray())
						{
							int index = idx.GetInt32();
							if (index >= 0 && index < frameSprites.Count)
							{
								animation.addFrame(frameSprites[index]);
							}
						}
						loadedAnimations[state] = animation;
					}
				}
			}

			DebugLogger.Log($"Sprite animations loaded successfully. Animation states: {loadedAnimations.Count}");
			return loadedAnimations;
		}
		catch (Exception ex)
		{
			DebugLogger.Log($"Exception loading sprite animations: {ex.Message}");
			return null;
		}
	}
}