namespace MojiGotchi;

using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public static class LM //shorthand for localizationmanager
{
	private static Dictionary<string, string> _currentStrings = new();
	private static string _currentLanguage = ""; //default to none so we always load once on game start

	public static void SetLanguage(string language)
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
			var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true};
			_currentStrings.Clear();
			_currentStrings = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonstring, options) ?? new();
			
			_currentLanguage = language;
			

			DebugLogger.Log($"Language file loaded: {_currentStrings.Count} strings (language: {_currentLanguage})");
		}
		catch (Exception ex)
		{
			DebugLogger.Log($"Exception loading language file: {ex.Message}");
		}
		
	}

	public static string Get(string key)
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

	public static string Get(string key, params object?[] args)
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

	public static string CurrentLang()
	{
		return _currentLanguage;
	}

	
}

