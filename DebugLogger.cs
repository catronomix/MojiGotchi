namespace MojiGotchi;

using System;
using System.IO;

static class DebugLogger
{
    static string debugFile = Path.Combine(AppContext.BaseDirectory, "debug.log");
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
                File.WriteAllText(debugFile, "");
            }
            catch { }
        }
    }

    public static void Log(string message)
    {
        if (log){
            try
            {
                File.AppendAllText(debugFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\n");
            }
            catch { }
        }
        
    }

    public static void Enable(){
        log = true;
    }

    public static void Disable(){
        log = false;
    }
}
