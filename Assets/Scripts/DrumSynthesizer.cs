using UnityEngine;

public class DrumSynthesizer : MonoBehaviour
{
    public float masterVolume = 0.5f;       // Overall volume
    public float sampleRate = 44100f;      // Audio sample rate

    private float phase;                   // Phase for waveform generation
    private float envelopeTime;            // Time elapsed for the envelope
    private float envelopeDuration = 0.0f; // Duration of the envelope
    private bool isPlaying = false;        // If a sound is currently being played

    // Envelope parameters
    private float attack = 0.01f;
    private float decay = 0.2f;
    private float sustain = 0.3f;
    private float release = 0.1f;

    // Active sound parameters
    private float frequency = 0f;          // Frequency of the sound
    private float noiseAmount = 0f;        // Amount of noise
    private string waveform = "sine";      // Waveform type: "sine", "square", "triangle"
    private float stepDuration = 0.0f;     // Duration of a step

    /// <summary>
    /// Calculate and set the step duration based on the current BPM.
    /// </summary>
    public void SetStepDuration(float bpm)
    {
        // Calculate step duration: 60 seconds per minute / (BPM * 4 steps per beat)
        stepDuration = 60f / (bpm * 4f);
        Debug.Log($"Step duration set to {stepDuration} seconds at {bpm} BPM.");
    }

    public void PlayDrum(float freq, string wave, float noise, float attackTime, float decayTime, float sustainLevel, float releaseTime)
    {
        frequency = freq;
        waveform = wave;
        noiseAmount = noise;

        attack = attackTime;
        decay = decayTime;
        sustain = sustainLevel;
        release = releaseTime;

        envelopeTime = 0f;
        envelopeDuration = attack + decay + release;
        isPlaying = true;

        // Reset phase to avoid discontinuities
        phase = 0f;

        // Ensure the AudioSource is running
        AudioSource audioSource = GetComponent<AudioSource>();
        if (!audioSource.isPlaying)
        {
            audioSource.clip = AudioClip.Create("DummyClip", 1, 1, (int)sampleRate, false);
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isPlaying)
        {
            // Fill buffer with silence when not playing
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = 0f;
            }
            return;
        }

        for (int i = 0; i < data.Length; i += channels)
        {
            float sample = GenerateWaveform(waveform, frequency);
            sample += Random.Range(-1f, 1f) * noiseAmount; // Add noise
            sample *= ApplyEnvelope();
            sample *= masterVolume;

            for (int j = 0; j < channels; j++)
            {
                data[i + j] = Mathf.Clamp(sample, -1f, 1f);
            }

            phase += frequency / sampleRate;
            if (phase > 1f) phase -= 1f;

            envelopeTime += 1f / sampleRate;

            // Stop playback when the envelope finishes
            if (envelopeTime >= envelopeDuration)
            {
                isPlaying = false;

                // Stop the AudioSource to prevent continuous playback
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }

                break;
            }
        }
    }

    private float GenerateWaveform(string type, float freq)
    {
        switch (type.ToLower())
        {
            case "sine": return Mathf.Sin(2f * Mathf.PI * phase);
            case "square": return Mathf.Sign(Mathf.Sin(2f * Mathf.PI * phase));
            case "triangle": return Mathf.PingPong(phase * 2f, 1f) * 2f - 1f;
            case "sawtooth": return 2f * (phase - Mathf.Floor(phase + 0.5f));
            default: return 0f;
        }
    }

    private float ApplyEnvelope()
    {
        if (envelopeTime < attack) return envelopeTime / attack;                       // Attack phase
        if (envelopeTime < attack + decay) return Mathf.Lerp(1f, sustain, (envelopeTime - attack) / decay); // Decay phase
        if (envelopeTime < envelopeDuration - release) return sustain;                // Sustain phase
        if (envelopeTime < envelopeDuration) return Mathf.Lerp(sustain, 0f, (envelopeTime - (envelopeDuration - release)) / release); // Release phase

        return 0f; // Ensure sound stops at the end of the envelope
    }

    /// <summary>
    /// Preset for an 8-bit kick drum.
    /// </summary>
    public void PlayKick()
    {
        PlayDrum(120f, "square", 0.1f, 0.01f, 0.1f, 0.8f, 0.1f);
        Debug.Log("8-bit Kick drum played!");
    }

    /// <summary>
    /// Preset for an 8-bit snare drum.
    /// </summary>
    public void PlaySnare()
    {
        PlayDrum(300f, "noise", 0.6f, 0.005f, 0.1f, 0.4f, 0.05f);
        Debug.Log("8-bit Snare drum played!");
    }

    /// <summary>
    /// Preset for an 8-bit hi-hat.
    /// </summary>
    public void PlayHiHat()
    {
        PlayDrum(8000f, "square", 0.9f, 0.001f, 0.05f, 0.3f, 0.02f);
        Debug.Log("8-bit Hi-Hat played!");
    }

    /// <summary>
    /// Preset for an 8-bit clap.
    /// </summary>
    public void PlayClap()
    {
        PlayDrum(600f, "noise", 1.0f, 0.005f, 0.08f, 0.3f, 0.1f);
        Debug.Log("8-bit Clap played!");
    }

    /// <summary>
    /// Preset for 8-bit percussion.
    /// </summary>
    public void PlayPercussion()
    {
        PlayDrum(200f, "triangle", 0.2f, 0.01f, 0.15f, 0.5f, 0.1f);
        Debug.Log("8-bit Percussion played!");
    }
}
