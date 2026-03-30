namespace MojiGotchi;

using System.Collections.Generic;
using System.IO;
using System.Text.Json;

class Program
{
	private static Dictionary<string, string> _gameOptions = new();

	private static void Main(string[] args)
	{
		ConsoleHelper.SetWindowSize(122,40);
		ConsoleHelper.EnableAnsiEscapeCodes(); // Enable ANSI support for the console
		ConsoleHelper.HideCursor();
		ConsoleHelper.EnableUTF8();

		DebugLogger.Enable();
		LoadGameOptions();

		//optionally start editor
		_gameOptions.TryGetValue( "devmode", out var devmode);
		if (devmode == "true")
		{
			Editor _editor = new Editor();
			bool devloop = true;
			while (devloop)
			{
				devloop = Loop(_editor);
			}
		}

		//start game
		Game _game = new Game();         // Declare as local variable
		_gameOptions.TryGetValue( "language", out var language);
		if (language == null)
		{
			SetupLanguage(_game);
		}

		bool running = true;
		//need to differentiate between game mode and editor mode
		while (running)
		{
			running = Loop(_game);
		}

	}

	private static bool Loop(Game game)
	{
		// Directly call Renderer.Resize and pass its boolean result to game.Step
		
		return game.Step();
	}

	private static bool Loop(Editor editor)
	{
		return editor.Step();
	}

	private static void LoadGameOptions()
	{
		try
		{
			using var stream = new FileStream("options.json", FileMode.Open, FileAccess.Read, FileShare.Read);
			using var reader = new StreamReader(stream);
			string jsonstring = reader.ReadToEnd();
			var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true};
			_gameOptions = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonstring, options) ?? new();
		}
		catch (Exception ex)
		{
			DebugLogger.Log($"Exception loading game options: {ex.Message}");
		}

		
		if (_gameOptions.TryGetValue("language", out var language))
		{
			LM.SetLanguage(language);
		}
		else
		{
			LM.SetLanguage("en");
		}
		
	}

	private static void SaveGameOptions()
	{
		try
		{
			var options = new JsonSerializerOptions { WriteIndented = true };
			string jsonString = JsonSerializer.Serialize(_gameOptions, options);
			using var stream = new FileStream("options.json", FileMode.Create, FileAccess.Write, FileShare.None);
			using var writer = new StreamWriter(stream);
			writer.Write(jsonString);	
		}
		catch (Exception ex)
		{
			DebugLogger.Log($"Exception saving game options: {ex.Message}");
		}
	}

	private static void SetupLanguage(Game game)
	{
		_gameOptions["language"] = "en";
		SaveGameOptions();
		game.ChooseLanguage();
	}
}