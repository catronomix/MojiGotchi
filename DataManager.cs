namespace MojiGotchi;

using System.Text.Json; // Still needed for savegame.json
using System.IO; // Needed for file operations

static class DataManager
{
	static string saveFile = Path.Combine(AppContext.BaseDirectory, "savegame.json");
	static string highScoreFile = "highscores.txt"; // Changed to .txt for flat file
	public static bool SavePet(Pet myPet)
	{
		bool success = false;
		DebugLogger.Log("SavePet called");
		if (EnsureSaveFileExists())
		{
			try
			{
				myPet.SaveTime = DateTime.Now;
				var context = new PetJsonContext();
				string jsonString = JsonSerializer.Serialize(myPet, typeof(Pet), context);
				DebugLogger.Log($"About to write JSON ({jsonString.Length} bytes)");
				using var filestream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);
				using var sw = new StreamWriter(filestream);
				sw.Write(jsonString);
				DebugLogger.Log("File written successfully");
				success = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error saving pet: {ex.Message}");
				DebugLogger.Log($"Exception: {ex.Message}");
				success = false;
			}
		}
		else
		{
			DebugLogger.Log("EnsureSaveFileExists returned false");
		}
		return success;
		
	}

	public static Pet? LoadPet()
	{
		try
		{
			if (!File.Exists(saveFile)) return null;
			using var filestream = new FileStream(saveFile, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var sr = new StreamReader(filestream);
			string jsonString = sr.ReadToEnd();
			if (string.IsNullOrWhiteSpace(jsonString) || jsonString == "{}") return null;
			
			var context = new PetJsonContext();
			Pet? myPet = JsonSerializer.Deserialize<Pet>(jsonString, context.Pet);
			if (myPet != null)
			{
				myPet = UpdateDateTimes(myPet);
				// myPet.Animations = JsonParser.LoadAnimations("PetSprites.json");
				myPet.ApplyColorToAnimations();
			}
			return myPet;
		}
		catch
		{
			return null;
		}
	}

	public static Pet UpdateDateTimes(Pet myPet)
	{
		// Calculate the time that has passed since the game was saved
		TimeSpan elapsedTime = DateTime.Now - myPet.SaveTime;

		// Add the elapsed time to all relevant timestamps to "pause" time
		myPet.BirthTime += elapsedTime;
		myPet.LastFed += elapsedTime;
		myPet.LastPlayed += elapsedTime;
		myPet.LastPetted += elapsedTime;
		myPet.LastWaked += elapsedTime;
		
		// Also adjust the timestamps within each Stat object
		myPet.Saturation.LastUpdated += elapsedTime;
		myPet.Energy.LastUpdated += elapsedTime;
		myPet.Mood.LastUpdated += elapsedTime;
		myPet.Sleepyness.LastUpdated += elapsedTime;

		return myPet;
	}

	public static bool EnsureSaveFileExists()
{
	try
	{
		// FileMode.CreateNew: Specifies that the OS should create a new file. 
		// If the file already exists, an IOException is thrown.
		using var stream = new FileStream(saveFile, FileMode.CreateNew, FileAccess.Write, FileShare.None);
		using var writer = new StreamWriter(stream);
		
		writer.Write("{}");
		return true;
	}
	catch (IOException)
	{
		// If the exception is specifically because the file exists, 
		// we technically succeeded in our goal of "Ensuring it exists."
		if (File.Exists(saveFile)) 
		{
			return true;
		}
		return false;
	}
	catch (Exception)
	{
		// Handle other issues like permissions or directory missing
		return false;
	}
}

	public static void DeleteSave()
	{
		if (File.Exists(saveFile))
		{
			File.Delete(saveFile);
		}
	
	}

	// Ensures the high score file exists, creating it with a header if not.
	public static bool EnsureHighScoreFileExists()
	{
		if (!File.Exists(highScoreFile))
		{
			try
			{
				// Create with a header for tab-delimited data
				using var stream = new FileStream(highScoreFile, FileMode.CreateNew, FileAccess.Write, FileShare.None);
				using var writer = new StreamWriter(stream);
				writer.WriteLine("Name\tAge\tDateOfDeath");
				DebugLogger.Log("Highscore file created");
			}
			catch (Exception ex)
			{
				// Log or handle the error appropriately
				Console.WriteLine($"HighScore file inaccesible: {ex.Message}");
				return false;
			}
		}
		return true;
	}

	public static void AddHighScore(Pet myPet)
	{
		if (myPet == null) return; // Defensive check

		if (!EnsureHighScoreFileExists())
		{
			// If the file cannot be ensured, we cannot save the high score.
			return;
		}

		// Format the entry as tab-delimited: Name\tAgeSeconds\tDateOfDeath (ISO 8601)
		double ageSeconds = (DateTime.Now - myPet.BirthTime).TotalSeconds;
		string newEntry = $"{myPet.Name}\t{ageSeconds:F0}\t{DateTime.Now:o}";

		// Append the new entry to the file
		File.AppendAllText(highScoreFile, newEntry + "\n");
	}

	public static List<string[]> GetHighScores()
	{
		if (!EnsureHighScoreFileExists())
		{
			// If the file cannot be ensured, we cannot load high scores.
			return new List<string[]>();
		}

		List<string[]> highScores = new List<string[]>();
		try
		{
			using var stream = new FileStream(highScoreFile, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var reader = new StreamReader(stream);
			
			string[] lines = reader.ReadToEnd().Split('\n');

			// Skip the header line
			for (int i = 1; i < lines.Length; i++)
			{
				if (string.IsNullOrWhiteSpace(lines[i])) continue;
				
				string[] parts = lines[i].Split('\t');
				if (parts.Length >= 3)
				{
					highScores.Add(parts);
				}
			}
		}
		catch
		{
			// ignore, we have our empty list to return
		}
		return highScores;
	}

	static public void ClearHighScores()
	{
		if (File.Exists(highScoreFile))
		{
			File.Delete(highScoreFile);
		}
		EnsureHighScoreFileExists();
	}

	static public string GetAgeString(TimeSpan ageSpan)
	{
		int days = ageSpan.Days;
		int hours = ageSpan.Hours;
		int minutes = ageSpan.Minutes;

		string result = "";
		if (days > 0) result += $"{days} dagen, ";
		if (hours > 0 || days > 0) result += $"{hours} uren en ";
		result += $"{minutes} minuten";

		return result;
	}
}