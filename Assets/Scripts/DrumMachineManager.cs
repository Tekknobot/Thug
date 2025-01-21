using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DrumMachineManager : MonoBehaviour
{
    public ScrollRect drumScrollRect;             // ScrollRect for the drum grid
    public GameObject drumStepPrefab;            // Prefab for individual drum step toggles
    public RectTransform drumContent;            // Public object for the ScrollRect content
    public int drumRows = 8;                     // Number of drum tracks (rows)
    public int drumColumns = 16;                 // Number of steps per track
    public float cellSize = 50f;                 // Size of each step cell
    public float cellSpacing = 5f;              // Spacing between cells

    // Updated drum track names
    public string[] drumTrackNames = { "Kick", "Snare", "Hi-Hat", "Open-Hat", "Clap", "Ride", "Rim", "Crash" };

    private RectTransform drumPanel;

    void Start()
    {
        // Ensure drumTrackNames matches the number of rows
        if (drumTrackNames.Length != drumRows)
        {
            Debug.LogError($"The number of drumTrackNames ({drumTrackNames.Length}) does not match drumRows ({drumRows}). Adjust the drumRows or drumTrackNames array.");
            return;
        }

        // Ensure drumContent is assigned
        if (drumContent == null)
        {
            Debug.LogError("Drum content (ScrollRect content) is not assigned in the Inspector!");
            return;
        }

        // Link the ScrollRect content
        drumScrollRect.content = drumContent;

        CreateDrumGrid();
        AdjustScrollRectContentSize(500, 150);
    }

    void CreateDrumGrid()
    {
        Debug.Log("Creating drum grid...");

        if (drumContent == null)
        {
            Debug.LogError("Drum content (ScrollRect content) is null! Assign it in the Inspector.");
            return;
        }

        // Create the DrumPanel (parent for grid elements)
        drumPanel = new GameObject("DrumPanel", typeof(RectTransform)).GetComponent<RectTransform>();
        drumPanel.SetParent(drumContent);
        drumPanel.anchorMin = new Vector2(0, 1);
        drumPanel.anchorMax = new Vector2(0, 1);
        drumPanel.pivot = new Vector2(0, 1); // Top-left pivot
        drumPanel.anchoredPosition = Vector2.zero;

        Debug.Log("Populating drum grid...");
        PopulateDrumGrid();
    }

    void PopulateDrumGrid()
    {
        for (int row = 0; row < drumRows; row++)
        {
            string trackName = drumTrackNames[row];

            for (int col = 0; col < drumColumns; col++)
            {
                GameObject step = Instantiate(drumStepPrefab, drumPanel);
                RectTransform stepRect = step.GetComponent<RectTransform>();
                stepRect.sizeDelta = new Vector2(cellSize, cellSize);
                stepRect.anchorMin = new Vector2(0, 1);
                stepRect.anchorMax = new Vector2(0, 1);
                stepRect.pivot = new Vector2(0, 1);
                stepRect.anchoredPosition = new Vector2(col * (cellSize + cellSpacing), -row * (cellSize + cellSpacing));

                CustomToggle customToggle = step.GetComponent<CustomToggle>();
                if (customToggle != null)
                {
                    customToggle.Initialize(row, false); // Initialize with the row index
                }

                TMP_Text textComponent = step.GetComponentInChildren<TMP_Text>();
                if (textComponent != null && col == 0) // Only add text for the first column
                {
                    textComponent.text = trackName;
                }
            }
        }
    }

    public void AdjustScrollRectContentSize(float? manualWidth = null, float? manualHeight = null)
    {
        // Calculate default width and height if not manually set
        float width = manualWidth ?? drumColumns * (cellSize + cellSpacing) - cellSpacing;
        float height = manualHeight ?? drumRows * (cellSize + cellSpacing) - cellSpacing;

        // Update the size of the content RectTransform
        drumContent.sizeDelta = new Vector2(width, height);
        drumContent.anchorMin = new Vector2(0, 1); // Top-left corner
        drumContent.anchorMax = new Vector2(0, 1);
        drumContent.pivot = new Vector2(0, 1);     // Top-left pivot
        drumContent.anchoredPosition = Vector2.zero;

        // Force update of the layout to ensure the ScrollRect updates
        Canvas.ForceUpdateCanvases();

        // Update the ScrollRect to make scrollbars visible if needed
        ScrollRect scrollRect = drumContent.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

            // Refresh ScrollRect bounds
            scrollRect.SetLayoutHorizontal();
            scrollRect.SetLayoutVertical();
        }

        Debug.Log($"Drum grid content size adjusted: Width={width}, Height={height}");
    }


    /// <summary>
    /// Retrieve the toggle at the specified drum row and step.
    /// </summary>
    public CustomToggle GetToggleAt(int row, int step)
    {
        int childIndex = row * drumColumns + step;
        if (childIndex >= 0 && childIndex < drumPanel.childCount)
        {
            return drumPanel.GetChild(childIndex).GetComponent<CustomToggle>();
        }
        return null;
    }
}
