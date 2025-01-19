using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SequencerController : MonoBehaviour
{
    public AudioSource audioSourcePrefab;
    public AudioClip[] noteClips;
    public float bpm = 120f; // Default BPM

    [HideInInspector]
    public bool isPlaying = false;
    [HideInInspector]
    public bool isInitialized = false; // Tracks whether initialization is complete
    private ScrollableGrid grid;
    private int currentStep = 0;
    private List<AudioSource> audioSources = new List<AudioSource>();
    private float stepDuration;
    public Color activeColumnColor = new Color(1f, 0.8f, 0.4f, 0.5f);

    public UnityEngine.UI.Slider bpmSlider; // Reference to the BPM slider

    private void Start()
    {
        StartCoroutine(WaitForGridInitialization());

        // Initialize the BPM from the slider's value (if assigned)
        if (bpmSlider != null)
        {
            bpm = bpmSlider.value;
            bpmSlider.onValueChanged.AddListener(UpdateBpm); // Listen for slider changes
        }

        // Calculate step duration based on the initial BPM
        UpdateStepDuration();
    }

    private IEnumerator WaitForGridInitialization()
    {
        // Wait until the ScrollableGrid is found
        while (grid == null)
        {
            GameObject gridObject = GameObject.FindGameObjectWithTag("SequencerGrid");
            if (gridObject != null)
            {
                grid = gridObject.GetComponent<ScrollableGrid>();
            }
            yield return null; // Wait for the next frame
        }

        InitializeAudioSources();
        isInitialized = true; // Set the initialized flag
        Debug.Log("SequencerController is initialized.");
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
        if (!isInitialized)
        {
            Debug.LogError("Cannot start sequencer: Grid is not initialized.");
            return;
        }

        if (isPlaying)
        {
            Debug.LogWarning("Sequencer is already playing.");
            return;
        }

        isPlaying = true;
        currentStep = 0;
        StartCoroutine(PlaySequence());
        Debug.Log("Sequencer started.");
    }

    public void Pause()
    {
        if (!isPlaying)
        {
            Debug.LogWarning("Sequencer is not playing.");
            return;
        }

        isPlaying = false;
        StopAllCoroutines();
        Debug.Log("Sequencer paused.");
    }

    public void Stop()
    {
        if (!isPlaying)
        {
            Debug.LogWarning("Sequencer is not playing.");
            return;
        }

        isPlaying = false;
        StopAllCoroutines();
        ResetAllColumns();
        currentStep = 0;
        Debug.Log("Sequencer stopped.");
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
        ResetAllColumns();
        for (int row = 0; row < grid.rows; row++)
        {
            CustomToggle toggle = grid.GetToggleAt(row, columnIndex);
            if (toggle != null)
            {
                toggle.SetDefaultOffColor(activeColumnColor);
            }
        }
        Debug.Log($"Highlighting column: {columnIndex}");
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
                    bool isBlackKey = grid.IsBlackKey(row % 12);
                    toggle.SetDefaultOffColor(isBlackKey ? grid.blackKeyRowColor : grid.whiteKeyRowColor);
                }
            }
        }
    }

    public void UpdateStepDuration()
    {
        stepDuration = 60f / (bpm * 4);
        Debug.Log($"Step duration updated to {stepDuration} seconds per step at {bpm} BPM.");
    }

    private void UpdateBpm(float newBpm)
    {
        bpm = newBpm;
        UpdateStepDuration();
        Debug.Log($"BPM updated to {bpm}.");
    }
}
