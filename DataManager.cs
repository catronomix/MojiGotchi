namespace MojiGotchi;

using System.Text.Json; // Still needed for savegame.json
using System.IO; // Needed for file operations

static class DataManager
{
    static string saveFile = "savegame.json";
    static string highScoreFile = "highscores.txt"; // Changed to .txt for flat file
    public static bool SavePet(Pet myPet)
    {
        bool success = false;
        if (EnsureSaveFileExists())
        {
            try
            {
                myPet.SaveTime = DateTime.Now;
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(myPet, options);
                File.WriteAllText(saveFile, jsonString);
                success = true;
            }
            catch
            {
                //do nothing
                success = false;
            }
        }
        return success;
        
    }

    public static Pet? LoadPet()
    {
        try
        {
            if (!File.Exists(saveFile)) return null;

            string jsonString = File.ReadAllText(saveFile);
            if (string.IsNullOrWhiteSpace(jsonString) || jsonString == "{}") return null;
            
            Pet? myPet = JsonSerializer.Deserialize<Pet>(jsonString);
            if (myPet != null)
            {
                myPet = UpdateDateTimes(myPet);
                myPet.Animations = JsonParser.LoadPetAnimations("PetSprites.json", 500);
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
        if (!File.Exists(saveFile))
        {
            // Create an empty file or a file with a null/empty JSON object
            try{
                File.WriteAllText(saveFile, "{}");
            }
            catch
            {
                return false;
            }
        }
        return true;
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
                File.WriteAllText(highScoreFile, "Name\tAge\tDateOfDeath\n");
            }
            catch (Exception ex)
            {
                // Log or handle the error appropriately
                Console.WriteLine($"Error creating high score file: {ex.Message}");
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
            string[] lines = File.ReadAllLines(highScoreFile);
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