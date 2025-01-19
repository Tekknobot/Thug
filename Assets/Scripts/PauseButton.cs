using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
    public SequencerController sequencerController; // Reference to the SequencerController
    public string synthesizerTag = "Synthesizer";   // Tag to find the Synthesizer
    private Synthesizer synthesizer;               // Reference to the Synthesizer

    private void Start()
    {
        // Get the Button component and set up the listener
        Button pauseButton = GetComponent<Button>();
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(PauseSequencer);
        }

        // Find the synthesizer by tag
        GameObject synthObject = GameObject.FindGameObjectWithTag(synthesizerTag);
        if (synthObject != null)
        {
            synthesizer = synthObject.GetComponent<Synthesizer>();
        }

        if (synthesizer == null)
        {
            Debug.LogError("Synthesizer not found! Ensure it has the correct tag.");
        }
    }

    /// <summary>
    /// Pauses the sequencer and stops all notes from the synthesizer.
    /// </summary>
    private void PauseSequencer()
    {
        if (sequencerController != null)
        {
            sequencerController.Pause();
        }

        if (synthesizer != null)
        {
            synthesizer.StopAllNotes();
            Debug.Log("All notes stopped.");
        }
    }
}
