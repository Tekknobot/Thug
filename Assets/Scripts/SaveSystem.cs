using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "sequencerData.json");

    public static void Save(SequencerData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true); // Serialize the object to JSON
            File.WriteAllText(SavePath, json); // Write JSON to a file
            Debug.Log($"Data successfully saved to {SavePath}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"Failed to save data: {ex.Message}");
        }
    }

    public static SequencerData Load()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath); // Read the JSON file
                SequencerData data = JsonUtility.FromJson<SequencerData>(json); // Deserialize to object
                Debug.Log($"Data successfully loaded from {SavePath}");
                return data;
            }
            catch (IOException ex)
            {
                Debug.LogError($"Failed to load data: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Save file not found. Returning default values.");
        }

        // Return default values if no save file exists
        return new SequencerData(120f, "", 0f);
    }
}
