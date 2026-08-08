using UnityEngine;

/// <summary>
/// Stamps the current git commit hash at build time. Visible in debug logs on boot.
/// </summary>
public class BuildInfo : MonoBehaviour
{
    // This value is replaced at build time by the pre-build script
    public static string CommitHash = "dev";

    void Awake()
    {
        Debug.Log($"[TapRush] Build: {CommitHash}");
    }
}
