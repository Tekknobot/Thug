using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro; // Import TextMeshPro namespace

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
    public Synthesizer synthesizer; // Reference to the synthesizer
    private const string BPM_KEY = "SequencerBPM";
    private const string GRID_STATE_KEY = "SequencerGridState";
    private const string SCROLL_POSITION_KEY = "ScrollPosition";
    public TextMeshProUGUI bpmText; // Reference to TextMeshPro text for displaying BPM

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

        StartCoroutine(LoadGridStateFromJson());   
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

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveData();
        }
    }

    private void OnApplicationQuit()
    {
        SaveData();
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
        // Keep track of active rows in this step
        List<int> activeRows = new List<int>();

        for (int row = 0; row < grid.rows; row++)
        {
            CustomToggle toggle = grid.GetToggleAt(row, step);

            if (toggle != null && toggle.GetState())
            {
                // Play the note for the active cell
                PlayNote(row);
                activeRows.Add(row); // Track the row playing
            }
        }

        // Stop notes for rows that are no longer active
        StopInactiveNotes(activeRows);
    }

    private void PlayNote(int row)
    {
        if (row < 0 || row >= grid.rows) return;

        // Calculate the frequency for the given row, starting from C1
        float frequency = GetFrequencyForRow(row);

        Debug.Log($"Playing note for row {row} at frequency {frequency} Hz");

        // Trigger the synthesizer to play the calculated frequency
        if (synthesizer != null)
        {
            synthesizer.PlayNote(frequency);
        }
    }

    private void StopInactiveNotes(List<int> activeRows)
    {
        // Iterate through all rows
        for (int row = 0; row < grid.rows; row++)
        {
            // If a row is not in the active list, stop its note
            if (!activeRows.Contains(row) && synthesizer != null)
            {
                float frequency = GetFrequencyForRow(row);
                synthesizer.StopNote(frequency); // Stop the note for this row
            }
        }
    }

    public void Stop()
    {
        isPlaying = false;
        StopAllCoroutines();
        ResetAllColumns();
        currentStep = 0;

        if (synthesizer != null)
        {
            synthesizer.StopAllNotes(); // Stop all active notes
        }

        Debug.Log("Sequencer stopped.");
    }



    private float GetFrequencyForRow(int row)
    {
        float baseFrequency = 32.703f; // Frequency of C1
        int adjustedRow = grid.rows - 1 - row; // Reverse the row index
        return baseFrequency * Mathf.Pow(2f, adjustedRow / 12f);
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
        UpdateBpmText(); // Update the BPM text
        Debug.Log($"BPM updated to {bpm}.");
    }

    private void UpdateBpmText()
    {
        if (bpmText != null)
        {
            bpmText.text = bpm.ToString("F0"); // Display BPM as a whole number
        }
    }


    public IEnumerator LoadGridStateFromJson()
    {
        // Ensure grid initialization is complete
        while (!isInitialized)
        {
            yield return null;
        }

        // Load data from JSON using SaveSystem
        SequencerData data = SaveSystem.Load();

        if (data == null)
        {
            Debug.LogWarning("No saved data found. Loading default grid state.");
            yield break; // Exit if no saved data exists
        }

        // Apply the loaded BPM
        bpm = data.bpm;
        UpdateStepDuration(); // Update step duration based on loaded BPM

        if (bpmSlider != null)
        {
            bpmSlider.value = bpm; // Sync slider with the loaded BPM
        }

        // Deserialize the grid state string
        if (!string.IsNullOrEmpty(data.gridState))
        {
            int index = 0;
            for (int row = 0; row < grid.rows; row++)
            {
                for (int col = 0; col < grid.columns; col++)
                {
                    if (index < data.gridState.Length)
                    {
                        CustomToggle toggle = grid.GetToggleAt(row, col);
                        if (toggle != null)
                        {
                            toggle.SetState(data.gridState[index] == '1'); // Set toggle state
                        }
                        index++;
                    }
                }
            }
            Debug.Log("Grid state loaded from JSON.");
        }
        else
        {
            Debug.LogWarning("Loaded grid state is empty.");
        }

        // Apply the saved scroll position
        if (grid.stepScrollRect != null)
        {
            grid.stepScrollRect.verticalScrollbar.value = data.scrollPosition;
            Debug.Log($"Scroll position loaded: {data.scrollPosition}");
        }
    }


    public void SaveSequencerState()
    {
        // Save grid state
        string gridState = "";
        for (int row = 0; row < grid.rows; row++)
        {
            for (int col = 0; col < grid.columns; col++)
            {
                CustomToggle toggle = grid.GetToggleAt(row, col);
                gridState += toggle != null && toggle.GetState() ? "1" : "0";
            }
        }
    }


    public void SaveData()
    {
        // Gather the current state
        string gridState = ""; // Serialize the grid state
        for (int row = 0; row < grid.rows; row++)
        {
            for (int col = 0; col < grid.columns; col++)
            {
                CustomToggle toggle = grid.GetToggleAt(row, col);
                gridState += toggle != null && toggle.GetState() ? "1" : "0";
            }
        }

        float scrollPosition = grid.stepScrollRect?.verticalScrollbar.value ?? 0f;

        // Create the data object
        SequencerData data = new SequencerData(bpm, gridState, scrollPosition);

        // Save the data
        SaveSystem.Save(data);
    }

    public void LoadData()
    {
        SequencerData data = SaveSystem.Load();

        // Apply the loaded data
        bpm = data.bpm;
        UpdateStepDuration();

        if (bpmSlider != null)
        {
            bpmSlider.value = bpm; // Sync slider
        }

        // Deserialize grid state
        int index = 0;
        for (int row = 0; row < grid.rows; row++)
        {
            for (int col = 0; col < grid.columns; col++)
            {
                if (index < data.gridState.Length)
                {
                    CustomToggle toggle = grid.GetToggleAt(row, col);
                    if (toggle != null)
                    {
                        toggle.SetState(data.gridState[index] == '1');
                    }
                    index++;
                }
            }
        }

        // Apply scroll position
        if (grid.stepScrollRect != null)
        {
            grid.stepScrollRect.verticalScrollbar.value = data.scrollPosition;
        }

        Debug.Log("Sequencer data loaded and applied.");
    }
}
