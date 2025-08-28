using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Linq;


namespace Tabtale.TTPlugins
{
    public enum TTPEnvironment
    {
        PRODUCTION, APPTESTING, STAGING
    }

    public class TTPMenu : MonoBehaviour
    {
        private const string PRODUCTION_DOMAIN = "http://ttplugins.ttpsdk.info";
        private const string PRODUCTION_APPSDB_DOMAIN = "http://appsdb.ttpsdk.info";
        private const string STAGING_DOMAIN = "http://ttplugins.ttpsdk-staging.info";
        private const string STAGING_APPSDB_DOMAIN = "http://appsdb.ttpsdk-staging.info";
        private const string APPTESTING_DOMAIN = "http://apptesting-ttplugins.ttpsdk-staging.info";
        private const string APPTESTING_APPSDB_DOMAIN = "http://tt-apptesting-appsdb.us-west-2.elasticbeanstalk.com";

        private const string SYMBOL_ANDROID_STORE_AMAZON = "TTP_ANDROID_STORE_AMAZON";
        
#if UNITY_ANDROID && !UNITY_2021_1_OR_NEWER
        private const string CONFIGURATIONS_PATH = "Assets/Plugins/Android/assets/ttp/configurations";
#else
        private const string CONFIGURATIONS_PATH = "Assets/StreamingAssets/ttp/configurations";
#endif
        private const string APP_CONFIG_PATH = "Assets/StreamingAssets/app_config.json";

        public static event Action<string, string> OnDownloadConfigurationCommand;

        [MenuItem("TT Plugins/Change Enviroment/Production")]
        private static void DownloadProductionConfiguration()
        {
            if (OnDownloadConfigurationCommand != null)
            {
                OnDownloadConfigurationCommand(PRODUCTION_DOMAIN, TTPEditorUtils.CurrentStore());
            }
            DownloadAppConfigFile(PRODUCTION_APPSDB_DOMAIN);
        }

        [MenuItem("TT Plugins/Change Enviroment/AppTesting")]
        private static void DownloadAppTestingConfiguration()
        {
            if (OnDownloadConfigurationCommand != null)
            {
                OnDownloadConfigurationCommand(APPTESTING_DOMAIN, TTPEditorUtils.CurrentStore());
            }
            DownloadAppConfigFile(APPTESTING_APPSDB_DOMAIN);
        }

        [MenuItem("TT Plugins/Change Enviroment/Staging")]
        private static void DownloadStagingConfiguration()
        {
            if (OnDownloadConfigurationCommand != null)
            {
                OnDownloadConfigurationCommand(STAGING_DOMAIN, TTPEditorUtils.CurrentStore());
            }
            DownloadAppConfigFile(STAGING_APPSDB_DOMAIN);
        }

        [MenuItem("TT Plugins/Change Enviroment/Custom Domain")]
        private static void DownloadCustomServerConfiguration()
        {
            EditorWindow.GetWindow(typeof(PSDKDownloadFromCustomServerWindow));
        }

#if !CRAZY_LABS_CLIK // Menu exists in the CLIK
        #region "Menu - Android Store Selector"

        private static bool IsAndroid() => EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;

        [MenuItem("TT Plugins/Android Store Selector/Switch to Google Play")]
        private static void SetAndroidStoreGooglePlay()
        {
            TTPEditorUtils.RemoveScriptingDefineSymbol(SYMBOL_ANDROID_STORE_AMAZON);
        }
    
        [MenuItem("TT Plugins/Android Store Selector/Switch to Google Play", true)]
        private static bool ValidateSetAndroidStoreGooglePlay()
        {
            return IsAndroid() && TTPEditorUtils.IsScriptingDefineSymbolExists(SYMBOL_ANDROID_STORE_AMAZON);
        }
    
        [MenuItem("TT Plugins/Android Store Selector/Switch to Amazon Appstore")]
        private static void SetAndroidStoreAmazonAppstore()
        {
            TTPEditorUtils.AddScriptingDefineSymbol(SYMBOL_ANDROID_STORE_AMAZON);
        }
    
