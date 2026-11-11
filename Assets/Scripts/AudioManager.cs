using UnityEngine;

/// <summary>
/// Sonidos generados por código (sin archivos de audio): soltar, aterrizar, perfecto, inicio y Game Over.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public enum Sfx { Drop, Land, Perfect, Start, GameOver }

    public static AudioManager Instance { get; private set; }

    [Range(0f, 1f)] public float volume = 0.7f;

    AudioSource[] sources;
    int next;
    AudioClip[] clips;
    const int SampleRate = 44100;

    void Awake()
    {
        Instance = this;
        sources = new AudioSource[6];
        for (int i = 0; i < sources.Length; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
        }

        clips = new AudioClip[5];
        clips[(int)Sfx.Drop] = Tone("Drop", 0.12f, 700f, 350f, 0.15f, 0.25f);
        clips[(int)Sfx.Land] = Tone("Land", 0.18f, 160f, 60f, 0.45f, 0.7f);
        clips[(int)Sfx.Perfect] = Chime("Perfect", 0.45f, 880f);
        clips[(int)Sfx.Start] = Tone("Start", 0.25f, 400f, 900f, 0f, 0.3f);
        clips[(int)Sfx.GameOver] = Tone("GameOver", 0.9f, 320f, 70f, 0.1f, 0.5f);
    }

    public static void Play(Sfx sfx, float pitch = 1f)
    {
        if (Instance == null) return;
        var src = Instance.sources[Instance.next];
        Instance.next = (Instance.next + 1) % Instance.sources.Length;
        src.pitch = pitch;
        src.PlayOneShot(Instance.clips[(int)sfx], Instance.volume);
    }

    // Barrido de frecuencia con un poco de ruido y envolvente que decae
    static AudioClip Tone(string name, float duration, float f0, float f1, float noise, float vol)
    {
        int n = Mathf.CeilToInt(duration * SampleRate);
        var data = new float[n];
        float phase = 0f;
        var rng = new System.Random(name.GetHashCode());
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float f = Mathf.Lerp(f0, f1, t);
            phase += 2f * Mathf.PI * f / SampleRate;
            float env = (1f - t) * (1f - t) * Mathf.Min(1f, i / (0.005f * SampleRate));
            float s = Mathf.Sin(phase) * (1f - noise) + ((float)rng.NextDouble() * 2f - 1f) * noise;
            data[i] = s * env * vol;
        }
        var clip = AudioClip.Create(name, n, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // Campanita brillante (fundamental + armónicos)
    static AudioClip Chime(string name, float duration, float f)
    {
        int n = Mathf.CeilToInt(duration * SampleRate);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float time = (float)i / SampleRate;
            float env = Mathf.Exp(-time * 9f) * Mathf.Min(1f, i / (0.003f * SampleRate));
            float s = Mathf.Sin(2f * Mathf.PI * f * time)
                      + 0.5f * Mathf.Sin(2f * Mathf.PI * f * 1.5f * time)
                      + 0.25f * Mathf.Sin(2f * Mathf.PI * f * 2f * time);
            data[i] = s * env * 0.25f;
        }
        var clip = AudioClip.Create(name, n, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
