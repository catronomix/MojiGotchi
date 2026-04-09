namespace MojiGotchi;

using System.Collections.Generic;
using System.IO;

public enum LoopResult
{
    CONTINUE, //continue loop
    QUIT,
    GOTOGAME,
    GOTOEDITOR
}

class Program
{
	private static Dictionary<string, string> _gameOptions = new();

	private enum AppState
    {
        GAME,
        EDITOR,
        QUIT
    }

	private static void Main(string[] args)
	{
		ConsoleHelper.EnableAnsiEscapeCodes(); // Enable ANSI support for the console
		ConsoleHelper.SetWindowSize(116,33);
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

		// Enable/disable dev mode
        bool devMode = _gameOptions.ContainsKey("devmode") && _gameOptions["devmode"] == "true";

        AppState currentState = AppState.GAME;

		while (currentState != AppState.QUIT)
        {
            switch (currentState)
            {
                case AppState.EDITOR:
                    currentState = RunEditorLoop(devMode);
                    break;

                case AppState.GAME:
                    currentState = RunGameLoop(isFirstLanguageSetup, devMode);
                    isFirstLanguageSetup = false; 
                    break;
            }
        }

	}

	private static AppState RunGameLoop(bool showLanguageModal, bool devMode)
    {
        // Pass devMode to the Game constructor
        Game game = new Game(devMode);

        if (showLanguageModal)
        {
            game.ChooseLanguage();
        }

        while (true)
        {
            LoopResult result = game.Step(); 

            if (result == LoopResult.GOTOEDITOR) return AppState.EDITOR;
            if (result == LoopResult.QUIT) return AppState.QUIT;
        }
    }

    private static AppState RunEditorLoop(bool devMode)
    {
        // Pass devMode to the Editor constructor
        Editor editor = new Editor(devMode);

        while (true)
        {
            LoopResult result = editor.Step();

            if (result == LoopResult.GOTOGAME) return AppState.GAME;
            if (result == LoopResult.QUIT) return AppState.QUIT;
        }
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