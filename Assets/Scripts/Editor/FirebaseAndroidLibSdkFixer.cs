#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Android;

/// <summary>
/// Unity's own legacy .androidlib -> Gradle conversion (used for
/// Assets/Plugins/Android/FirebaseApp.androidlib, which only ships an old
/// Eclipse-style project.properties) generates a build.gradle with
/// "minSdk 26 / targetSdk 9" regardless of Player Settings — targetSdk below
/// minSdk makes Gradle report "No variants exist" for the module. This is
/// unconditionally regenerated on every export (both a hand-written
/// build.gradle in the source folder and Player Settings changes get
/// ignored), so the only fix that survives future exports is patching it
/// right after Unity generates it, before Gradle runs. Values are pinned to
/// match launcherTemplate.gradle's hardcoded minSdk 24 / targetSdk 34 so the
/// app module and this library can never drift apart again.
/// </summary>
public class FirebaseAndroidLibSdkFixer : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 0;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        // This callback fires once per generated Gradle module with that module's own
        // export path (e.g. .../unityLibrary), not once per nested .androidlib — so
        // search under it rather than checking `path` itself.
        foreach (string buildGradlePath in Directory.GetFiles(path, "build.gradle", SearchOption.AllDirectories))
        {
            if (!Path.GetDirectoryName(buildGradlePath).EndsWith("FirebaseApp.androidlib")) continue;

            string content = File.ReadAllText(buildGradlePath);
            string patched = Regex.Replace(content, @"minSdk\s+\d+", "minSdk 24");
            patched = Regex.Replace(patched, @"targetSdk\s+\d+", "targetSdk 34");

            if (patched != content)
            {
                File.WriteAllText(buildGradlePath, patched);
                UnityEngine.Debug.Log("[FirebaseAndroidLibSdkFixer] Patched FirebaseApp.androidlib build.gradle minSdk/targetSdk at " + buildGradlePath);
            }
        }
    }
}
#endif
