using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PresetSwitcher : MonoBehaviour
{
    public Synthesizer synthesizer; // Reference to the Synthesizer
    public SynthVolume synthVolume; // Reference to the SynthVolume script
    public TextMeshProUGUI presetNameText; // Text to display the current preset name
    public Button previousButton; // Button to go to the previous preset
    public Button nextButton; // Button to go to the next preset

    private int currentPresetIndex = 0; // Tracks the current preset
    private readonly List<string> presetNames = new List<string>
    {
        "DistortedSaw",
        "CrunchySquare",
        "FrenchHouseBass",
        "TechnoBass",
        "DeepSubBass",
        "PiercingLead",
        "PunchyBass",
        "MetallicPluck",
        "DistortedSquareBass",
        "PercussiveBlip",
        "SmoothHouseBass"
    };

    private Dictionary<string, System.Action> presetActions; // Map preset names to actions
    private const string PRESET_KEY = "SelectedPreset"; // PlayerPrefs key for saving the preset

    private void Start()
    {
        Debug.Log($"Initializing PresetSwitcher with {presetNames.Count} presets.");

        // Ensure Synthesizer is assigned
        if (synthesizer == null)
        {
            Debug.LogError("Synthesizer not assigned to PresetSwitcher!");
            return;
        }

        // Ensure SynthVolume is assigned
        if (synthVolume == null)
        {
            Debug.LogError("SynthVolume not assigned to PresetSwitcher!");
            return;
        }

        // Ensure TextMeshProUGUI is assigned
        if (presetNameText == null)
        {
            Debug.LogError("Preset name text not assigned to PresetSwitcher!");
            return;
        }

        // Add listeners to the buttons
        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousPreset);

        if (nextButton != null)
            nextButton.onClick.AddListener(NextPreset);

        // Initialize preset actions
        InitializePresets();

        // Log available presets
        Debug.Log($"Available presets: {string.Join(", ", presetNames)}");

        // Load the saved preset or default to the first one
        LoadPreset();
    }

    /// <summary>
    /// Initializes the dictionary mapping preset names to their corresponding methods.
    /// </summary>
    private void InitializePresets()
    {
        presetActions = new Dictionary<string, System.Action>
        {
            { "DistortedSaw", synthesizer.DistortedSaw },
            { "CrunchySquare", synthesizer.CrunchySquare },
            { "FrenchHouseBass", synthesizer.FrenchHouseBass },
            { "TechnoBass", synthesizer.TechnoBass },
            { "DeepSubBass", synthesizer.DeepSubBass },
            { "PiercingLead", synthesizer.PiercingLead },
            { "PunchyBass", synthesizer.PunchyBass },
            { "MetallicPluck", synthesizer.MetallicPluck },
            { "DistortedSquareBass", synthesizer.DistortedSquareBass },
            { "PercussiveBlip", synthesizer.PercussiveBlip },
            { "SmoothHouseBass", synthesizer.SmoothHouseBass }
        };

        Debug.Log($"Initialized {presetActions.Count} preset actions.");
    }

    /// <summary>
    /// Applies the current preset.
    /// </summary>
    private void ApplyPreset()
    {
        Debug.Log($"Applying preset: {presetNames[currentPresetIndex]} (Index: {currentPresetIndex})");

        if (presetActions.TryGetValue(presetNames[currentPresetIndex], out var presetAction))
        {
            presetAction?.Invoke(); // Invoke the corresponding preset method
            UpdatePresetName();

            // Notify the SynthVolume script to update the slider
            synthVolume.OnPresetLoaded();

            Debug.Log($"Preset text updated: {presetNames[currentPresetIndex]}");
            SavePreset();
        }
        else
        {
            Debug.LogError($"Unknown preset: {presetNames[currentPresetIndex]}");
        }
    }

    /// <summary>
    /// Switch to the previous preset.
    /// </summary>
    public void PreviousPreset()
    {
        currentPresetIndex = (currentPresetIndex - 1 + presetNames.Count) % presetNames.Count; // Cycle backward
        Debug.Log($"Switching to previous preset: {presetNames[currentPresetIndex]} (Index: {currentPresetIndex})");
        ApplyPreset();
    }

    /// <summary>
    /// Switch to the next preset.
    /// </summary>
    public void NextPreset()
    {
        currentPresetIndex = (currentPresetIndex + 1) % presetNames.Count; // Cycle forward
        Debug.Log($"Switching to next preset: {presetNames[currentPresetIndex]} (Index: {currentPresetIndex})");
        ApplyPreset();
    }

    /// <summary>
    /// Updates the displayed preset name.
    /// </summary>
    private void UpdatePresetName()
    {
        if (presetNameText != null)
            presetNameText.text = presetNames[currentPresetIndex];
        Debug.Log($"Updated preset name: {presetNames[currentPresetIndex]}");
    }

    /// <summary>
    /// Saves the current preset index to PlayerPrefs.
    /// </summary>
    private void SavePreset()
    {
        PlayerPrefs.SetInt(PRESET_KEY, currentPresetIndex);
        PlayerPrefs.Save();
        Debug.Log($"Preset saved: {presetNames[currentPresetIndex]} (Index: {currentPresetIndex})");
    }

    /// <summary>
    /// Loads the saved preset index from PlayerPrefs.
    /// </summary>
    private void LoadPreset()
    {
        if (PlayerPrefs.HasKey(PRESET_KEY))
        {
            currentPresetIndex = PlayerPrefs.GetInt(PRESET_KEY);
            currentPresetIndex = Mathf.Clamp(currentPresetIndex, 0, presetNames.Count - 1); // Ensure index is valid
            Debug.Log($"Loaded preset index: {currentPresetIndex}");
            ApplyPreset();
        }
        else
        {
            Debug.Log("No preset saved. Defaulting to first preset.");
            ApplyPreset(); // Default to the first preset
        }
    }
}
