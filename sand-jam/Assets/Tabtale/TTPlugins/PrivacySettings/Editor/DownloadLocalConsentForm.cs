using UnityEditor;
using UnityEngine;
using System.IO;

namespace Tabtale.TTPlugins
{
    public class DownloadLocalConsentForm
    {
#if UNITY_ANDROID && !UNITY_2021_1_OR_NEWER
        private const string CONFIGURATIONS_PATH = "/Plugins/Android/assets/ttp/configurations";
#else
        private const string CONFIGURATIONS_PATH = "/StreamingAssets/ttp/configurations";
#endif

        private const string STREAMING_ASSETS_PATH_JSON = CONFIGURATIONS_PATH + "/privacySettings.json";
        private const string STREAMING_ASSETS_PATH_ADDITIONAL_JSON = CONFIGURATIONS_PATH + "/additionalConfig.json";
        private const string STREAMING_ASSETS_PATH_CONSENT_ANDROID = "/Plugins/Android/assets/consentForm";
        private const string STREAMING_ASSETS_PATH_PRIVACY_ANDROID = "/Plugins/Android/assets/privacyForm";
        private const string STREAMING_ASSETS_PATH_PRIVACY_TCF_ANDROID = "/Plugins/Android/assets/privacyFormTCF";
        private const string STREAMING_ASSETS_PATH_CONSENT = "/StreamingAssets/consentForm";
        private const string STREAMING_ASSETS_PATH_PRIVACY = "/StreamingAssets/privacyForm";
        private const string STREAMING_ASSETS_PATH_PRIVACY_TCF = "/StreamingAssets/privacyFormTCF";
        private const string STREAMING_ASSETS_PATH_CONSENT_ZIP = "/StreamingAssets/consentForm.zip";
        private const string STREAMING_ASSETS_PATH_PRIVACY_ZIP = "/StreamingAssets/privacyForm.zip";
        private const string STREAMING_ASSETS_PATH_PRIVACY_TCF_ZIP = "/StreamingAssets/privacyFormTCF.zip";
        private const string STREAMING_ASSETS_PATH_HTML = "/StreamingAssets/index.html";

        [System.Serializable]
        private class PrivacySettingsConfiguration
        {
            public string consentFormURL;
            public string privacySettingsURL;
            public string privacySettingsTCFURL;
        }

        [System.Serializable]
        private class AdditionalConfiguration
        {
            public bool iabConsent = true;
        }

