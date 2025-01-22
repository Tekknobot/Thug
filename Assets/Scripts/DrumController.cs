using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.IO; // Import TextMeshPro namespace

public class DrumController : MonoBehaviour
{
    public DrumSampler drumSampler;   // Reference to the DrumSynthesizer
    public float bpm = 120f;                  // Default BPM

    [HideInInspector]
    public bool isPlaying = false;
    [HideInInspector]
    public bool isInitialized = false;       // Tracks whether initialization is complete

    private DrumMachineManager drumGrid;
    private int currentStep = 0;
    private float stepDuration;
    public Color activeColumnColor = new Color(1f, 0.8f, 0.4f, 0.5f);

    public Slider bpmSlider;                 // Reference to the BPM slider
    public TextMeshProUGUI bpmText;          // Reference to TextMeshPro text for displaying BPM

    private string saveFilePath;

    private void Awake()
    {
        // Set the save file path
        saveFilePath = Path.Combine(Application.persistentDataPath, "drum_sequencerData.json");
    }

    private void Start()
    {
        StartCoroutine(InitializeAndLoadState());
    }

    private IEnumerator InitializeAndLoadState()
    {
        // Simulate loading saved state
        yield return StartCoroutine(WaitForGridInitialization()); // Wait for grid initialization
        yield return StartCoroutine(LoadDrumSequencerState());

        // Sync the slider with the loaded BPM
        if (bpmSlider != null)
        {
            bpmSlider.value = bpm;
            bpmSlider.onValueChanged.AddListener(UpdateBpm); // Listen for slider changes
        }

        UpdateStepDuration();
        UpdateBpmText();
        Debug.Log("[DrumController] Initialization and state loading complete.");
    }

    public void UpdateBpm(float newBpm)
    {
        bpm = newBpm;
        UpdateStepDuration();
        UpdateBpmText();
        Debug.Log($"[DrumController] BPM updated to {bpm}");
    }

    public void UpdateStepDuration()
    {
        stepDuration = 60f / (bpm * 4); // 4 steps per beat
        Debug.Log($"[DrumController] Step duration updated: {stepDuration:F3} seconds.");
    }

    private void UpdateBpmText()
    {
        if (bpmText != null)
        {
            bpmText.text = bpm.ToString("F0");
        }
    }

