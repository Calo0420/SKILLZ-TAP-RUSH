using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;
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
    }

    public void PlayCorrectTap()
    {
        PlayOneShot(correctTapClip);
    }

    public void PlayWrongTap()
    {
        PlayOneShot(wrongTapClip);
    }

    public void PlayComboMilestone()
    {
        PlayOneShot(comboMilestoneClip);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }
}
