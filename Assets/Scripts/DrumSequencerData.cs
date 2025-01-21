using System;

[System.Serializable]
public class DrumSequencerData
{
    public float bpm;           // Stores the BPM value
    public string gridState;    // Stores the serialized grid state (e.g., a string of "1" and "0")
    public float scrollPosition; // Stores the scroll position of the sequencer grid

    // Constructor for easy initialization
    public DrumSequencerData(float bpm, string gridState, float scrollPosition)
    {
        this.bpm = bpm;
        this.gridState = gridState;
        this.scrollPosition = scrollPosition;
    }
}
