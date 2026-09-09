using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Handles crisp mobile haptic feedback for Tap Rush.
/// Uses native iOS Taptic Engine (UIImpactFeedbackGenerator) on iOS,
/// and Handheld.Vibrate() / vibration fallback on other platforms.
/// </summary>
public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance { get; private set; }

    [SerializeField] private bool hapticsEnabled = true;

    public bool HapticsEnabled
    {
        get => hapticsEnabled;
        set => hapticsEnabled = value;
    }

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _tapRush_TriggerImpactLight();

    [DllImport("__Internal")]
    private static extern void _tapRush_TriggerImpactMedium();

    [DllImport("__Internal")]
    private static extern void _tapRush_TriggerImpactHeavy();

    [DllImport("__Internal")]
    private static extern void _tapRush_TriggerNotificationWarning();
#endif

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Subtle, crisp click for normal successful target taps.
    /// </summary>
    public void PlayLight()
    {
        if (!hapticsEnabled) return;

#if UNITY_IOS && !UNITY_EDITOR
        try { _tapRush_TriggerImpactLight(); } catch { Handheld.Vibrate(); }
#elif UNITY_ANDROID && !UNITY_EDITOR
        // Very brief vibration pulse on Android
        Handheld.Vibrate();
#endif
    }

    /// <summary>
    /// Moderate punch for Gold Bonus hits and Gauge taps.
    /// </summary>
    public void PlayMedium()
    {
        if (!hapticsEnabled) return;

#if UNITY_IOS && !UNITY_EDITOR
        try { _tapRush_TriggerImpactMedium(); } catch { Handheld.Vibrate(); }
#elif UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }

    /// <summary>
    /// Heavy vibration for Combo Milestones (x5, x10, x15), Chaos mode activation, and Power-up ready.
    /// </summary>
    public void PlayHeavy()
    {
        if (!hapticsEnabled) return;

#if UNITY_IOS && !UNITY_EDITOR
        try { _tapRush_TriggerImpactHeavy(); } catch { Handheld.Vibrate(); }
#elif UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }

    /// <summary>
    /// Double buzz / warning rumble for decoy red targets and wrong taps.
    /// </summary>
    public void PlayWarning()
    {
        if (!hapticsEnabled) return;

#if UNITY_IOS && !UNITY_EDITOR
        try { _tapRush_TriggerNotificationWarning(); } catch { Handheld.Vibrate(); }
#elif UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }
}
