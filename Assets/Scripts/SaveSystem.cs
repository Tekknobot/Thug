using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "sequencerData.json");
    private static string SavePath_Drum => Path.Combine(Application.persistentDataPath, "drum_sequencerData.json");

    /// <summary>
    /// Save data for the main sequencer.
    /// </summary>
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

    /// <summary>
    /// Load data for the main sequencer.
    /// </summary>
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

    /// <summary>
    /// Save data for the drum sequencer.
    /// </summary>
    public static void SaveDrum(DrumSequencerData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true); // Serialize the object to JSON
            File.WriteAllText(SavePath_Drum, json); // Write JSON to the drum save file
            Debug.Log($"Drum data successfully saved to {SavePath_Drum}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"Failed to save drum data: {ex.Message}");
        }
    }

    /// <summary>
    /// Load data for the drum sequencer.
    /// </summary>
    public static DrumSequencerData LoadDrum()
    {
        if (File.Exists(SavePath_Drum))
        {
            try
            {
                string json = File.ReadAllText(SavePath_Drum); // Read the drum JSON file
                DrumSequencerData data = JsonUtility.FromJson<DrumSequencerData>(json); // Deserialize to object
                Debug.Log($"Drum data successfully loaded from {SavePath_Drum}");
                return data;
            }
            catch (IOException ex)
            {
                Debug.LogError($"Failed to load drum data: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Drum save file not found. Returning default values.");
        }

        // Return default values if no save file exists
        return new DrumSequencerData(120f, "", 0f);
    }
}
