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
    public float cellSize = 50f;           // Base size of each cell (adjusted by CanvasScaler)
    public float cellSpacing = 5f;         // Spacing between cells (adjusted by CanvasScaler)
    public float pianoKeyWidth = 70f;      // Base width of piano keys (adjusted by CanvasScaler)
    public string[] pitchNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

    private RectTransform pianoPanel;
    private RectTransform stepPanel;
    public Color blackKeyRowColor = Color.gray;  // Color for rows aligned with black keys
    public Color whiteKeyRowColor = Color.white; // Color for rows aligned with white keys
    public CanvasScaler canvasScaler;            // Reference to CanvasScaler for dynamic scaling

    void Start()
    {
        // Ensure CanvasScaler is assigned
        if (canvasScaler == null)
        {
            canvasScaler = FindObjectOfType<CanvasScaler>();
        }

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

        // Ensure scroll listeners are properly registered
        stepScrollRect.onValueChanged.AddListener(OnStepScrollChanged);
        pianoScrollRect.onValueChanged.AddListener(OnPianoScrollChanged);

        // Adjust content sizes to synchronize them
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

        // Create the StepPanel (linked to scrollRectContent)
        stepPanel = new GameObject("StepPanel", typeof(RectTransform)).GetComponent<RectTransform>();
        stepPanel.SetParent(scrollRectContent);
        stepPanel.anchorMin = new Vector2(0, 1);
        stepPanel.anchorMax = new Vector2(0, 1);
        stepPanel.pivot = new Vector2(0, 1); // Top-left pivot
        stepPanel.anchoredPosition = Vector2.zero; // Align with top-left corner
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

            // Position vertically without spacing
            keyRect.anchoredPosition = new Vector2(0, -i * cellSize); 

            TMP_Text textComponent = key.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = $"{pitchNames[pitchIndex]}{octave}"; // E.g., "B3", "A#3", etc.
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

                // Position without spacing
                stepRect.anchoredPosition = new Vector2(j * cellSize, -i * cellSize); 

                // Initialize the toggle
                CustomToggle customToggle = step.GetComponent<CustomToggle>();
                if (customToggle != null)
                {
                    customToggle.Initialize(reverseIndex, isBlackKey); // Set row index and black key status
                }

                // Add pitch name to the step toggle, if needed
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
        // Calculate content height based on rows and cell size
        float contentHeight = rows * cellSize;

        // Calculate content width for the step panel
        float contentWidth = columns * cellSize;

        // Set the same height for both panels
        scrollRectContent.sizeDelta = new Vector2(200, 1500);
        pianoScrollRectContent.sizeDelta = new Vector2(pianoKeyWidth, 1500);

        // Force recalculation
        Canvas.ForceUpdateCanvases();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRectContent);
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(pianoScrollRectContent);

        Debug.Log($"ScrollRectContent: Width={scrollRectContent.sizeDelta.x}, Height={scrollRectContent.sizeDelta.y}");
        Debug.Log($"PianoScrollRectContent: Width={pianoScrollRectContent.sizeDelta.x}, Height={pianoScrollRectContent.sizeDelta.y}");
    }

    private bool isSyncingScroll = false;

    private void OnStepScrollChanged(Vector2 scrollPosition)
    {
        if (isSyncingScroll) return; // Prevent feedback loop

        isSyncingScroll = true;

        // Sync the vertical scrolling of the piano panel with the step panel
        pianoScrollRect.normalizedPosition = new Vector2(
            pianoScrollRect.normalizedPosition.x, 
            scrollPosition.y
        );

        isSyncingScroll = false;

        Debug.Log($"Step Scroll Position: {scrollPosition.y}, Piano Scroll Synchronized");
    }

    private void OnPianoScrollChanged(Vector2 scrollPosition)
    {
        if (isSyncingScroll) return; // Prevent feedback loop

        isSyncingScroll = true;

        // Sync the vertical scrolling of the step panel with the piano panel
        stepScrollRect.normalizedPosition = new Vector2(
            stepScrollRect.normalizedPosition.x, 
            scrollPosition.y
        );

        isSyncingScroll = false;

        Debug.Log($"Piano Scroll Position: {scrollPosition.y}, Step Scroll Synchronized");
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
