using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    public SequencerController sequencerController; // Reference to the SequencerController
    public Sprite playIcon; // Play button icon
    public Sprite stopIcon; // Stop button icon

    private Button playButton; // Reference to the button
    private bool isPlaying = false; // Tracks if the sequencer is playing

    private void Start()
    {
        // Get the Button component and set up the listener
        playButton = GetComponent<Button>();
        if (playButton != null)
        {
            playButton.onClick.AddListener(TogglePlayStop);
        }

        // Set the initial icon to Play
        UpdateIcon();
    }

    /// <summary>
    /// Toggles between Play and Stop.
    /// </summary>
    private void TogglePlayStop()
    {
        if (sequencerController == null) return;

        if (isPlaying)
        {
            sequencerController.Stop();
            isPlaying = false;
        }
        else
        {
            sequencerController.Play();
            isPlaying = true;
        }

        UpdateIcon();
    }

    /// <summary>
    /// Updates the button icon based on the current state.
    /// </summary>
    private void UpdateIcon()
    {
        if (playButton != null)
        {
            Image image = playButton.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = isPlaying ? stopIcon : playIcon;
            }
        }
    }
}
