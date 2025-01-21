using UnityEngine;
using UnityEngine.UI;

public class BPMSlider : MonoBehaviour
{
    public Slider bpmSlider;                    // Reference to the BPM slider
    public DrumSynthesizer drumSynthesizer;     // Reference to the DrumSynthesizer
    public DrumController drumController;       // Reference to the DrumController

    private void Start()
    {
        if (bpmSlider != null)
        {
            // Add a listener to the slider to update BPM and step duration
            bpmSlider.onValueChanged.AddListener(OnBpmChanged);

            // Initialize with the slider's current value
            OnBpmChanged(bpmSlider.value);
        }
        else
        {
            Debug.LogError("BPM slider not assigned in BPMSlider script.");
        }
    }

    private void OnBpmChanged(float newBpm)
    {
        // Update the BPM in the DrumController
        if (drumController != null)
        {
            drumController.bpm = newBpm;
            drumController.UpdateStepDuration();
        }
        else
        {
            Debug.LogError("DrumController not assigned in BPMSlider script.");
        }

        // Update the step duration in the DrumSynthesizer
        if (drumSynthesizer != null)
        {
            drumSynthesizer.SetStepDuration(newBpm);
        }
        else
        {
            Debug.LogError("DrumSynthesizer not assigned in BPMSlider script.");
        }

        Debug.Log($"BPM changed to {newBpm}.");
    }
}
