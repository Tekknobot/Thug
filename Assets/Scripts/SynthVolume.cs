using UnityEngine;
using UnityEngine.UI;

public class SynthVolume : MonoBehaviour
{
    public Synthesizer synthesizer; // Reference to the Synthesizer
    public Slider volumeSlider;    // UI Slider to control the master volume

    private void Start()
    {
        // Ensure the synthesizer and slider are assigned
        if (synthesizer == null)
        {
            Debug.LogError("Synthesizer not assigned to SynthVolume!");
            return;
        }

        if (volumeSlider == null)
        {
            Debug.LogError("Volume slider not assigned to SynthVolume!");
            return;
        }

        // Initialize the slider to the current master volume from the synthesizer
        SyncSliderToSynthVolume();

        // Add listener for slider value changes
        volumeSlider.onValueChanged.AddListener(UpdateMasterVolume);
    }

    /// <summary>
    /// Updates the synthesizer's master volume based on the slider value.
    /// </summary>
    /// <param name="value">The new master volume value from the slider.</param>
    public void UpdateMasterVolume(float value)
    {
        synthesizer.masterVolume = value; // Set the master volume in the synthesizer
        Debug.Log($"Synth master volume updated to {value}");
    }

    /// <summary>
    /// Synchronizes the slider with the current master volume of the synthesizer.
    /// This is called during initialization or when a preset is loaded.
    /// </summary>
    private void SyncSliderToSynthVolume()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = synthesizer.masterVolume; // Set slider value to match synthesizer volume
            Debug.Log($"Slider initialized to match master volume: {synthesizer.masterVolume}");
        }
    }

    /// <summary>
    /// Called by the synthesizer whenever a preset is loaded to sync the slider.
    /// </summary>
    public void OnPresetLoaded()
    {
        SyncSliderToSynthVolume(); // Sync slider when a new preset is loaded
    }

    private void OnDestroy()
    {
        // Remove listener to prevent potential memory leaks
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(UpdateMasterVolume);
        }
    }
}
