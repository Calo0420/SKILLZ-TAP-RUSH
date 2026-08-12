using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Plays the studio intro video fullscreen, then transitions to MainMenu.
/// Tap anywhere or press any key to skip.
/// </summary>
public class IntroVideoPlayer : MonoBehaviour
{
    [SerializeField] private VideoClip introClip;
    [SerializeField] private string nextScene = "MainMenu";
    [SerializeField] private float fadeOutDuration = 0.5f;

    private VideoPlayer videoPlayer;
    private AudioSource videoAudio;
    private bool transitioning;
    private CanvasGroup fadeGroup;

    void Start()
    {
#if UNITY_WEBGL
        // Skillz WebSDK: the game must be ready to receive OnMatchWillBegin
        // immediately on launch, so the studio splash video (a pre-Skillz-launch
        // UX flow, same category as a tutorial/FTUE) is skipped entirely on web —
        // straight to MainMenu, which itself launches Skillz immediately. See
        // MainMenu.cs's UNITY_WEBGL branch for the matching fix.
        SceneManager.LoadScene(nextScene);
        return;
#endif

        // Black background
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }

        CreateFadeOverlay();
        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        // Setup video player
        GameObject vpObj = new GameObject("VideoPlayer");
        videoPlayer = vpObj.AddComponent<VideoPlayer>();
        videoAudio = vpObj.AddComponent<AudioSource>();

        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.targetCamera = Camera.main;
        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
        videoPlayer.isLooping = false;

        // Audio output
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, videoAudio);

        if (introClip != null)
        {
            videoPlayer.clip = introClip;
        }
        else
        {
            // Try loading from path
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "IntroVideo.mp4");
            videoPlayer.url = videoPath;
        }

        videoPlayer.Prepare();

        // Wait for preparation
        float prepTimeout = 5f;
        float prepElapsed = 0f;
        while (!videoPlayer.isPrepared && prepElapsed < prepTimeout)
        {
            prepElapsed += Time.deltaTime;
            yield return null;
        }

        if (!videoPlayer.isPrepared)
        {
            Debug.LogWarning("[TapRush] Intro video failed to prepare, skipping.");
            TransitionToNext();
            yield break;
        }

        // Fade in from black
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 1f;
            float fadeInTime = 0.3f;
            float fadeElapsed = 0f;
            while (fadeElapsed < fadeInTime)
            {
                fadeElapsed += Time.deltaTime;
                fadeGroup.alpha = Mathf.Lerp(1f, 0f, fadeElapsed / fadeInTime);
                yield return null;
            }
            fadeGroup.alpha = 0f;
        }

        videoPlayer.Play();

        // Wait for video to finish
        // Small delay to let the player actually start
        yield return new WaitForSeconds(0.2f);

        while (videoPlayer.isPlaying)
        {
            yield return null;
        }

        // Video finished naturally
        TransitionToNext();
    }

    void Update()
    {
        if (transitioning) return;

        // Skip on tap/click/key
        bool skip = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) skip = true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) skip = true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) skip = true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0)) skip = true;
#endif

        if (skip)
        {
            TransitionToNext();
        }
    }

    private void TransitionToNext()
    {
        if (transitioning) return;
        transitioning = true;

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        if (fadeGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                fadeGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeOutDuration);
                yield return null;
            }
            fadeGroup.alpha = 1f;
        }

        SceneManager.LoadScene(nextScene);
    }

    private void CreateFadeOverlay()
    {
        GameObject canvasObj = new GameObject("FadeCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        fadeGroup = canvasObj.AddComponent<CanvasGroup>();
        fadeGroup.alpha = 1f; // Start black
        fadeGroup.blocksRaycasts = false;
        fadeGroup.interactable = false;

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        UnityEngine.UI.Image img = imageObj.AddComponent<UnityEngine.UI.Image>();
        img.color = Color.black;
        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
