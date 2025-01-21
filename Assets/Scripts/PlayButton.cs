using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    public string sequencerControllerTag = "SequencerController"; // Tag for the SequencerController
    public string drumControllerTag = "DrumController";           // Tag for the DrumController
    public string synthesizerTag = "Synthesizer";                // Tag for the Sequencer's Synthesizer
    public string drumSynthesizerTag = "DrumSynthesizer";        // Tag for the Drum Synthesizer
    public Sprite playIcon;                                      // Play button icon
    public Sprite stopIcon;                                      // Stop button icon

    private Button playButton;                                   // Reference to the button
    public SequencerController sequencerController;              // Reference to the SequencerController
    public DrumController drumController;                        // Reference to the DrumController
    private Synthesizer sequencerSynthesizer;                    // Reference to the Sequencer's Synthesizer
    private Synthesizer drumSynthesizer;                         // Reference to the Drum Synthesizer

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

        // Find the DrumController by tag
        GameObject drumControllerObject = GameObject.FindGameObjectWithTag(drumControllerTag);
        if (drumControllerObject != null)
        {
            drumController = drumControllerObject.GetComponent<DrumController>();
        }

        // Find the Sequencer's Synthesizer by tag
        GameObject sequencerSynthObject = GameObject.FindGameObjectWithTag(synthesizerTag);
        if (sequencerSynthObject != null)
        {
            sequencerSynthesizer = sequencerSynthObject.GetComponent<Synthesizer>();
        }

        // Find the Drum Synthesizer by tag
        GameObject drumSynthObject = GameObject.FindGameObjectWithTag(drumSynthesizerTag);
        if (drumSynthObject != null)
        {
            drumSynthesizer = drumSynthObject.GetComponent<Synthesizer>();
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
    /// Toggles between Play and Stop based on the sequencers' states.
    /// </summary>
    private void TogglePlayStop()
    {
        if (sequencerController == null || drumController == null) return;

        // Use the isPlaying state from the SequencerController and DrumController
        if (sequencerController.isPlaying || drumController.isPlaying)
        {
            sequencerController.Stop();
            drumController.Stop();

            // Stop all notes if the synthesizers are available
            if (sequencerSynthesizer != null)
            {
                sequencerSynthesizer.StopAllNotes();
            }

            if (drumSynthesizer != null)
            {
                drumSynthesizer.StopAllNotes();
            }
        }
        else
        {
            sequencerController.Play();
            drumController.Play();
        }

        // Update the button icon after the state changes
        UpdateIcon();
    }

    /// <summary>
    /// Updates the button icon based on the sequencers' playing state.
    /// </summary>
    private void UpdateIcon()
    {
        if (playButton != null && sequencerController != null && drumController != null)
        {
            Image image = playButton.GetComponent<Image>();
            if (image != null)
            {
                bool isPlaying = sequencerController.isPlaying || drumController.isPlaying;
                image.sprite = isPlaying ? stopIcon : playIcon;
            }
        }
    }
}
