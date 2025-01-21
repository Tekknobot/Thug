using UnityEngine;

public class DrumSynthesizer : MonoBehaviour
{
    public float masterVolume = 0.5f;       // Overall volume
    public float sampleRate = 44100f;      // Audio sample rate

    private float phase;                   // Phase for waveform generation
    private float noisePhase;              // Phase for noise modulation
    private bool isPlaying = false;        // If a sound is currently being played
    private float envelopeTime;            // Time elapsed for the envelope
    private float envelopeDuration = 0.0f; // Duration of the envelope

    // Envelope parameters
    private float attack = 0.01f;
    private float decay = 0.2f;
    private float sustain = 0.3f;
    private float release = 0.1f;

    // Active sound parameters
    private float frequency = 0f;          // Frequency of the sound
    private float noiseAmount = 0f;        // Amount of noise
    private string waveform = "sine";      // Waveform type: "sine", "square", "triangle"
    private float currentVolume = 0f;      // Current volume

    /// <summary>
    /// Play a drum sound with specific parameters.
    /// </summary>
    public void PlayDrum(float freq, string wave, float noise, float attackTime, float decayTime, float sustainLevel, float releaseTime)
    {
        frequency = freq;
        waveform = wave;
        noiseAmount = noise;

        // Set envelope parameters
        attack = attackTime;
        decay = decayTime;
        sustain = sustainLevel;
        release = releaseTime;

        envelopeTime = 0f;         // Reset envelope
        envelopeDuration = attack + decay + release;
        isPlaying = true;
    }

    /// <summary>
    /// Generate audio dynamically using OnAudioFilterRead.
    /// </summary>
    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isPlaying) return;

        for (int i = 0; i < data.Length; i += channels)
        {
            // Generate waveform
            float sample = GenerateWaveform(waveform, frequency);

            // Add noise
            sample += Random.Range(-1f, 1f) * noiseAmount;

            // Apply envelope
            sample *= ApplyEnvelope();

            // Scale with master volume
            sample *= masterVolume;

            // Write to all channels
            for (int j = 0; j < channels; j++)
            {
                data[i + j] = Mathf.Clamp(sample, -1f, 1f);
            }

            // Increment phase
            phase += frequency / sampleRate;
            if (phase > 1f) phase -= 1f;

            // Stop playback if envelope is done
            if (envelopeTime > envelopeDuration)
            {
                isPlaying = false;
                break;
            }
        }
    }

    /// <summary>
    /// Generate a waveform based on the type.
    /// </summary>
    private float GenerateWaveform(string type, float freq)
    {
        switch (type.ToLower())
        {
            case "sine":
                return Mathf.Sin(2f * Mathf.PI * phase);
            case "square":
                return Mathf.Sign(Mathf.Sin(2f * Mathf.PI * phase));
            case "triangle":
                return Mathf.PingPong(phase * 2f, 1f) * 2f - 1f;
            case "sawtooth":
                return (2f * (phase - Mathf.Floor(phase + 0.5f)));
            default:
                return 0f;
        }
    }

    /// <summary>
    /// Apply an ADSR envelope to the sound.
    /// </summary>
    private float ApplyEnvelope()
    {
        envelopeTime += 1f / sampleRate;

        if (envelopeTime < attack) // Attack phase
        {
            return envelopeTime / attack;
        }
        else if (envelopeTime < attack + decay) // Decay phase
        {
            float decayProgress = (envelopeTime - attack) / decay;
            return Mathf.Lerp(1f, sustain, decayProgress);
        }
        else if (envelopeTime < envelopeDuration - release) // Sustain phase
        {
            return sustain;
        }
        else if (envelopeTime < envelopeDuration) // Release phase
        {
            float releaseProgress = (envelopeTime - (envelopeDuration - release)) / release;
            return Mathf.Lerp(sustain, 0f, releaseProgress);
        }

        return 0f; // End of envelope
    }

    /// <summary>
    /// Preset for a kick drum.
    /// </summary>
    public void PlayKick()
    {
        PlayDrum(60f, "sine", 0.1f, 0.01f, 0.15f, 0.2f, 0.1f);
        Debug.Log("Kick drum played!");
    }

    /// <summary>
    /// Preset for a snare drum.
    /// </summary>
    public void PlaySnare()
    {
        PlayDrum(200f, "triangle", 0.5f, 0.01f, 0.1f, 0.1f, 0.2f);
        Debug.Log("Snare drum played!");
    }

    /// <summary>
    /// Preset for a hi-hat.
    /// </summary>
    public void PlayHiHat()
    {
        PlayDrum(4000f, "square", 0.8f, 0.005f, 0.03f, 0.05f, 0.02f);
        Debug.Log("Hi-Hat played!");
    }

    /// <summary>
    /// Preset for a clap.
    /// </summary>
    public void PlayClap()
    {
        PlayDrum(300f, "noise", 0.9f, 0.01f, 0.05f, 0.3f, 0.2f);
        Debug.Log("Clap played!");
    }

    /// <summary>
    /// Preset for percussion.
    /// </summary>
    public void PlayPercussion()
    {
        PlayDrum(150f, "sawtooth", 0.4f, 0.01f, 0.2f, 0.3f, 0.1f);
        Debug.Log("Percussion played!");
    }
}
