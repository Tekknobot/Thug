using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro; // Import TextMeshPro namespace

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

        StartCoroutine(LoadDrumSequencerState());
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

    public void UpdateStepDuration()
    {
        stepDuration = 60f / (bpm * 4);
        Debug.Log($"Step duration updated to {stepDuration} seconds per step at {bpm} BPM.");
    }

    private void UpdateBpm(float newBpm)
    {
        bpm = newBpm;
        UpdateStepDuration();
        UpdateBpmText();
        Debug.Log($"BPM updated to {bpm}.");
    }

    private void UpdateBpmText()
    {
        if (bpmText != null)
        {
            bpmText.text = bpm.ToString("F0"); // Display BPM as a whole number
        }
    }

    public void SaveDrumSequencerState()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Cannot save state: Drum grid is not initialized.");
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

        SaveSystem.SaveDrum(data);
        Debug.Log("Drum sequencer state saved.");
    }

    public IEnumerator LoadDrumSequencerState()
    {
        // Ensure grid initialization is complete
        while (!isInitialized)
        {
            yield return null;
        }

       DrumSequencerData data = SaveSystem.LoadDrum();

        if (data == null)
        {
            Debug.LogWarning("No saved data found. Loading default drum sequencer state.");
            yield break;
        }

        bpm = data.bpm;
        UpdateStepDuration();

        if (bpmSlider != null)
        {
            bpmSlider.value = bpm;
        }

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
            Debug.Log("Drum grid state loaded from save file.");
        }

        if (drumGrid.drumScrollRect != null)
        {
            drumGrid.drumScrollRect.verticalScrollbar.value = data.scrollPosition;
            Debug.Log($"Scroll position loaded: {data.scrollPosition}");
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveDrumSequencerState();
        }
    }

    private void OnApplicationQuit()
    {
        SaveDrumSequencerState();
    }
}
