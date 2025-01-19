using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScrollableGrid : MonoBehaviour
{
    public ScrollRect stepScrollRect;       // ScrollRect for the StepPanel (visible scrollbar)
    public ScrollRect pianoScrollRect;     // ScrollRect for the PianoPanel (invisible scrollbar)
    public RectTransform scrollRectContent; // Content of the ScrollRect for the StepPanel (horizontal scrolling)
    public RectTransform pianoScrollRectContent; // Content of the ScrollRect for the Piano Roll (vertical scrolling)
    public GameObject pianoKeyPrefab;      // Prefab for piano roll keys
    public GameObject stepTogglePrefab;    // Prefab for step toggles
    public int rows = 36;                  // Number of rows (pitches - 3 octaves)
    public int columns = 16;               // Number of steps
    public float cellSize = 50f;           // Size of each cell (adjustable in Inspector)
    public float cellSpacing = 5f;         // Spacing between cells (adjustable in Inspector)
    public float pianoKeyWidth = 70f;      // Width of piano keys (adjustable in Inspector)
    public string[] pitchNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

    private RectTransform pianoPanel;
    private RectTransform stepPanel;
    public Color blackKeyRowColor = Color.gray;  // Color for rows aligned with black keys
    public Color whiteKeyRowColor = Color.white; // Color for rows aligned with white keys
    public SequencerController sequencerController;

    void Start()
    {
        // Disable the piano's vertical scrollbar visibility but keep functionality
        pianoScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        if (pianoScrollRect.verticalScrollbar != null)
        {
            pianoScrollRect.verticalScrollbar.gameObject.SetActive(false); // Hide the scrollbar
        }

        // Link scrolling of the two scroll rects
        stepScrollRect.onValueChanged.AddListener(OnStepScrollChanged);

        CreatePanels();
        PopulatePianoPanel();
        PopulateStepPanel();
        AdjustScrollRectContentSizes();
    }

    void CreatePanels()
    {
        // Create the PianoPanel (linked to pianoScrollRectContent)
        pianoPanel = new GameObject("PianoPanel", typeof(RectTransform)).GetComponent<RectTransform>();
        pianoPanel.SetParent(pianoScrollRectContent);
        pianoPanel.anchorMin = new Vector2(0, 1);
        pianoPanel.anchorMax = new Vector2(0, 1);
        pianoPanel.pivot = new Vector2(0, 1); // Top-left pivot
        pianoPanel.anchoredPosition = Vector2.zero; // Align with top-left corner
        pianoPanel.sizeDelta = new Vector2(pianoKeyWidth, cellSize * rows + (rows - 1) * cellSpacing);

        // Create the StepPanel (linked to scrollRectContent)
        stepPanel = new GameObject("StepPanel", typeof(RectTransform)).GetComponent<RectTransform>();
        stepPanel.SetParent(scrollRectContent);
        stepPanel.anchorMin = new Vector2(0, 1);
        stepPanel.anchorMax = new Vector2(0, 1);
        stepPanel.pivot = new Vector2(0, 1); // Top-left pivot
        stepPanel.anchoredPosition = Vector2.zero; // Align with top-left corner
        stepPanel.sizeDelta = new Vector2(cellSize * columns + (columns - 1) * cellSpacing, cellSize * rows + (rows - 1) * cellSpacing);
    }


    void PopulatePianoPanel()
    {
        int startOctave = 1; // Starting octave for the lowest pitch (C1)

        for (int i = 0; i < rows; i++) // Start from the highest pitch at the top
        {
            int reverseIndex = rows - 1 - i; // Reverse row order
            int pitchIndex = reverseIndex % 12; // Cycle through pitch names (C, C#, D, etc.)
            int octave = startOctave + (reverseIndex / 12); // Increment octave as we go up

            GameObject key = Instantiate(pianoKeyPrefab, pianoPanel);
            RectTransform keyRect = key.GetComponent<RectTransform>();
            keyRect.sizeDelta = new Vector2(pianoKeyWidth, cellSize); // Set size for piano keys
            keyRect.anchorMin = new Vector2(0, 1);
            keyRect.anchorMax = new Vector2(0, 1);
            keyRect.pivot = new Vector2(0, 1); // Align to top-left corner
            keyRect.anchoredPosition = new Vector2(0, -i * (cellSize + cellSpacing)); // Position vertically

            TMP_Text textComponent = key.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = $"{pitchNames[pitchIndex]}{octave}"; // E.g., "B3", "A#3", etc.
            }

            PianoKey pianoKey = key.GetComponent<PianoKey>();
            if (pianoKey != null)
            {
                // Calculate the key index and set the frequency
                int keyIndex = reverseIndex; // Start from 0 for C1 and increment
                pianoKey.frequency = pianoKey.CalculateFrequency(keyIndex);
                pianoKey.keyIndex = keyIndex;
                Debug.Log($"Key {pitchNames[pitchIndex]}{octave} assigned frequency {pianoKey.frequency} Hz");
            }

            Image keyImage = key.GetComponent<Image>();
            Button keyButton = key.GetComponent<Button>();
            if (keyImage != null && keyButton != null)
            {
                if (IsBlackKey(pitchIndex))
                {
                    // Change the Button's normal color to black
                    ColorBlock colors = keyButton.colors;
                    colors.normalColor = Color.black;
                    keyButton.colors = colors;

                    if (textComponent != null) textComponent.color = Color.white; // Text color for black keys
                }
                else
                {
                    keyImage.color = Color.white; // Keep white key background
                    if (textComponent != null) textComponent.color = Color.black; // Text color for white keys
                }
            }
        }
    }



    void PopulateStepPanel()
    {
        int startOctave = 1; // Starting octave for the lowest pitch (C1)

        for (int i = 0; i < rows; i++)
        {
            int reverseIndex = rows - 1 - i; // Reverse row order
            int pitchIndex = reverseIndex % 12; // Align with pitch names
            int octave = startOctave + (reverseIndex / 12); // Calculate current octave
            string pitchName = $"{pitchNames[pitchIndex]}{octave}"; // Generate pitch name for this row

            bool isBlackKey = IsBlackKey(pitchIndex); // Determine if it's a black key row

            for (int j = 0; j < columns; j++)
            {
                GameObject step = Instantiate(stepTogglePrefab, stepPanel);
                RectTransform stepRect = step.GetComponent<RectTransform>();
                stepRect.sizeDelta = new Vector2(cellSize, cellSize); // Fixed size for step toggles
                stepRect.anchorMin = new Vector2(0, 1);
                stepRect.anchorMax = new Vector2(0, 1);
                stepRect.pivot = new Vector2(0, 1); // Align to top-left corner
                stepRect.anchoredPosition = new Vector2(j * (cellSize + cellSpacing), -i * (cellSize + cellSpacing)); // Align in grid

                // Initialize the toggle with the appropriate row type
                CustomToggle customToggle = step.GetComponent<CustomToggle>();
                if (customToggle != null)
                {
                    customToggle.Initialize(reverseIndex, isBlackKey); // Set row index and whether it's a black key row
                }

                // Update the text on the step to show the pitch name
                TMP_Text textComponent = step.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = pitchName;
                }
            }
        }
    }

    void AdjustScrollRectContentSizes()
    {
        // Calculate the actual grid dimensions
        float stepContentWidth = (cellSize + cellSpacing) * columns - cellSpacing;
        float stepContentHeight = (cellSize + cellSpacing) * rows - cellSpacing;

        // Set the ScrollRectContent size manually
        scrollRectContent.sizeDelta = new Vector2(stepContentWidth, stepContentHeight);

        // Ensure the StepPanel also matches the content size
        stepPanel.sizeDelta = new Vector2(stepContentWidth, stepContentHeight);

        // Adjust PianoPanel to fit rows
        float pianoContentHeight = (cellSize + cellSpacing) * rows - cellSpacing;
        pianoScrollRectContent.sizeDelta = new Vector2(pianoKeyWidth, pianoContentHeight);

        // Force Unity to recalculate the layout
        Canvas.ForceUpdateCanvases();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRectContent);

        // Adjust the width of ScrollRectContent manually after it's built
        float manualWidthAdjustmentFactor = 0.35f; // Adjust this factor as needed (e.g., 0.8 for 80%)
        float adjustedWidth = stepContentWidth * manualWidthAdjustmentFactor;
        scrollRectContent.sizeDelta = new Vector2(adjustedWidth, stepContentHeight);

        // Debug output for verification
        Debug.Log($"ScrollRectContent (Adjusted): Width={scrollRectContent.sizeDelta.x}, Height={scrollRectContent.sizeDelta.y}");
    }

    private void OnStepScrollChanged(Vector2 scrollPosition)
    {
        // Sync the vertical scrolling of the piano panel with the step panel
        Vector2 pianoScrollPosition = pianoScrollRect.normalizedPosition;
        pianoScrollPosition.y = scrollPosition.y; // Match vertical scrolling
        pianoScrollRect.normalizedPosition = pianoScrollPosition;
    }

    public bool IsBlackKey(int pitchIndex)
    {
        // Black keys are C#, D#, F#, G#, A# (indices: 1, 3, 6, 8, 10)
        return pitchIndex == 1 || pitchIndex == 3 || pitchIndex == 6 || pitchIndex == 8 || pitchIndex == 10;
    }

    /// <summary>
    /// Retrieve the toggle at the specified row and step.
    /// </summary>
    /// <param name="row">Row index (pitch).</param>
    /// <param name="step">Step index.</param>
    /// <returns>CustomToggle if it exists; otherwise, null.</returns>
    public CustomToggle GetToggleAt(int row, int step)
    {
        int childIndex = row * columns + step;
        if (childIndex >= 0 && childIndex < stepPanel.childCount)
        {
            return stepPanel.GetChild(childIndex).GetComponent<CustomToggle>();
        }
        return null;
    }

}
