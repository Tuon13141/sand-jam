using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;

namespace Tabtale.TTPlugins
{
    public class SettingsValidator
    {
        // Use adaptive icons
        public static bool ValidateIcons()
        {
#if UNITY_ANDROID
            // This validation is actual only for Android
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                return true;

            var icons = PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android, AndroidPlatformIconKind.Adaptive);
            foreach (var icon in icons)
                if (icon.GetTextures().Length == 0)
                    return false;
#endif
            return true;
        }

        // Enable Gradle Build Templates + Resolver (check is there .gradle in 'Assets/Plugins/Android')
        public static bool ValidateGradleTemplates()
        {
#if UNITY_ANDROID
            // This validation is actual only for Android
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                return true;

            var gradleBuildTemplates = Directory
                .EnumerateFiles("Assets/Plugins/Android", "*.gradle", SearchOption.AllDirectories);
            if (gradleBuildTemplates.ToArray().Length == 0)
                return false;
#endif
            return true;

        }

        // Select Target Architecture ARM64 (requires Scripting Backend IL2CPP)
        public static bool ValidateTargetArchitecture()
        {
#if UNITY_ANDROID
            // This validation is actual only for Android
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                return true;

            var archs = PlayerSettings.Android.targetArchitectures;
            return (archs & AndroidArchitecture.ARM64) != 0;
#endif
            return true;
        }

        // Minimum API Level to 23 (this value can change between versions of CLIK Plugin)
        public static bool ValidateTargetMinSdkVersion()
        {
#if UNITY_ANDROID
            // This validation is actual only for Android
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                return true;

            return PlayerSettings.Android.minSdkVersion >= AndroidSdkVersions.AndroidApiLevel23;
#endif
            return true;
        }

        // Set Target API Level to 31 (this value can change between versions of CLIK Plugin)
        public static bool ValidateTargetSdkVersion()
        {
#if UNITY_ANDROID
            // This validation is actual only for Android
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                return true;

            return PlayerSettings.Android.targetSdkVersion > AndroidSdkVersions.AndroidApiLevel30;
#endif
            return true;
        }

        // Select Build App Bundle (Google Play)
        public static bool ValidateBuildAppBundleSetting()
        {
#if UNITY_ANDROID
            // This validation is actual only for Android
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                return true;

            return EditorUserBuildSettings.buildAppBundle;
#endif
            return true;
        }
    }
}