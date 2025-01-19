using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    public string sequencerControllerTag = "SequencerController"; // Tag for the SequencerController
    public Sprite playIcon; // Play button icon
    public Sprite stopIcon; // Stop button icon

    private Button playButton; // Reference to the button
    public SequencerController sequencerController; // Reference to the SequencerController

    private void Start()
    {
        // Find the SequencerController by tag if not assigned
        if (sequencerController == null)
        {
            GameObject controllerObject = GameObject.FindGameObjectWithTag(sequencerControllerTag);
            if (controllerObject != null)
            {
                sequencerController = controllerObject.GetComponent<SequencerController>();
            }
        }

        // Get the Button component and set up the listener
        playButton = GetComponent<Button>();
        if (playButton != null)
        {
            playButton.onClick.AddListener(TogglePlayStop);
        }

        // Set the initial icon based on the sequencer's state
        UpdateIcon();
    }

    /// <summary>
    /// Toggles between Play and Stop based on the sequencer's state.
    /// </summary>
    private void TogglePlayStop()
    {
        if (sequencerController == null) return;

        // Use the isPlaying state from the SequencerController
        if (sequencerController.isPlaying)
        {
            sequencerController.Stop();
        }
        else
        {
            sequencerController.Play();
        }

        // Update the button icon after the state changes
        UpdateIcon();
    }

    /// <summary>
    /// Updates the button icon based on the sequencer's playing state.
    /// </summary>
    private void UpdateIcon()
    {
        if (playButton != null && sequencerController != null)
        {
            Image image = playButton.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = sequencerController.isPlaying ? stopIcon : playIcon;
            }
        }
    }
}
