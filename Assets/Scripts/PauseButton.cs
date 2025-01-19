using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
    public SequencerController sequencerController; // Reference to the SequencerController

    private void Start()
    {
        // Get the Button component and set up the listener
        Button pauseButton = GetComponent<Button>();
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(PauseSequencer);
        }
    }

    /// <summary>
    /// Pauses the sequencer.
    /// </summary>
    private void PauseSequencer()
    {
        if (sequencerController != null)
        {
            sequencerController.Pause();
        }
    }
}
