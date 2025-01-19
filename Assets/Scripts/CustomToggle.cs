using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomToggle : MonoBehaviour, IPointerClickHandler, IDragHandler
{
    public Color onColor = Color.grey;            // Color when toggled ON
    public Color blackKeyOffColor = Color.gray;   // Default color for rows corresponding to black keys
    public Color whiteKeyOffColor = Color.white;  // Default color for rows corresponding to white keys
    private Color currentOffColor;                // Stores the appropriate off color for this toggle
    private bool isOn = false;                    // Current state of the toggle
    private Image background;                     // Image component to change color

    void Awake()
    {
        background = GetComponent<Image>();
    }

    /// <summary>
    /// Assign the row index and set the default off color based on the row type (black or white key).
    /// </summary>
    /// <param name="rowIndex">The row index of the toggle.</param>
    /// <param name="isBlackKey">Whether the toggle belongs to a black key row.</param>
    public void Initialize(int rowIndex, bool isBlackKey)
    {
        currentOffColor = isBlackKey ? blackKeyOffColor : whiteKeyOffColor; // Determine the color
        SetDefaultOffColor(currentOffColor);
    }

    /// <summary>
    /// Set the default off color for the toggle.
    /// </summary>
    /// <param name="color">The default color for the toggle when OFF</param>
    public void SetDefaultOffColor(Color color)
    {
        currentOffColor = color; // Update the default off color
        if (!isOn && background != null)
        {
            Debug.Log($"Setting color to {color} for {name}");
            background.color = currentOffColor; // Set the initial color
        }
    }


    /// <summary>
    /// Toggle the state when clicked.
    /// </summary>
    /// <param name="eventData">Pointer click event data</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleState();
    }

    /// <summary>
    /// Detect dragging over the toggle (optional for drag behavior).
    /// </summary>
    /// <param name="eventData">Drag event data</param>
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log($"Dragging on {name} at position {eventData.position}");
        // Additional drag-specific logic can be added here if needed
    }

    /// <summary>
    /// Toggles the state and updates the color.
    /// </summary>
    private void ToggleState()
    {
        isOn = !isOn; // Flip the state
        if (background != null)
        {
            background.color = isOn ? onColor : currentOffColor; // Update to ON or OFF color
        }

        Debug.Log($"{name} is now {(isOn ? "ON" : "OFF")}");
    }

    /// <summary>
    /// Manually set the toggle state (useful for external control).
    /// </summary>
    /// <param name="state">Desired toggle state</param>
    public void SetState(bool state)
    {
        isOn = state;
        if (background != null)
        {
            background.color = isOn ? onColor : currentOffColor; // Update to ON or OFF color
        }
    }

    /// <summary>
    /// Returns the current toggle state.
    /// </summary>
    /// <returns>True if ON, False if OFF</returns>
    public bool GetState()
    {
        return isOn;
    }
}