        public void DownloadLocalConsentForms(BuildTarget target)
        {
            Debug.Log("DownloadLocalConsentForm");

            bool iabConsent = true;
            PrivacySettingsConfiguration privacySettingsConfiguration = null;

            string jsonFp = Application.dataPath + STREAMING_ASSETS_PATH_JSON;
            if (File.Exists(jsonFp))
            {
                try
                {
                    string jsonStr = File.ReadAllText(jsonFp);
                    privacySettingsConfiguration = JsonUtilityWrapper.FromJson<PrivacySettingsConfiguration>(jsonStr);
                }
                catch (System.Exception e)
                {
                    Debug.Log("DownloadLocalConsentForm::OnPreprocessBuild: failed to process DownloadLocalConsentFormsNonIAB. exception - " +
                              e.Message);
                    EditorApplication.Exit(-1);
                }
            }

            if (privacySettingsConfiguration == null)
            {
                Debug.Log("DownloadLocalConsentForm::OnPreprocessBuild: privacy config json doesn't exist");
                EditorApplication.Exit(-1);
            }
            
            string jsonAdditionalConfig = Application.dataPath + STREAMING_ASSETS_PATH_ADDITIONAL_JSON;
            if (File.Exists(jsonAdditionalConfig))
            {
                try
                {
                    string jsonStr = File.ReadAllText(jsonAdditionalConfig);
                    AdditionalConfiguration additionalConfiguration =
                        JsonUtilityWrapper.FromJson<AdditionalConfiguration>(jsonStr);
                    if (additionalConfiguration != null)
                    {
                        iabConsent = additionalConfiguration.iabConsent;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.Log(
                        "DownloadLocalConsentForm::OnPreprocessBuild: failed to process DownloadLocalConsentFormsIAB. exception - " +
                        e.Message);
                    EditorApplication.Exit(-1);
                }
            }

            Debug.Log("DownloadLocalConsentForm:iabConsent=" + iabConsent);

            try
            {
                if (iabConsent)
                {
                    DownloadLocalConsentFormsIAB(target,
                        privacySettingsConfiguration.privacySettingsTCFURL);
                }
                else
                {
                    DownloadLocalConsentFormsNonIAB(target,
                        privacySettingsConfiguration.consentFormURL,
                        privacySettingsConfiguration.privacySettingsURL);
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(
                    "DownloadLocalConsentForm::OnPreprocessBuild: failed to process. exception - " +
                    e.Message);
                EditorApplication.Exit(-1);
            }
        }

        private void DownloadLocalConsentFormsIAB(BuildTarget target, string privacyFormTCFUrl)
        {
            if (string.IsNullOrEmpty(privacyFormTCFUrl))
            {
                Debug.LogError("DownloadLocalConsentFormsIAB:: privacyFormTCFUrl does not exist - aborting.");
                throw new System.Exception("privacyFormTCFUrl does not exist");
            }
            else
            {
                var isOldUnity = false;
#if !UNITY_2021_3_OR_NEWER
                isOldUnity = true;
#endif
                string privacyDst = "";
                if (target == BuildTarget.Android && isOldUnity)
                {
                    privacyDst = Application.dataPath + STREAMING_ASSETS_PATH_PRIVACY_TCF_ANDROID;
                }
                else
                {
                    privacyDst = Application.dataPath + STREAMING_ASSETS_PATH_PRIVACY_TCF;
                }

                DownloadConsentForms(privacyFormTCFUrl, Application.dataPath + STREAMING_ASSETS_PATH_PRIVACY_TCF_ZIP,
                    privacyDst);
            }
        }

        private void DownloadLocalConsentFormsNonIAB(BuildTarget target, string consentFormUrl, string privacyFormUrl)
        {
            if (string.IsNullOrEmpty(consentFormUrl) || string.IsNullOrEmpty(privacyFormUrl))
            {
                Debug.LogError("DownloadLocalConsentFormsNonIAB:: consentFormUrl or privacySettingsURL do not exist - aborting.");
                throw new System.Exception("consentFormUrl or privacySettingsURL do not exist");
            }
            else
            {
                string consentDst = "";
                string privacyDst = "";
                                var isOldUnity = false;
#if !UNITY_2021_3_OR_NEWER
                isOldUnity = true;
#endif
                if (target == BuildTarget.Android && isOldUnity)
                {
                    consentDst = Application.dataPath + STREAMING_ASSETS_PATH_CONSENT_ANDROID;
                    privacyDst = Application.dataPath + STREAMING_ASSETS_PATH_PRIVACY_ANDROID;
                }
                else
                {
                    consentDst = Application.dataPath + STREAMING_ASSETS_PATH_CONSENT;
                    privacyDst = Application.dataPath + STREAMING_ASSETS_PATH_PRIVACY;
                }

                DownloadConsentForms(consentFormUrl, Application.dataPath + STREAMING_ASSETS_PATH_CONSENT_ZIP,
                    consentDst);
                DownloadConsentForms(privacyFormUrl, Application.dataPath + STREAMING_ASSETS_PATH_PRIVACY_ZIP,
                    privacyDst);
            }
        }

        private void DownloadConsentForms(string url, string pathToZip, string unzipFolder)
        {
            Debug.Log("DownloadConsentForms:: url - " + url);

            string exceptionMessage = null;
            if (TTPEditorUtils.DownloadFile(url, pathToZip))
            {
                try
                {
                    string tmpExistingFolder = unzipFolder + "_tmp";
                    if (Directory.Exists(unzipFolder))
                    {
                        Directory.Move(unzipFolder, tmpExistingFolder);
                    }

                    Directory.CreateDirectory(unzipFolder);
                    ZipUtil.Unzip(pathToZip, unzipFolder);
                    if (!File.Exists(unzipFolder + "/index.html"))
                    {
                        DeleteDirectory(unzipFolder);
                        Directory.Move(tmpExistingFolder, unzipFolder);
                        exceptionMessage =
                            "DownloadConsentForms:: Html for consent form not found. Something must have went wrong with the download process.";
                    }

                    File.Delete(pathToZip);
                    if (Directory.Exists(tmpExistingFolder))
                    {
                        DeleteDirectory(tmpExistingFolder);
                    }
                }
                catch (System.Exception e)
                {
                    exceptionMessage =
                        "DownloadConsentForms:: Failed to write consent form zip to file system. Exception - " +
                        e.Message;
                }
            }
            else
            {
                exceptionMessage = "DownloadConsentForms:: Failed to retrieve consent form from server";
            }

            if (exceptionMessage != null)
            {
                throw new System.Exception(exceptionMessage);
            }
            else
            {
                Debug.Log("DownloadConsentForms:: Successfully downloaded consent and privacy forms");
            }
        }

        private void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }
    }
}