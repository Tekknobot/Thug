using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SequencerController : MonoBehaviour
{
    public AudioSource audioSourcePrefab;  // Prefab for playing notes
    public AudioClip[] noteClips;          // Array of audio clips for each pitch
    public float bpm = 120f;               // Beats per minute for playback

    private ScrollableGrid grid;           // Dynamically assigned ScrollableGrid
    private bool isPlaying = false;        // Is the sequencer currently playing?
    private int currentStep = 0;           // Current step in the sequence
    private List<AudioSource> audioSources = new List<AudioSource>(); // Pool of AudioSources for playback
    private float stepDuration;            // Duration of each step in seconds
    public Color activeColumnColor = new Color(1f, 0.8f, 0.4f, 0.5f); // Tint for active column

    private void Start()
    {
        // Start a coroutine to wait for the ScrollableGrid to be available
        StartCoroutine(WaitForGridInitialization());

        // Calculate step duration based on BPM
        stepDuration = 60f / bpm;
    }

    private IEnumerator WaitForGridInitialization()
    {
        // Wait until the ScrollableGrid with the tag "SequencerGrid" is found
        while (grid == null)
        {
            GameObject gridObject = GameObject.FindGameObjectWithTag("SequencerGrid");
            if (gridObject != null)
            {
                grid = gridObject.GetComponent<ScrollableGrid>();
            }
            yield return null; // Wait for the next frame
        }

        // Initialize the audio source pool after the grid is found
        InitializeAudioSources();
    }

    private void InitializeAudioSources()
    {
        for (int i = 0; i < grid.rows; i++)
        {
            AudioSource source = Instantiate(audioSourcePrefab, transform);
            audioSources.Add(source);
        }
    }

    public void Play()
    {
        if (isPlaying || grid == null) return;
        isPlaying = true;
        currentStep = 0;
        StartCoroutine(PlaySequence());
    }

    public void Pause()
    {
        isPlaying = false;
        StopAllCoroutines();
    }

    public void Stop()
    {
        isPlaying = false;
        StopAllCoroutines();
        ResetAllColumns();
        currentStep = 0;
    }

    private IEnumerator PlaySequence()
    {
        while (isPlaying)
        {
            HighlightColumn(currentStep);
            PlayStep(currentStep);
            currentStep = (currentStep + 1) % grid.columns;
            yield return new WaitForSeconds(stepDuration);
        }
    }

    private void PlayStep(int step)
    {
        for (int row = 0; row < grid.rows; row++)
        {
            CustomToggle toggle = grid.GetToggleAt(row, step);
            if (toggle != null && toggle.GetState())
            {
                PlayNote(row);
            }
        }
    }

    private void PlayNote(int row)
    {
        if (row >= 0 && row < noteClips.Length)
        {
            AudioSource source = audioSources[row];
            source.clip = noteClips[row];
            source.Play();
        }
    }

    private void HighlightColumn(int columnIndex)
    {
        // Reset all columns before highlighting the current column
        ResetAllColumns();

        for (int row = 0; row < grid.rows; row++)
        {
            CustomToggle toggle = grid.GetToggleAt(row, columnIndex);
            if (toggle != null)
            {
                // Temporarily tint the column's step with the active color
                toggle.SetDefaultOffColor(activeColumnColor);
            }
        }
    }

    private void ResetAllColumns()
    {
        if (grid == null) return;

        for (int row = 0; row < grid.rows; row++)
        {
            for (int col = 0; col < grid.columns; col++)
            {
                CustomToggle toggle = grid.GetToggleAt(row, col);
                if (toggle != null)
                {
                    // Reset to the appropriate default color based on whether it's a black or white key row
                    bool isBlackKey = grid.IsBlackKey(row % 12);
                    toggle.SetDefaultOffColor(isBlackKey ? grid.blackKeyRowColor : grid.whiteKeyRowColor);
                }
            }
        }
    }
}
