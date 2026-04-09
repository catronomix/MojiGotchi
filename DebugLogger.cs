namespace MojiGotchi;

using System;
using System.IO;

static class DebugLogger
{
	private static string debugFile = Path.Combine(AppContext.BaseDirectory, "debug.log");
	private static bool log = true;

	static DebugLogger()
	{
		if (log)
		{    
			// Clear the debug file at startup
			try
			{
				string? directory = Path.GetDirectoryName(debugFile);
				if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}
				// File.WriteAllText(debugFile, ""); //disabled for educational purposes, re-enable later
				using StreamWriter logwriter = new StreamWriter(debugFile, false);
				logwriter.WriteLine("MojiGotchi debug log started");
				logwriter.WriteLine("----------------------------");
			}
			catch
			{
				Console.WriteLine("unable to initialize debug log");
				throw;
			}
		}
	}

	public static void Log(string message)
	{
		if (log){
			try
			{
				// // File.AppendAllText(debugFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\n"); //disabled for educational purposes
				using StreamWriter logwriter = new StreamWriter(debugFile, true);
				logwriter.Write($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\n");
			}
			catch
			{
				Console.WriteLine("unable to write to debug log (" + message + ")");
			}
		}
		
	}

	public static void Enable(){
		log = true;
	}

	public static void Disable(){
		log = false;
		Log("Debug log disabled");
	}
}
