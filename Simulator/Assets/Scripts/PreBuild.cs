#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

class PreBuild : IPreprocessBuildWithReport {
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report) {

        string[] versionParts = PlayerSettings.bundleVersion.Split('.');
        if (versionParts.Length != 3 || int.Parse(versionParts[2]) == -1) {
            Debug.LogError("BuildPostprocessor failed to update version " + PlayerSettings.bundleVersion);
            return;
        }
        // major-minor-build
        versionParts[2] = (int.Parse(versionParts[2]) + 1).ToString();
        PlayerSettings.bundleVersion = string.Join(".", versionParts);
    }
}
#endif