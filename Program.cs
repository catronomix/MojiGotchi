namespace MojiGotchi;

using System.Collections.Generic;
using System.IO;

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
		
		// Load language BEFORE creating Game so menu items can be localized
		bool isFirstLanguageSetup = !_gameOptions.ContainsKey("language");
		string language = isFirstLanguageSetup ? "en" : _gameOptions["language"];
		
		if (isFirstLanguageSetup)
		{
			_gameOptions["language"] = language;
			SaveGameOptions();
		}
		LM.SetLanguage(language);

		//optionally start editor
		if (_gameOptions["devmode"] == "true")
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
		
		// Show language choice modal on first run
		if (isFirstLanguageSetup)
		{
			_game.ChooseLanguage();
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
			_gameOptions = OptionsJsonContextHelper.Deserialize(jsonstring);
			foreach (KeyValuePair<string, string> kv in _gameOptions)
			{
				DebugLogger.Log($"Game option loaded: {kv.Key}: {kv.Value}");
			}
		}
		catch (Exception ex)
		{
			DebugLogger.Log($"Exception loading game options: {ex.Message}");
			_gameOptions = new Dictionary<string, string>();
		}

		// Ensure default values exist
		if (!_gameOptions.ContainsKey("devmode"))
		{
			_gameOptions["devmode"] = "false";
		}
	}

	private static void SaveGameOptions()
	{
		try
		{
			string jsonString = OptionsJsonContextHelper.Serialize(_gameOptions);
			using var stream = new FileStream("options.json", FileMode.Create, FileAccess.Write, FileShare.None);
			using var writer = new StreamWriter(stream);
			writer.Write(jsonString);	
		}
		catch (Exception ex)
		{
			DebugLogger.Log($"Exception saving game options: {ex.Message}");
		}
	}

}