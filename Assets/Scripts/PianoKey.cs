using UnityEngine;
using UnityEngine.EventSystems;

public class PianoKey : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public string synthesizerTag = "Synthesizer"; // Tag to find the Synthesizer
    public int keyIndex;                         // Index of the key, starting from C1 (C1 = 0, C#1 = 1, ..., B7 = 87)
    private Synthesizer synthesizer;             // Reference to the Synthesizer
    public float frequency;                     // Frequency of the note (calculated dynamically)

    private void Start()
    {
        // Find the synthesizer by tag
        GameObject synthObject = GameObject.FindGameObjectWithTag(synthesizerTag);
        if (synthObject != null)
        {
            synthesizer = synthObject.GetComponent<Synthesizer>();
        }

        if (synthesizer == null)
        {
            Debug.LogError("Synthesizer not found! Ensure the Synthesizer has the correct tag.");
        }

        // Calculate frequency based on keyIndex
        frequency = CalculateFrequency(keyIndex);
    }

    /// <summary>
    /// Calculate the frequency of a key based on its index.
    /// </summary>
    public float CalculateFrequency(int keyIndex)
    {
        float baseFrequency = 32.703f; // Frequency of C1
        return baseFrequency * Mathf.Pow(2f, keyIndex / 12f);
    }

    /// <summary>
    /// Called when the user presses the key.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (synthesizer != null)
        {
            synthesizer.PlayNote(frequency); // Start playing the note
        }
    }

    /// <summary>
    /// Called when the user releases the key.
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (synthesizer != null)
        {
            synthesizer.StopAllNotes(); // Stop playing the note
        }
    }
}
