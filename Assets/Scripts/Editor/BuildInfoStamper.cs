#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Diagnostics;

/// <summary>
/// Pre-build step: stamps the current git commit hash into BuildInfo.cs
/// so every APK is traceable to its exact source commit.
/// </summary>
public class BuildInfoStamper : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        string hash = GetGitCommitHash();
        BuildInfo.CommitHash = hash;
        UnityEngine.Debug.Log($"[BuildInfoStamper] Stamped build with commit: {hash}");
    }

    private static string GetGitCommitHash()
    {
        try
        {
            var psi = new ProcessStartInfo("git", "rev-parse --short HEAD")
            {
                WorkingDirectory = System.IO.Path.GetDirectoryName(UnityEngine.Application.dataPath),
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                return string.IsNullOrEmpty(output) ? "unknown" : output;
            }
        }
        catch
        {
            return "unknown";
        }
    }
}
#endif
