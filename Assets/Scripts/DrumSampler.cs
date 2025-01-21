using UnityEngine;

public class DrumSampler : MonoBehaviour
{
    [Header("Drum Samples")]
    public AudioClip[] drumSamples; // Array to input pre-recorded drum sounds
    private AudioSource[] audioSources; // Array of AudioSources corresponding to each drum sound

    [Header("Playback Settings")]
    public float volume = 1.0f; // Global volume control for all drum sounds

    private void Awake()
    {
        // Initialize audio sources
        if (drumSamples == null || drumSamples.Length == 0)
        {
            Debug.LogError("No drum samples assigned to the DrumSampler.");
            return;
        }

        audioSources = new AudioSource[drumSamples.Length];

        for (int i = 0; i < drumSamples.Length; i++)
        {
            GameObject audioSourceObject = new GameObject($"DrumAudioSource_{i}"); // Create a new GameObject for each AudioSource
            audioSourceObject.transform.SetParent(this.transform); // Set this object as the parent
            AudioSource source = audioSourceObject.AddComponent<AudioSource>(); // Add an AudioSource component

            source.clip = drumSamples[i]; // Assign the respective drum sample
            source.playOnAwake = false; // Disable play on awake
            source.volume = volume; // Set the global volume
            audioSources[i] = source;
        }
    }

    /// <summary>
    /// Plays a drum sound based on the drum index.
    /// </summary>
    /// <param name="drumIndex">Index of the drum sample to play.</param>
    public void PlayDrum(int drumIndex)
    {
        if (drumIndex < 0 || drumIndex >= audioSources.Length)
        {
            Debug.LogWarning($"Invalid drum index {drumIndex}. Check your drumSamples array.");
            return;
        }

        audioSources[drumIndex].Play();
        Debug.Log($"Playing drum sound {drumIndex}: {audioSources[drumIndex].clip.name}");
    }
}
