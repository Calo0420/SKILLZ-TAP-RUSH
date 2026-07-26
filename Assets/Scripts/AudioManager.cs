using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource comboSource;
    [SerializeField] private AudioClip correctTapClip;
    [SerializeField] private AudioClip wrongTapClip;
    [SerializeField] private AudioClip comboMilestoneClip;
    [SerializeField] private AudioClip gaugeTapClip;
    [SerializeField] private AudioClip gaugeCompleteClip;


    [Header("Music")]
    [SerializeField] private AudioClip gameplayMusicClip;
    [SerializeField] private AudioClip chaosMusicClip;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.45f;


    [Header("Chaos Finale")]
    [SerializeField] private AudioClip chaosStartClip;
    [SerializeField] private float chaosMusicPitch = 1.2f;

    private AudioSource musicSource;



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

        musicSource = EnsureMusicSource();
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

public void PlayBonusHit()
    {
        // Brighter/louder feedback for yellow bonus targets.
        PlayOneShot(sfxSource != null ? sfxSource : comboSource, comboMilestoneClip, "bonus hit", 1.5f);
    }


public void PlayGaugeTap()
    {
        PlayOneShot(sfxSource, gaugeTapClip, "gauge tap", 0.9f);
    }

    public void PlayGaugeComplete()
    {
        PlayOneShot(comboSource != null ? comboSource : sfxSource, gaugeCompleteClip, "gauge complete", 1.1f);
    }


public void PlayChaosStartCue()
    {
        // Reuse combo source if no dedicated electric clip is assigned yet.
        if (chaosStartClip != null)
        {
            PlayOneShot(comboSource != null ? comboSource : sfxSource, chaosStartClip, "chaos start", 1.15f);
        }
        else
        {
            PlayComboMilestone();
        }
    }

public void SetChaosMusicState(bool enabled)
    {
        if (musicSource == null)
        {
            musicSource = EnsureMusicSource();
        }

        AudioClip targetClip = enabled ? chaosMusicClip : gameplayMusicClip;
        if (targetClip != null)
        {
            if (musicSource.clip != targetClip)
            {
                musicSource.clip = targetClip;
            }

            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }

        musicSource.pitch = enabled ? chaosMusicPitch : 1f;
    }

private AudioSource EnsureMusicSource()
    {
        AudioSource existing = GetComponent<AudioSource>();
        if (existing != null && existing != sfxSource && existing != comboSource)
        {
            existing.loop = true;
            existing.playOnAwake = false;
            existing.spatialBlend = 0f;
            existing.volume = musicVolume;
            return existing;
        }

        GameObject musicSourceObject = new GameObject("MusicAudioSource");
        musicSourceObject.transform.SetParent(transform, false);
        AudioSource created = musicSourceObject.AddComponent<AudioSource>();
        created.playOnAwake = false;
        created.loop = true;
        created.spatialBlend = 0f;
        created.volume = musicVolume;
        return created;
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
