namespace MojiGotchi;

using System.Runtime.InteropServices;
using System.Text;

public static class ConsoleHelper
{
	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr GetStdHandle(int nStdHandle);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

	private const int STD_OUTPUT_HANDLE = -11;
	private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

	public static void EnableAnsiEscapeCodes()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
			if (GetConsoleMode(iStdOut, out uint outConsoleMode))
			{
				outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
				SetConsoleMode(iStdOut, outConsoleMode);
			}
		}
	}

	public static string GetForegroundAnsiCode(Color color)
	{
		return $"\x1b[38;2;{color.R};{color.G};{color.B}m";
	}

	public static string GetBackgroundAnsiCode(Color color)
	{
		return $"\x1b[48;2;{color.R};{color.G};{color.B}m";
	}

	public static void HideCursor() => Console.CursorVisible = false;

	public static void EnableUTF8() => Console.OutputEncoding = Encoding.UTF8;

	public static void SetWindowSize(int width, int height)
	{
		width = Math.Min(width, Console.LargestWindowWidth);
		height = Math.Min(height, Console.LargestWindowHeight);

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			Console.WindowWidth = width;
			Console.WindowHeight = height;
		}
		else
		{
			Console.Write($"\x1b[8;{height};{width}t");
		}
	}

	public static Vec2 GetWindowSize() => new Vec2(Console.WindowWidth, Console.WindowHeight);
}