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
    public void DistortedSaw()
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


    /// <summary>
    /// Sets a preset for a French house bass sound.
    /// </summary>
    public void FrenchHouseBass()
    {
        masterVolume = 0.85f;
        osc1Volume = 0.8f;
        osc2Volume = 0.4f;
        detune = 0.01f; // Subtle detune for depth
        distortionAmount = 1.5f; // Soft distortion for warmth
        filterCutoff = 0.3f; // Low cutoff for a deep, rounded bass sound
        filterResonance = 0.6f; // Moderate resonance for a slightly punchy tone

        Debug.Log("French House Bass preset loaded!");
    }

    /// <summary>
    /// Sets a preset for a Techno bass sound.
    /// </summary>
    public void TechnoBass()
    {
        masterVolume = 0.9f;
        osc1Volume = 0.7f;
        osc2Volume = 0.5f;
        detune = 0.03f; // More detune for a gritty sound
        distortionAmount = 3.0f; // Heavy distortion for an aggressive tone
        filterCutoff = 0.4f; // Medium-low cutoff for a dark, driving bass
        filterResonance = 0.4f; // Low resonance to keep the sound focused

        Debug.Log("Techno Bass preset loaded!");
    }


    /// <summary>
    /// Sets a preset for a crunchy square wave sound optimized for 16th notes.
    /// </summary>
    public void CrunchySquare()
    {
        masterVolume = 0.7f;
        osc1Volume = 0.6f;
        osc2Volume = 0.4f;
        detune = 0.01f; // Slight detune for subtle phasing
        distortionAmount = 1.8f; // Moderate distortion for a crunchy effect
        filterCutoff = 0.5f; // Lower cutoff for a warmer sound
        filterResonance = 0.7f; // Higher resonance for a squelchy character

        Debug.Log("Crunchy Square preset loaded!");
    }

    /// <summary>
    /// Sets a preset for a deep sub-bass sound.
    /// </summary>
    public void DeepSubBass()
    {
        masterVolume = 0.6f;
        osc1Volume = 0.5f;
        osc2Volume = 0.2f;
        detune = 0f; // No detune for a pure tone
        distortionAmount = 1.0f; // Minimal distortion
        filterCutoff = 0.2f; // Very low cutoff for sub frequencies
        filterResonance = 0.3f; // Minimal resonance to keep it smooth

        Debug.Log("Deep Sub-Bass preset loaded!");
    }


    /// <summary>
    /// Sets a preset for a plucky lead sound.
    /// </summary>
    public void PluckyLead()
    {
        masterVolume = 0.8f;          // Balanced volume
        osc1Volume = 0.6f;           // Main oscillator volume
        osc2Volume = 0.4f;           // Secondary oscillator volume
        detune = 0.02f;              // Slight detune for richness
        distortionAmount = 1.2f;     // Mild distortion for brightness
        filterCutoff = 0.6f;         // Medium cutoff for clarity
        filterResonance = 0.8f;      // High resonance for plucky effect

        // Additional settings for a plucky envelope
        float attackTime = 0.01f;    // Fast attack
        float decayTime = 0.15f;     // Short decay
        float sustainLevel = 0.2f;   // Low sustain
        float releaseTime = 0.1f;    // Quick release

        // Apply envelope settings if implemented in the synthesizer
        SetEnvelope(attackTime, decayTime, sustainLevel, releaseTime);

        Debug.Log("Plucky Lead preset loaded!");
    }

    /// <summary>
    /// Example function for setting ADSR envelope settings.
    /// This should be implemented in your synthesizer if not already.
    /// </summary>
    private void SetEnvelope(float attack, float decay, float sustain, float release)
    {
        // Example implementation of envelope parameters
        // This would depend on your Synthesizer's internal handling of envelopes
        Debug.Log($"Envelope Set - Attack: {attack}, Decay: {decay}, Sustain: {sustain}, Release: {release}");
    }

}
