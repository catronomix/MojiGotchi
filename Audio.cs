using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
#if WINDOWS
using NAudio.Wave;
#endif

public static class SoundManager
{
	private static float _volume = 0.5f;

	public static float Volume
	{
		get => _volume;
		set => _volume = Math.Clamp(value, 0f, 1f);
	}

	public static void Play(string filePath)
	{
		if (!File.Exists(filePath)) return;

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			PlayWindows(filePath);
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			PlayMac(filePath);
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			PlayLinux(filePath);
		}
	}

	private static void PlayWindows(string filePath)
	{
#if WINDOWS
        try
        {
            var outputDevice = new WaveOutEvent();
            var audioFile = new AudioFileReader(filePath);
            audioFile.Volume = _volume;

            outputDevice.Init(audioFile);
            outputDevice.Play();

            outputDevice.PlaybackStopped += (s, e) =>
            {
                outputDevice.Dispose();
                audioFile.Dispose();
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Windows Audio Error: {ex.Message}");
        }
#else
		Console.WriteLine("Windows audio requested on a non-Windows platform.");
#endif
	}

	private static void PlayMac(string filePath)
	{
		// afplay is native to macOS and very low latency
		ExecuteCommandLine("afplay", $"\"{filePath}\"");
	}

	private static void PlayLinux(string filePath)
	{
		// aplay is standard for .wav files on Linux
		ExecuteCommandLine("aplay", $"\"{filePath}\"");
	}

	private static void ExecuteCommandLine(string command, string arguments)
	{
		try
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = command,
				Arguments = arguments,
				CreateNoWindow = true,
				UseShellExecute = false
			});
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Linux/Mac Audio Error: {ex.Message}");
		}
	}
}