    private IEnumerator LoadDrumSequencerState()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning($"[DrumController] Save file not found at path: {saveFilePath}. Loading default state.");
            yield break;
        }

        string json = File.ReadAllText(saveFilePath);
        DrumSequencerData data = JsonUtility.FromJson<DrumSequencerData>(json);

        if (data == null)
        {
            Debug.LogError($"[DrumController] Failed to parse save file at path: {saveFilePath}");
            yield break;
        }

        // Apply the loaded BPM
        if (data.bpm > 0)
        {
            bpm = data.bpm;
            Debug.Log($"[DrumController] Loaded BPM: {bpm}");
        }
        else
        {
            Debug.LogWarning("[DrumController] Invalid BPM loaded. Using default.");
        }

        // Apply grid state
        if (!string.IsNullOrEmpty(data.gridState))
        {
            int index = 0;
            for (int row = 0; row < drumGrid.drumRows; row++)
            {
                for (int col = 0; col < drumGrid.drumColumns; col++)
                {
                    if (index < data.gridState.Length)
                    {
                        CustomToggle toggle = drumGrid.GetToggleAt(row, col);
                        if (toggle != null)
                        {
                            toggle.SetState(data.gridState[index] == '1');
                        }
                        index++;
                    }
                }
            }
            Debug.Log("[DrumController] Drum grid state loaded.");
        }
        else
        {
            Debug.LogWarning("[DrumController] Grid state is empty.");
        }

        // Apply scroll position
        if (drumGrid.drumScrollRect != null)
        {
            drumGrid.drumScrollRect.verticalScrollbar.value = data.scrollPosition;
            Debug.Log($"[DrumController] Scroll position loaded: {data.scrollPosition}");
        }
    }
    
    public void SaveData()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[DrumController] Cannot save data: Drum grid is not initialized.");
            return;
        }

        string gridState = "";
        for (int row = 0; row < drumGrid.drumRows; row++)
        {
            for (int col = 0; col < drumGrid.drumColumns; col++)
            {
                CustomToggle toggle = drumGrid.GetToggleAt(row, col);
                gridState += toggle != null && toggle.GetState() ? "1" : "0";
            }
        }

        float scrollPosition = drumGrid.drumScrollRect?.verticalScrollbar.value ?? 0f;

        DrumSequencerData data = new DrumSequencerData(bpm, gridState, scrollPosition);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log("[DrumController] Drum sequencer data saved.");
    }

    private IEnumerator WaitForGridInitialization()
    {
        while (drumGrid == null)
        {
            GameObject gridObject = GameObject.FindGameObjectWithTag("DrumMachineGrid");
            if (gridObject != null)
            {
                drumGrid = gridObject.GetComponent<DrumMachineManager>();
                if (drumGrid != null)
                {
                    Debug.Log("Drum grid found and assigned.");
                }
                else
                {
                    Debug.LogError("Drum grid GameObject found, but DrumMachineManager script is missing!");
                }
            }
            else
            {
                Debug.LogWarning("Drum grid GameObject with tag 'DrumMachineGrid' not found. Retrying...");
            }
            yield return null; // Wait for the next frame
        }

        isInitialized = true; // Set the initialized flag
        Debug.Log("DrumController is initialized.");
    }

    public void Play()
    {
        if (!isInitialized)
        {
            Debug.LogError("Cannot start drum sequencer: Grid is not initialized.");
            return;
        }

        if (isPlaying)
        {
            Debug.LogWarning("Drum sequencer is already playing.");
            return;
        }

        isPlaying = true;
        currentStep = 0;
        StartCoroutine(PlaySequence());
        Debug.Log("Drum sequencer started.");
    }

    public void Stop()
    {
        if (!isPlaying)
        {
            Debug.LogWarning("Drum sequencer is not playing, no need to stop.");
            return;
        }

        isPlaying = false; // Set the playing state to false
        StopAllCoroutines(); // Stop the PlaySequence coroutine

        // Reset the grid visual state
        ResetAllColumns();

        currentStep = 0; // Optionally reset the current step to 0

        Debug.Log("Drum sequencer stopped.");
    }

    private IEnumerator PlaySequence()
    {
        while (isPlaying)
        {
            HighlightColumn(currentStep);
            PlayStep(currentStep);
            currentStep = (currentStep + 1) % drumGrid.drumColumns;
            yield return new WaitForSeconds(stepDuration);
        }
    }

    private void PlayStep(int step)
    {
        for (int row = 0; row < drumGrid.drumRows; row++)
        {
            CustomToggle toggle = drumGrid.GetToggleAt(row, step);
            if (toggle != null && toggle.GetState())
            {
                PlayDrumSample(row);
            }
        }
    }

    private void PlayDrumSample(int row)
    {
        if (drumSampler == null)
        {
            Debug.LogError("DrumSampler is not assigned!");
            return;
        }

        drumSampler.PlayDrum(row);
    }

    private void HighlightColumn(int columnIndex)
    {
        ResetAllColumns();
        for (int row = 0; row < drumGrid.drumRows; row++)
        {
            CustomToggle toggle = drumGrid.GetToggleAt(row, columnIndex);
            if (toggle != null)
            {
                toggle.SetDefaultOffColor(activeColumnColor);
            }
        }
        Debug.Log($"Highlighting column: {columnIndex}");
    }

    private void ResetAllColumns()
    {
        if (drumGrid == null) return;

        for (int row = 0; row < drumGrid.drumRows; row++)
        {
            for (int col = 0; col < drumGrid.drumColumns; col++)
            {
                CustomToggle toggle = drumGrid.GetToggleAt(row, col);
                if (toggle != null)
                {
                    toggle.SetDefaultOffColor(Color.white);
                }
            }
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

    public void LoadData()
    {
        DrumSequencerData data = SaveSystem.LoadDrum();

        if (data == null)
        {
            Debug.LogWarning("No saved data found. Loading default drum sequencer state.");
            return;
        }

        // Apply the loaded BPM
        if (data.bpm > 0) // Ensure valid BPM
        {
            bpm = data.bpm;
            UpdateStepDuration();

            if (bpmSlider != null)
            {
                bpmSlider.value = bpm; // Sync the slider with the loaded BPM
            }

            UpdateBpmText(); // Update BPM text UI
            Debug.Log($"Loaded BPM from save file: {bpm}");
        }

        // Apply the loaded grid state
        if (!string.IsNullOrEmpty(data.gridState))
        {
            int index = 0;
            for (int row = 0; row < drumGrid.drumRows; row++)
            {
                for (int col = 0; col < drumGrid.drumColumns; col++)
                {
                    if (index < data.gridState.Length)
                    {
                        CustomToggle toggle = drumGrid.GetToggleAt(row, col);
                        if (toggle != null)
                        {
                            toggle.SetState(data.gridState[index] == '1'); // Restore toggle state
                        }
                        index++;
                    }
                }
            }
            Debug.Log("Drum grid state loaded from save file.");
        }

        // Apply the saved scroll position
        if (drumGrid.drumScrollRect != null)
        {
            drumGrid.drumScrollRect.verticalScrollbar.value = data.scrollPosition;
            Debug.Log($"Scroll position loaded: {data.scrollPosition}");
        }
    }

}
