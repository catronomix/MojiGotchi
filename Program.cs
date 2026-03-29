namespace MojiGotchi;

class Program
{
	private static void Main(string[] args)
	{
		ConsoleHelper.SetWindowSize(122,40);
		ConsoleHelper.EnableAnsiEscapeCodes(); // Enable ANSI support for the console
		ConsoleHelper.HideCursor();
		ConsoleHelper.EnableUTF8();

		DebugLogger.Enable(); 

		Game _game = new Game();             // Declare as local variable
		
		bool running = true;
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
}