using UnityEngine;
using System.Collections;

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
    [SerializeField] private AudioClip countdownBeepClip;

    [Header("Music")]
    [SerializeField] private AudioClip gameplayMusicClip;
    [SerializeField] private AudioClip chaosMusicClip;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.45f;
    [SerializeField] private float musicCrossfadeDuration = 1.5f;

    [Header("Dynamic Music")]
    [Tooltip("Music pitch increases slightly as combo builds — creates tension.")]
    [SerializeField] private float comboPitchRampMax = 1.08f;
    [SerializeField] private float comboPitchRampSmoothing = 3f;
    [Tooltip("How many combo hits to reach max pitch ramp.")]
    [SerializeField] private int comboPitchFullAt = 20;
    [Tooltip("Brief volume duck on each tap for rhythmic feel.")]
    [SerializeField] private float tapDuckAmount = 0.12f;
    [SerializeField] private float tapDuckRecovery = 6f;

    [Header("Musical Tap Pitch")]
    [Tooltip("Tap SFX pitch climbs a pentatonic scale with combo.")]
    [SerializeField] private bool enableMusicalTaps = true;
    // Pentatonic semitone offsets (C, D, E, G, A pattern repeating)
    private static readonly float[] PentatonicSemitones = { 0, 2, 4, 7, 9, 12, 14, 16, 19, 21 };

    [Header("Chaos Finale")]
    [SerializeField] private AudioClip chaosStartClip;
    [SerializeField] private float chaosMusicPitch = 1.2f;

    private AudioSource musicSourceA;
    private AudioSource musicSourceB;
    private AudioSource activeMusicSource;
    private Coroutine crossfadeRoutine;

    private float targetMusicPitch = 1f;
    private float currentDuck;
    private bool chaosMusicActive;

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

        musicSourceA = CreateMusicSource("MusicSourceA");
        musicSourceB = CreateMusicSource("MusicSourceB");
        activeMusicSource = musicSourceA;
    }

    void Update()
    {
        // Smooth pitch ramp
        if (activeMusicSource != null && !chaosMusicActive)
        {
            activeMusicSource.pitch = Mathf.Lerp(activeMusicSource.pitch, targetMusicPitch, Time.deltaTime * comboPitchRampSmoothing);
        }

        // Recover from tap duck
        if (currentDuck > 0f)
        {
            currentDuck = Mathf.MoveTowards(currentDuck, 0f, Time.deltaTime * tapDuckRecovery);
            ApplyMusicVolume();
        }
    }

    public void PlayCorrectTap()
    {
        float pitch = 1f;
        if (enableMusicalTaps && GameManager.Instance != null)
        {
            int combo = GameManager.Instance.ComboCount;
            int noteIndex = Mathf.Clamp(combo % PentatonicSemitones.Length, 0, PentatonicSemitones.Length - 1);
            pitch = Mathf.Pow(2f, PentatonicSemitones[noteIndex] / 12f);
        }

        PlayOneShotPitched(sfxSource, correctTapClip, "correct tap", pitch, 1f);

        // Duck music on tap for rhythmic feel
        currentDuck = tapDuckAmount;
        ApplyMusicVolume();

        // Update combo-driven pitch ramp
        UpdateComboPitch();
    }

    public void PlayWrongTap()
    {
        PlayOneShot(sfxSource, wrongTapClip, "wrong tap");
        // Reset music pitch back to baseline on combo break
        targetMusicPitch = 1f;
    }

    public void PlayComboMilestone()
    {
        // Pitch the milestone sound based on how high the combo is
        float milestonePitch = 1f + (GameManager.Instance != null ? GameManager.Instance.ComboCount / 50f : 0f);
        PlayOneShotPitched(comboSource != null ? comboSource : sfxSource, comboMilestoneClip, "combo milestone", milestonePitch, 1.1f);
    }

    public void PlayBonusHit()
    {
        PlayOneShotPitched(sfxSource != null ? sfxSource : comboSource, comboMilestoneClip, "bonus hit", 1.3f, 1.5f);
        currentDuck = tapDuckAmount * 1.5f;
        ApplyMusicVolume();
    }

    public void PlayGaugeTap()
    {
        PlayOneShot(sfxSource, gaugeTapClip, "gauge tap", 0.9f);
    }

    public void PlayGaugeComplete()
    {
        PlayOneShot(comboSource != null ? comboSource : sfxSource, gaugeCompleteClip, "gauge complete", 1.1f);
    }

    public void PlayCountdownBeep(int second)
    {
        AudioClip clipToPlay = countdownBeepClip != null ? countdownBeepClip : gaugeTapClip;
        // Pitch rises as countdown approaches zero
        float beepPitch = 0.8f + (1f - Mathf.Clamp01(second / 5f)) * 0.5f;
        PlayOneShotPitched(comboSource != null ? comboSource : sfxSource, clipToPlay, "countdown beep " + second, beepPitch, 1f);
    }

    public void PlayChaosStartCue()
    {
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
        chaosMusicActive = enabled;

        AudioClip targetClip = enabled ? chaosMusicClip : gameplayMusicClip;
        if (targetClip == null) return;

        // Crossfade to the new track
        AudioSource incoming = (activeMusicSource == musicSourceA) ? musicSourceB : musicSourceA;
        incoming.clip = targetClip;
        incoming.pitch = enabled ? chaosMusicPitch : 1f;
        incoming.loop = true;
        incoming.Play();

        if (crossfadeRoutine != null) StopCoroutine(crossfadeRoutine);
        crossfadeRoutine = StartCoroutine(CrossfadeMusic(activeMusicSource, incoming, musicCrossfadeDuration));
        activeMusicSource = incoming;

        if (!enabled)
        {
            targetMusicPitch = 1f;
        }
    }

    private IEnumerator CrossfadeMusic(AudioSource outgoing, AudioSource incoming, float duration)
    {
        float elapsed = 0f;
        float startVolumeOut = outgoing.volume;
        float targetVolume = musicVolume - currentDuck;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            // Equal-power crossfade
            float fadeOut = Mathf.Cos(t * Mathf.PI * 0.5f);
            float fadeIn = Mathf.Sin(t * Mathf.PI * 0.5f);
            outgoing.volume = startVolumeOut * fadeOut;
            incoming.volume = targetVolume * fadeIn;
            yield return null;
        }

        outgoing.Stop();
        outgoing.volume = 0f;
        incoming.volume = targetVolume;
        crossfadeRoutine = null;
    }

    private void UpdateComboPitch()
    {
        if (GameManager.Instance == null) return;
        float comboNorm = Mathf.Clamp01((float)GameManager.Instance.ComboCount / comboPitchFullAt);
        targetMusicPitch = Mathf.Lerp(1f, comboPitchRampMax, comboNorm);
    }

    private void ApplyMusicVolume()
    {
        float vol = musicVolume - currentDuck;
        if (activeMusicSource != null)
        {
            activeMusicSource.volume = Mathf.Max(vol, 0.05f);
        }
    }

    private AudioSource CreateMusicSource(string sourceName)
    {
        GameObject go = new GameObject(sourceName);
        go.transform.SetParent(transform, false);
        AudioSource src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 0f;
        src.volume = 0f;
        src.priority = 64;
        return src;
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

    private void PlayOneShotPitched(AudioSource source, AudioClip clip, string clipName, float pitch, float volumeScale = 1f)
    {
        if (source == null || clip == null)
        {
            PlayOneShot(source, clip, clipName, volumeScale);
            return;
        }

        // Temporarily change pitch, play, restore — works for overlapping one-shots
        float originalPitch = source.pitch;
        source.pitch = pitch;
        source.PlayOneShot(clip, volumeScale);
        source.pitch = originalPitch;
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
