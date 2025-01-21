// DrumMachineManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DrumMachineManager : MonoBehaviour
{
    public ScrollableGrid drumGrid;          // Grid for the drum machine (steps and rows)
    public ScrollRect drumScrollRect;       // ScrollRect for the drum grid (horizontal scrolling)
    public ScrollRect pianoRollScrollRectPrefab;

    public GameObject drumStepPrefab;       // Prefab for individual drum step toggles
    public int drumRows = 5;                // Number of drum tracks (rows)
    public int drumColumns = 16;            // Number of steps per track
    public float cellSize = 50f;            // Size of each step cell
    public float cellSpacing = 5f;          // Spacing between cells
    public string[] drumTrackNames = { "Kick", "Snare", "Hi-Hat", "Clap", "Percussion" };

    private RectTransform drumPanel;

    void Start()
    {
        CreateDrumGrid();
        CreatePianoRoll();
    }

    void CreateDrumGrid()
    {
        // Create the DrumPanel (linked to drumScrollRect content)
        drumPanel = new GameObject("DrumPanel", typeof(RectTransform)).GetComponent<RectTransform>();
        drumPanel.SetParent(drumScrollRect.content);
        drumPanel.anchorMin = new Vector2(0, 1);
        drumPanel.anchorMax = new Vector2(0, 1);
        drumPanel.pivot = new Vector2(0, 1); // Top-left pivot
        drumPanel.anchoredPosition = Vector2.zero;

        PopulateDrumGrid();
        AdjustScrollRectContentSize();
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

    void AdjustScrollRectContentSize()
    {
        drumScrollRect.content.sizeDelta = new Vector2(200, 200);
    }

    /// <summary>
    /// Retrieve the toggle at the specified drum row and step.
    /// </summary>
    /// <param name="row">Row index (track).</param>
    /// <param name="step">Step index.</param>
    /// <returns>CustomToggle if it exists; otherwise, null.</returns>
    public CustomToggle GetToggleAt(int row, int step)
    {
        int childIndex = row * drumColumns + step;
        if (childIndex >= 0 && childIndex < drumPanel.childCount)
        {
            return drumPanel.GetChild(childIndex).GetComponent<CustomToggle>();
        }
        return null;
    }

    void CreatePianoRoll()
    {
        ScrollRect pianoRollScrollRect = Instantiate(pianoRollScrollRectPrefab, transform);
        RectTransform pianoRollRect = pianoRollScrollRect.content;

        pianoRollRect.sizeDelta = new Vector2(cellSize * 2, drumRows * (cellSize + cellSpacing));
        pianoRollRect.anchoredPosition = Vector2.zero;

        for (int i = 0; i < drumRows; i++)
        {
            GameObject pianoKey = new GameObject(drumTrackNames[i], typeof(RectTransform), typeof(Button));
            RectTransform keyRect = pianoKey.GetComponent<RectTransform>();
            keyRect.SetParent(pianoRollRect);
            keyRect.anchorMin = new Vector2(0, 1);
            keyRect.anchorMax = new Vector2(1, 1);
            keyRect.pivot = new Vector2(0.5f, 1);
            keyRect.sizeDelta = new Vector2(0, cellSize);
            keyRect.anchoredPosition = new Vector2(0, -i * (cellSize + cellSpacing));

            Button keyButton = pianoKey.GetComponent<Button>();
            keyButton.onClick.AddListener(() => PlayDrumSound(i));

            TMP_Text keyText = new GameObject("KeyLabel", typeof(RectTransform), typeof(TMP_Text)).GetComponent<TMP_Text>();
            keyText.transform.SetParent(pianoKey.transform);
            keyText.text = drumTrackNames[i];
            keyText.fontSize = 18;
            keyText.alignment = TextAlignmentOptions.Center;
            RectTransform textRect = keyText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
        }
    }


    void PlayDrumSound(int trackIndex)
    {
        Debug.Log($"Playing sound for track: {drumTrackNames[trackIndex]}");
        // Add sound playback logic here (e.g., AudioSource.PlayOneShot)
    }
}
