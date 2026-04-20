namespace MojiGotchi;
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

	//optimized version: play from preloaded


	//original version: play from disk
	public static void PlaySound(byte[]? soundbytes)
	{
		if (soundbytes == null) return;

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			PlayWindows(soundbytes);
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			// currently not implemented
			// PlayMac(soundbytes);
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			//currently not implemented
			// PlayLinux(soundbytes);
		}
	}

	private static void PlayWindows(byte[] soundbytes)
	{
#if WINDOWS
		try
		{
			WaveOutEvent outputDevice = new WaveOutEvent();
			using MemoryStream ms = new MemoryStream(soundbytes);
			WaveFileReader audioFile = new WaveFileReader(ms);
			outputDevice.Volume = _volume;

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

	// private static void PlayMac(byte[] soundbytes)
	// {
	// 	// afplay is native to macOS and very low latency
	// 	ExecuteCommandLine("afplay", $"\"{soundbytes}\"");
	// }

	// private static void PlayLinux(byte[] soundbytes)
	// {
	// 	// aplay is standard for .wav files on Linux
	// 	ExecuteCommandLine("aplay", $"\"{soundbytes}\"");
	// }

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

public class Sounds
{
	//have sounds
	private Dictionary<string, byte[]?> _library;

	public Sounds()
	{
		_library = new();
	}

	public void AddSound(string name, string filename)
	{
		try
		{
			byte[]? stream = File.ReadAllBytes(filename);
			if (stream != null)
			{
				DebugLogger.Log(stream.ToString());
			}
			_library.Add(name, stream);
		}
		catch(Exception e)
		{
			DebugLogger.Log($"Error loading sound file {filename} ({e})");
		}
	}

	public byte[]? GetSound(string key)
	{
		return _library.GetValueOrDefault(key, null);
	}
}