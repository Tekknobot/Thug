using UnityEngine;
using UnityEngine.UI;

public class AbletonStyleControls : MonoBehaviour
{
    public SequencerController sequencerController; // Reference to SequencerController
    public Canvas canvas; // Reference to the Canvas
    public Sprite playIcon; // Icon for the Play button
    public Sprite stopIcon; // Icon for the Stop (Play toggled) button

    public Vector2 buttonSize = new Vector2(30, 30); // Size of each button
    public float buttonSpacing = 10f; // Spacing between buttons
    public Vector2 groupPosition = new Vector2(0, 0); // Position of the button group

    void Start()
    {
        // Ensure a canvas exists
        if (canvas == null)
        {
            CreateCanvas();
        }

        // Create the button group
        GameObject buttonGroup = CreateButtonGroup();

        // Create and configure the Play/Stop button
        CreatePlayButton(buttonGroup.transform);
    }

    /// <summary>
    /// Creates a Canvas if none exists.
    /// </summary>
    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Configure CanvasScaler
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
    }

    /// <summary>
    /// Creates a group for the buttons.
    /// </summary>
    /// <returns>The GameObject representing the button group.</returns>
    private GameObject CreateButtonGroup()
    {
        GameObject buttonGroupObject = new GameObject("ButtonGroup", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        buttonGroupObject.transform.SetParent(canvas.transform);

        // Configure RectTransform
        RectTransform rectTransform = buttonGroupObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(buttonSize.x, buttonSize.y);
        rectTransform.anchoredPosition = groupPosition;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f); // Center anchor
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

        // Configure HorizontalLayoutGroup
        HorizontalLayoutGroup layoutGroup = buttonGroupObject.GetComponent<HorizontalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = buttonSpacing;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;

        return buttonGroupObject;
    }

    /// <summary>
    /// Creates the Play/Stop toggle button and attaches the PlayButton script.
    /// </summary>
    /// <param name="parent">Parent transform.</param>
    private void CreatePlayButton(Transform parent)
    {
        GameObject playButtonObject = new GameObject("PlayButton", typeof(RectTransform), typeof(Button), typeof(Image));
        playButtonObject.transform.SetParent(parent);

        // Configure RectTransform
        RectTransform rectTransform = playButtonObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = buttonSize;

        // Configure Button
        Button playButton = playButtonObject.GetComponent<Button>();

        // Configure Image
        Image image = playButtonObject.GetComponent<Image>();
        image.sprite = playIcon; // Initial icon
        image.type = Image.Type.Simple;
        image.color = Color.white; // Default button color

        // Attach the PlayButton script dynamically
        PlayButton playButtonScript = playButtonObject.AddComponent<PlayButton>();
        playButtonScript.sequencerController = sequencerController;
        playButtonScript.playIcon = playIcon;
        playButtonScript.stopIcon = stopIcon;
    }
}