        [MenuItem("TT Plugins/Android Store Selector/Switch to Amazon Appstore", true)]
        private static bool ValidateSetAndroidStoreAmazonAppstore()
        {
            return IsAndroid() && !TTPEditorUtils.IsScriptingDefineSymbolExists(SYMBOL_ANDROID_STORE_AMAZON);
        }
    
        #endregion
#endif
        
        public static void DownloadAppConfigFile(string baseUrl)
        {
            string url = baseUrl + "/alm/title/" + PlayerSettings.applicationIdentifier;
            TTPEditorUtils.DownloadStringToFile(url, APP_CONFIG_PATH, true);
        }

        public static void DownloadConfigurations(string url, string store)
        {
            if (OnDownloadConfigurationCommand != null)
            {
                OnDownloadConfigurationCommand(url, store);
            }
            else
            {
                Debug.Log("DownloadConfigurations:: OnDownloadConfigurationCommand are null");
            }
        }

        public static bool DownloadConfiguration(string url, string fileName)
        {
            if(TTPEditorUtils.DownloadStringToFile(url, CONFIGURATIONS_PATH + "/" + fileName + ".json"))
            {
                return true;
            }
            else
            {
                EditorUtility.DisplayDialog("Failed to Download Configuration", "Failed to download configuration from the following url - " + url, "OK");
            }
            return false;
        }

        public static bool DownloadConfiguration(string url, string fileName, out string configurationOutput)
        {
            if (TTPEditorUtils.DownloadStringToFile(url, CONFIGURATIONS_PATH + "/" + fileName + ".json", out configurationOutput))
            {
                return true;
            }
            else
            {
                EditorUtility.DisplayDialog("Failed to Download Configuration", "Failed to download configuration from the following url - " + url, "OK");
            }
            return false;
        }
    }

    public class PSDKDownloadFromCustomServerWindow : EditorWindow
    {
        private static string url; //static to retain the domain for next use

        void OnGUI()
        {
            GUILayout.Label("Select a custom domain:", EditorStyles.boldLabel);
            url = EditorGUILayout.TextField("Domain", url);
            EditorGUILayout.Space();
            if (GUILayout.Button("Download"))
            {
                TTPMenu.DownloadConfigurations(url, TTPEditorUtils.CurrentStore());
                this.Close();
            }
        }
    }

    public class PSDKPreProcess : IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get
            {
                return 0;
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            bool batchMode = false;
            string domain = "";
            string store = "";
            for (int i = 0; i < args.Length; i++)
            {
                Debug.Log("ARG " + i + ": " + args[i]);
                switch (args[i])
                {
                    case "-configEnv":
                        domain = args[i + 1];
                        batchMode = true;
                        break;
                    case "-store":
                        store = args[i + 1];
                        break;
                }
            }

            if (batchMode)
            {
                if (!domain.StartsWith("-", StringComparison.InvariantCultureIgnoreCase))
                {
                    Debug.Log("TTPMenu: detected batch mode configEnv. will download configurations. env - " + domain + " store - " + store);
                    TTPMenu.DownloadConfigurations(MakeUrl(domain), store);
                }
                else
                {
                    Debug.Log("TTPMenu: detected batch mode configEnv, but env is not mentioned. param after configEnv - " + domain);
                }
            }
        }

        private string MakeUrl(string domain)
        {
            string url = "";
            if(domain != null)
            {
                if (domain.Contains("://"))
                {
                    url = domain.Substring(domain.IndexOf("://", StringComparison.InvariantCultureIgnoreCase) + 3);
                    if (url.EndsWith("/", StringComparison.InvariantCultureIgnoreCase))
                    {
                        url = url.Substring(0, url.Length - 2);
                    }
                }
                else
                {
                    url = domain;
                }
                url = "http://" + url;
            }
            return url;
        }
    }
}
