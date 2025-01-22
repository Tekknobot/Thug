using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.IO; // Import TextMeshPro namespace

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

    private string saveFilePath;

    private void Awake()
    {
        // Set the save file path
        saveFilePath = Path.Combine(Application.persistentDataPath, "sequencerData.json");
    }

    private void Start()
    {
        StartCoroutine(InitializeAndLoadState());
    }

    private IEnumerator InitializeAndLoadState()
    {
        yield return StartCoroutine(WaitForGridInitialization()); // Wait for grid initialization
        yield return StartCoroutine(LoadGridStateFromJson());     // Load saved state

        // Sync the slider with the loaded BPM
        if (bpmSlider != null)
        {
            bpmSlider.value = bpm;
            bpmSlider.onValueChanged.AddListener(UpdateBpm); // Listen for slider changes
        }

        UpdateStepDuration();
        UpdateBpmText();
        Debug.Log("[SequencerController] Initialization and state loading complete.");
    }

    public void UpdateBpm(float newBpm)
    {
        bpm = newBpm;
        UpdateStepDuration();
        UpdateBpmText();
        Debug.Log($"[SequencerController] BPM updated to {bpm}");
    }

    private void UpdateStepDuration()
    {
        stepDuration = 60f / (bpm * 4); // 4 steps per beat
        Debug.Log($"[SequencerController] Step duration updated: {stepDuration:F3} seconds.");
    }

    private void UpdateBpmText()
    {
        if (bpmText != null)
        {
            bpmText.text = bpm.ToString("F0");
        }
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

    public void SaveData()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[SequencerController] Cannot save data: Sequencer is not initialized.");
            return;
        }

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

        // Create the SequencerData object
        SequencerData data = new SequencerData(bpm, gridState, scrollPosition);

        // Save to file
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"[SequencerController] Sequencer data saved to {saveFilePath}");
    }

    private IEnumerator LoadGridStateFromJson()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning($"[SequencerController] Save file not found at path: {saveFilePath}. Loading default state.");
            yield break;
        }

        string json = File.ReadAllText(saveFilePath);
        SequencerData data = JsonUtility.FromJson<SequencerData>(json);

        if (data == null)
        {
            Debug.LogError("[SequencerController] Failed to parse save file.");
            yield break;
        }

        // Apply the loaded BPM
        if (data.bpm > 0)
        {
            bpm = data.bpm;
            Debug.Log($"[SequencerController] Loaded BPM: {bpm}");
        }
        else
        {
            Debug.LogWarning("[SequencerController] Invalid BPM loaded. Using default.");
        }

        // Apply grid state
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
                            toggle.SetState(data.gridState[index] == '1');
                        }
                        index++;
                    }
                }
            }
            Debug.Log("[SequencerController] Grid state loaded.");
        }
        else
        {
            Debug.LogWarning("[SequencerController] Grid state is empty.");
        }

        // Apply scroll position
        if (grid.stepScrollRect != null)
        {
            grid.stepScrollRect.verticalScrollbar.value = data.scrollPosition;
            Debug.Log($"[SequencerController] Scroll position loaded: {data.scrollPosition}");
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
    
    public void LoadData()
    {
        SequencerData data = SaveSystem.Load();

        if (data == null)
        {
            Debug.LogWarning("No saved data found. Loading default sequencer state.");
            return;
        }

        // Apply the loaded BPM
        if (data.bpm > 0) // Ensure valid BPM
        {
            bpm = data.bpm;
            UpdateStepDuration();

            if (bpmSlider != null)
            {
                bpmSlider.value = bpm; // Sync slider with the loaded BPM
            }

            UpdateBpmText(); // Update BPM text UI
            Debug.Log($"Loaded BPM from save file: {bpm}");
        }

        // Apply the loaded grid state
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
                            toggle.SetState(data.gridState[index] == '1'); // Restore toggle state
                        }
                        index++;
                    }
                }
            }
            Debug.Log("Grid state loaded from save file.");
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

}
