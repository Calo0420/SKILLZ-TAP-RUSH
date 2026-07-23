using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource comboSource;
    [SerializeField] private AudioClip correctTapClip;
    [SerializeField] private AudioClip wrongTapClip;
    [SerializeField] private AudioClip comboMilestoneClip;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (comboSource == null)
        {
            comboSource = CreateFallbackComboSource();
        }
    }

    public void PlayCorrectTap()
    {
        PlayOneShot(sfxSource, correctTapClip, "correct tap");
    }

    public void PlayWrongTap()
    {
        PlayOneShot(sfxSource, wrongTapClip, "wrong tap");
    }

    public void PlayComboMilestone()
    {
        PlayOneShot(comboSource != null ? comboSource : sfxSource, comboMilestoneClip, "combo milestone", 1.1f);
    }

    private void PlayOneShot(AudioSource source, AudioClip clip, string clipName, float volumeScale = 1f)
    {
        if (source == null)
        {
            Debug.LogWarning("[TapRush] AudioSource missing on AudioManager for: " + clipName);
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning("[TapRush] Missing audio clip: " + clipName);
            return;
        }

        source.PlayOneShot(clip, volumeScale);
    }

    private AudioSource CreateFallbackComboSource()
    {
        if (sfxSource == null)
        {
            return null;
        }

        GameObject comboSourceObject = new GameObject("ComboAudioSource");
        comboSourceObject.transform.SetParent(transform, false);
        AudioSource created = comboSourceObject.AddComponent<AudioSource>();
        created.playOnAwake = false;
        created.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
        created.volume = sfxSource.volume;
        created.pitch = 1f;
        created.spatialBlend = 0f;
        return created;
    }
}
