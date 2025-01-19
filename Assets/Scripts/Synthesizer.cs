using UnityEngine;
using System.Collections.Generic;

public class Synthesizer : MonoBehaviour
{
    public float masterVolume = 0.5f; // Overall volume
    public float osc1Volume = 0.7f;   // Volume of oscillator 1
    public float osc2Volume = 0.3f;   // Volume of oscillator 2
    public float detune = 0.05f;      // Detune amount for the second oscillator
    public float sampleRate = 44100f; // Audio sample rate

    public float distortionAmount = 2.0f; // Amount of distortion
    public float filterCutoff = 0.9f;     // Low-pass filter cutoff (0 to 1)
    public float filterResonance = 0.5f;  // Resonance for the low-pass filter

    private HashSet<float> activeFrequencies = new HashSet<float>(); // Store active frequencies
    private Dictionary<float, float[]> phases = new Dictionary<float, float[]>(); // Store phases for each active frequency
    private float previousSample = 0f; // For low-pass filtering

    /// <summary>
    /// Start playing a note at a specific frequency.
    /// </summary>
    /// <param name="freq">Frequency of the note to play.</param>
    public void PlayNote(float freq)
    {
        if (!activeFrequencies.Contains(freq))
        {
            activeFrequencies.Add(freq);
            phases[freq] = new float[2]; // Initialize phase for osc1 and osc2
        }
    }

    /// <summary>
    /// Stop playing the note.
    /// </summary>
    public void StopNote(float freq)
    {
        if (activeFrequencies.Contains(freq))
        {
            activeFrequencies.Remove(freq);
            phases.Remove(freq);
        }
    }

    /// <summary>
    /// Stop all notes.
    /// </summary>
    public void StopAllNotes()
    {
        activeFrequencies.Clear();
        phases.Clear();
    }

    /// <summary>
    /// Generate audio dynamically using OnAudioFilterRead.
    /// </summary>
    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (activeFrequencies.Count == 0) return;

        for (int i = 0; i < data.Length; i += channels)
        {
            float sample = 0f;

            foreach (float freq in activeFrequencies)
            {
                // Retrieve or initialize phases for the frequency
                float[] currentPhases = phases[freq];

                // Generate sawtooth waves for both oscillators
                float osc1 = Mathf.PingPong(currentPhases[0], 1f) * 2f - 1f;
                float osc2 = Mathf.PingPong(currentPhases[1], 1f) * 2f - 1f;

                // Combine oscillators and apply volume
                sample += (osc1 * osc1Volume + osc2 * osc2Volume) * masterVolume;

                // Increment phases
                currentPhases[0] += freq / sampleRate;
                currentPhases[1] += freq * (1f + detune) / sampleRate;

                // Wrap phases to avoid overflow
                if (currentPhases[0] > 1f) currentPhases[0] -= 1f;
                if (currentPhases[1] > 1f) currentPhases[1] -= 1f;
            }

            // Apply distortion
            sample = ApplyDistortion(sample, distortionAmount);

            // Apply low-pass filtering
            sample = LowPassFilter(sample, filterCutoff);

            // Write the sample to all channels (stereo)
            for (int j = 0; j < channels; j++)
            {
                data[i + j] = Mathf.Clamp(sample, -1f, 1f); // Clamp to prevent clipping
            }
        }
    }

    /// <summary>
    /// Applies distortion to a sample.
    /// </summary>
    private float ApplyDistortion(float sample, float amount)
    {
        return Mathf.Tan(sample * amount); // Improved tanh distortion
    }

    /// <summary>
    /// Applies a simple low-pass filter to smooth the sound.
    /// </summary>
    private float LowPassFilter(float sample, float cutoff)
    {
        float smoothedSample = previousSample + (sample - previousSample) * cutoff;
        previousSample = smoothedSample;
        return smoothedSample;
    }

    /// <summary>
    /// Sets a preset for a distorted saw wave sound.
    /// </summary>
    public void DistortedSawPreset()
    {
        masterVolume = 0.8f;
        osc1Volume = 0.7f;
        osc2Volume = 0.5f;
        detune = 0.02f;
        distortionAmount = 2.5f;
        filterCutoff = 0.7f;
        filterResonance = 0.5f;

        Debug.Log("Distorted Saw preset loaded!");
    }
}
