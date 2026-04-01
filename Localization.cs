namespace MojiGotchi;

using System.Collections.Generic;
using System.IO;
using System.Text.Json;

internal static class LM //shorthand for localizationmanager
{
	private static Dictionary<string, string> _currentStrings = new();
	private static string _currentLanguage = ""; //default to none so we always load once on game start

	internal static void SetLanguage(string language)
	{

		if (_currentLanguage == language) return; // check if changing

		string langfile = $"localization_{language}.json";

		if (!File.Exists(langfile))
		{
			DebugLogger.Log($"Language file not found: {langfile}");
			return;
		}

		try
		{
			using var stream = new FileStream(langfile, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var reader = new StreamReader(stream);
			string jsonstring = reader.ReadToEnd();
			_currentStrings.Clear();
			_currentStrings = OptionsJsonContextHelper.Deserialize(jsonstring);
			
			_currentLanguage = language;
			
			DebugLogger.Log($"Language file loaded: {_currentStrings.Count} strings (language: {_currentLanguage})");
		}
		catch (Exception ex)
		{
			DebugLogger.Log($"Exception loading language file: {ex.Message}");
		}
		
	}

	internal static string Get(string key)
	{
		if (_currentStrings.TryGetValue(key, out var value))
		{
			return value;
		}
		else
		{
			DebugLogger.Log($"Missing localization key: {key}");
			return key;
		}
		
	}

	internal static string Get(string key, params object?[] args)
    {
        if (!_currentStrings.TryGetValue(key, out var value))
        {
            DebugLogger.Log($"Missing localization key: {key}");
            return "";
        }

        if (args.Length == 0)
            return value;

        try
		{
			string formatted = string.Format(value, args);
            return formatted;
        }
        catch (FormatException ex)
        {
            DebugLogger.Log($"Format error for key '{key}': {ex.Message}");
            return value;
        }
    }

	internal static string CurrentLang()
	{
		return _currentLanguage;
	}
	
}

internal class LanguageChoice : Modal
{
	
	internal LanguageChoice() : base("Choose Language", Color.DarkCyan, Color.Cyan)
	{
		
	}

	internal void UpdatePage(Vec2 size)
	{
		SetSpriteBg(size);
		ClearContentSprite(size);

		string messagestring = "Language setting not found, defaulting to English.\nYou can change this in options.json.\nPress [ESC] to close this window]\n";
		messagestring += "Taalinstalling niet gevonden, standaardtaal is Engels.\nJe kan dit wijzigen in options.json\nDruk op [ESC] om dit venster te sluiten]";

		string[] message = messagestring.Split('\n');


		//turn message into sprite
		for (int i = 0; i < Math.Min(message.Length, size.Y - 5); i++)
		{

			Sprite lineSprite = new Sprite(new Vec2(message[i].Length, 1));

			for (int x = 0; x < message[i].Length; x++)
			{
				// write aligned horizontally
				lineSprite.WriteCell(new Vec2(x, 0), new ScreenCell 
				{ 
					Character = message[i][x], 
					Color = Color.White, 
					BgColor = this.BgColor
				});
			}
			AddContent(lineSprite, new Vec2(size.X / 2 - message[i].Length / 2, i*2 + 4));
		}
	}
}