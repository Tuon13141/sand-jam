#if CRAZY_LABS_CLIK
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Tabtale.TTPlugins
{
    public class PlatformChecker : IPreprocessBuildWithReport
    {
        public int callbackOrder => 95;

        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            Debug.Log("PlatformChecker::OnPreprocessBuild:");
            string platformCode = CLIKConfiguration.GetPlatformCode(report.summary.platform);
            CLIKConfiguration.LoadConfigurationsFromFiles();
            if (!CLIKConfiguration.GlobalConfigFileExists(platformCode))
            {
                ShowNoConfigError();
            }
            else if (CLIKConfiguration.IsInvalidPlatfrom(report.summary.platform))
            {
                ShowInvalidPlatformError();
            }

            CopyConfigurationForPlatform(platformCode);
        }

        private static void ShowNoConfigError()
        {
            ShowError("You have built an app with CLIK but did not load a configuration ZIP. " +
                      "Please load a ZIP file through CLIK → Load Configuration….");
        }
        
        private static void ShowInvalidPlatformError()
        {
            ShowError("You have started a build for your game in platform: " +
                      (CLIKConfiguration.IsAndroid() ? "Android" : "iOS") +
                      ", but the configuration you have loaded is meant for " +
                      (CLIKConfiguration.IsAndroid() ? "iOS" : "Android") +
                      ". While the build might pass and run, publishing functions will misbehave. " + 
                      "Please load the correct zip file for this platform");
        }
        
        private static void ShowError(string message)
        {
            Debug.Log("PlatformChecker::OnPreprocessBuild: error - " + message);
            if (!TTPEditorUtils.IsTTPBatchMode())
            {
                EditorUtility.DisplayDialog("Error", message, "Got it");
            }
        }
        
        private static void CopyConfigurationForPlatform(string platformCode)
        {
            Debug.Log("PlatformChecker::CopyConfigurationForPlatform:" + platformCode);

            var destinationPath = CLIKConfiguration.GetTTPConfigPath();
            if (Directory.Exists(destinationPath))
            {
                Debug.Log("PlatformChecker::CopyConfigurationForPlatform:removing files at path:" + destinationPath);
                Directory.Delete(CLIKConfiguration.GetTTPConfigPath(), true);
            }

            Directory.CreateDirectory(destinationPath);

            var sourcePath = CLIKConfiguration.GetTTPTemplateConfigPath(platformCode);
            var files = Directory.GetFiles(sourcePath);
            foreach (var file in files)
            {
                Debug.Log("PlatformChecker::CopyConfigurationForPlatform:" + Path.GetFileName(file));
                var fileDest = file.Replace(sourcePath, destinationPath);
                File.Copy(file, fileDest, true);
            }
        }
    }
}
#endif