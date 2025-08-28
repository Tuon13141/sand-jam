using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Google;
using Tabtale.TTPlugins;
using TTPlugins.DependenciesFile;
using TTPlugins.PomFile;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using UnityEngine;

// ReSharper disable InconsistentNaming

public class CLIKConfiguration : EditorWindow
{
    private class Constants
    {
        public const string CONFIG_FN_ANALYTICS = "analytics";
        public const string CONFIG_FN_APPSFLYER = "appsFlyer";
        public const string CONFIG_FN_ADJUST = "adjust";
        public const string CONFIG_FN_BANNERS = "banners";
        public const string CONFIG_FN_CRASHTOOL = "crashMonitoringTool";
        public const string CONFIG_FN_ELEPHANT = "elephant";
        public const string CONFIG_FN_GLOBAL = "global";
        public const string CONFIG_FN_INTERSTITIALS = "interstitials";
        public const string CONFIG_FN_POPUPSMGR = "popupsMgr";
        public const string CONFIG_FN_PRIVACY_SETTINGS = "privacySettings";
        public const string CONFIG_FN_RV = "rewardedAds";
        public const string CONFIG_FN_RV_INTER = "rewardedInterstitials";
        public const string CONFIG_FN_RATEUS = "rateUs";
        public const string CONFIG_FN_OPENADS = "openads";
        public const string CONFIG_FN_PROMOTION = "promotion";
        public const string CONFIG_FN_FACEBOOK = "facebook";
        public const string CONFIG_FN_BILLING = "billing";

        public const string PACAKAGES_ANALYTICS = "analytics";
        public const string PACAKAGES_APPSFLYER = "appsflyer";
        public const string PACAKAGES_ADJUST = "adjust";
        public const string PACAKAGES_BANNERS = "banners";
        public const string PACAKAGES_CRASHTOOL = "crashtool";
        public const string PACAKAGES_ELEPHANT = "elephant";
        public const string PACAKAGES_GLOBAL = "global";
        public const string PACAKAGES_INTERSTITIALS = "interstitials";
        public const string PACAKAGES_POPUPSMGR = "popupsmgr";
        public const string PACAKAGES_PRIVACY_SETTINGS = "privacysettings";
        public const string PACAKAGES_RV = "rewardedads";
        public const string PACAKAGES_RV_INTER = "rewardedinterstitials";
        public const string PACAKAGES_RATEUS = "rateus";
        public const string PACAKAGES_OPENADS = "openads";
        public const string PACAKAGES_PROMOTION = "promotion";
        public const string PACAKAGES_BILLING = "billing";
        public const string PACAKAGES_FACEBOOK = "facebook";

        public const string CONFIG_KEY_INCLUDED = "included";

        public const string CONFIG_KEY_FIREBASE = "firebase";
        public const string CONFIG_KEY_FIREBASE_ECHO_MODE = "firebaseEchoMode";
        public const string CONFIG_KEY_FIREBASE_GOOGLE_ID = "googleAppId";
        public const string CONFIG_KEY_FIREBASE_SENDER_ID = "senderId";
        public const string CONFIG_KEY_FIREBASE_CLIENT_ID = "clientId";
        public const string CONFIG_KEY_FIREBASE_DB_URL = "databaseURL";
        public const string CONFIG_KEY_FIREBASE_STORAGE_BUCKET = "storageBucket";
        public const string CONFIG_KEY_FIREBASE_API_KEY = "apiKey";
        public const string CONFIG_KEY_FIREBASE_PROJECT_ID = "projectId";

        public const string CONFIG_KEY_APPSFLYER = "appsFlyerKey";
        
        public const string CONFIG_TOKEN_ADJUST = "adjustToken";
        public const string CONFIG_DEBUG_ADJUST = "adjustDebug";

        public const string CONFIG_KEY_HOCKEYAPP = "hockeyAppKey";

        public const string CONFIG_KEY_SERVER_DOMAIN = "serverDomain";

        public const string CONFIG_KEY_BANNERS_ALIGN_TO_TOP = "alignToTop";
        public const string CONFIG_KEY_BANNERS_AD_DISPLAY_TIME = "adDisplayTime";
        public const string CONFIG_KEY_BANNERS_ADMOB_KEY = "bannersAdMobKey";
        public const string CONFIG_KEY_BANNERS_HOUSEADS_SERVER_DOMAIN = "houseAdsServerDomain";

        public const string CONFIG_KEY_GLOBAL_BUNDLE_ID = "bundleId";
        public const string CONFIG_KEY_GLOBAL_TEST_MODE = "testMode";
        public const string CONFIG_KEY_GLOBAL_STORE = "store";
        public const string CONFIG_KEY_GLOBAL_GAME_ENGINE = "gameEngine";
        public const string CONFIG_KEY_GLOBAL_AUDIENCE_MODE = "audienceModeBuildOnly";
        public const string CONFIG_KEY_GLOBAL_ORIENTATION = "orientation";
        public const string CONFIG_KEY_GLOBAL_APPBUILDCONFIG = "appBuildConfig";
        public const string CONFIG_KEY_GLOBAL_ADMOB = "admob";
        public const string CONFIG_KEY_GLOBAL_IS_ADMOB = "isAdMob";
        public const string CONFIG_KEY_GLOBAL_APPLICATION = "application";
        public const string CONFIG_KEY_GLOBAL_CONVERSION_MODEL_TYPE = "conversionModelType";

        public const string CONFIG_KEY_INTERSTITIALS = "interstitialsAdMobKey";

        public const string CONFIG_KEY_RV = "rewardedAdsAdMobKey";
        public const string CONFIG_KEY_RV_INTER = "rewardedInterAdMobKey";

        public const string CONFIG_KEY_OPENADS = "appOpenAdMobKey";

        public const string CONFIG_KEY_POPUPMGR_POPUP_INTERVALS_BY_SESSION = "popupsIntervalsBySession";
        public const string CONFIG_KEY_POPUPMGR_POPUP_TIME_BETWEEN_RV_AND_INTER_BY_SESSION = "timeBetweenRvAndInterBySession";
        public const string CONFIG_KEY_POPUPMGR_GAME_TIME_TO_FIRST_POPUP_BY_SESSION = "gameTimeToFirstPopupBySession";
        public const string CONFIG_KEY_POPUPMGR_SESSION_TIME_TO_FIRST_POPUP_BY_SESSION = "sessionTimeToFirstPopupBySession";
        public const string CONFIG_KEY_POPUPMGR_RESET_POPUP_TIMER_ON_APP_OPEN_BY_SESSION = "resetPopupTimerOnAppOpenBySession";
        public const string CONFIG_KEY_POPUPMGR_FIRST_POPUP_AT_SESSION = "firstPopupAtSession";
        public const string CONFIG_KEY_POPUPMGR_LEVEL_TO_FIRST_POPUP = "levelToFirstPopup";
        public const string CONFIG_KEY_POPUPMGR_CACHE_ADS_ON_SHOW = "cacheAdsOnShow";
        public const string CONFIG_KEY_POPUPMGR_APP_OPEN_FROM_BG = "appOpenBackFromBackground";

        public const string CONFIG_KEY_PRIVACY_SETTINGS_CONSENT_FORM_VERSION = "consentFormVersion";
        public const string CONFIG_KEY_PRIVACY_SETTINGS_CONSENT_FORM_URL = "consentFormURL";
        public const string CONFIG_KEY_PRIVACY_SETTINGS_URL = "privacySettingsURL";
        public const string CONFIG_KEY_PRIVACY_SETTINGS_USE_TTP_POPUPS = "useTTPGDPRPopups";

        public const string CONFIG_KEY_RATE_US_ICON_URL = "iconUrl";
        public const string CONFIG_KEY_RATE_US_COOLDOWN = "coolDown";

        public const string ANDROID_MANIFEST_FILE_PATH = "Assets/Plugins/Android/AndroidManifest.xml";
        public const string ANDROID_MANIFEST_FILE_PATH_BACKUP = "Assets/Plugins/Android/_AndroidManifest.xml";
        
        public const string ANDROID_MANIFEST_RES_FILE_PATH = "Assets/Plugins/Android/AndroidConfigurations.androidlib/res/values/google-services.xml";
        public const string ANDROID_MANIFEST_STRINGS_FILE_PATH = "Assets/Plugins/Android/AndroidConfigurations.androidlib/res/values/strings.xml";

        public const string ANDROID_MANIFEST_RES_PATH = "//resources";

        public const string GOOGLE_SERVICES_JSON_PATH = "Assets/StreamingAssets/google-services.json";

        public const string RES_NAME_ADMOB_APP_ID = "admob_app_id";
        public const string RES_NAME_FIREBASE_DB = "firebase_database_url";
        public const string RES_NAME_SENDER_ID = "gcm_defaultSenderId";
        public const string RES_NAME_STORAGE_BUCKET = "google_storage_bucket";
        public const string RES_NAME_PROJECT_ID = "project_id";
        public const string RES_NAME_API_KEY = "google_api_key";
        public const string RES_NAME_APP_ID = "google_app_id";
        public const string RES_NAME_CLIENT_ID = "client_id";
        public const string META_APP_ID = "facebook_application_id";
        public const string FB_APP_ID = "FacebookAppID";
        public const string FB_CLIENT_TOKEN = "FacebookClientToken";
        public const string FB_APP_NAME = "FacebookDisplayName";
        public const string FB_AUTO_LOG_EVENTS_TOKEN = "FacebookAutoLogEvents";

        public const string TEST_APP_BUNDLE_1 = "com.tabtaleint.ttplugins";
        public const string TEST_APP_BUNDLE_2 = "com.sunstorm.ttplugins";
        public const string TEST_APP_BUNDLE_3 = "com.tabtaleint.ttpluginsclik";
        public const string TEST_APP_BUNDLE_AMAZON = "com.tabtaleint.ttplugins.amazon";
        public const string ALIEN_APP_BUNDLE = "com.multicastgames.venomSurvive";
        public const string APPLOVIN_PROD_KEY = "TREvWeSbneklepMTdxWL5KCqUD57xezP4CIarlBcOwM1kiVMe0hkLvTq7dy3HwSL6mxyV7Tu1wwlcP5FQo-nhW";
        public const string APPLOVIN_QA_KEY = "yRHC8kgWwG5S4lOh7Dx_pZB2iEBLVWMSzde5MKbGahifQ6MTKIT7tk9ZzLvTsFwptZvDuVTTBB8cHU9bohkeQu";
        public const string APPLOVIN_ALIEN_KEY = "xeKHWZnfbclWCQbfyiTKoIQQrcRNbQR7-7cnL4ebXxJDP3JeC_TO4xwNZ83PWctXDE9EFzSmOIHNLZLO1TSL_x";

        public const string PLAYER_PREFS_KEY_FIRST_CONFIGURATION = "CLIK-firstTimeConfiguration";
#if UNITY_2020_1_OR_NEWER
        public const string REQUIRED_UNITY_VERSION = "2020.1.14f1";
#else
        public const string REQUIRED_UNITY_VERSION = "2019.3.15f1";
#endif

        public const string GOOGLEPLAY_CONFIG_PREFIX = "gp";
        public const string IOS_CONFIG_PREFIX = "ios";
        public const string AMAZON_CONFIG_PREFIX = "amazon";

#if TTP_ANDROID_STORE_AMAZON
        public const string ANDROID_CONFIG_PREFIX = AMAZON_CONFIG_PREFIX;
#else
        public const string ANDROID_CONFIG_PREFIX = GOOGLEPLAY_CONFIG_PREFIX;
#endif

        public const string ANDROID_STORE_NAME = "google";
        public const string IOS_STORE_NAME = "apple";
        public const string AMAZON_STORE_NAME = "amazon";

        public const string SYMBOL_ANDROID_STORE_AMAZON = "TTP_ANDROID_STORE_AMAZON";
    }

    private static PlatformConfiguration _configurationAndroid = new PlatformConfiguration(Constants.GOOGLEPLAY_CONFIG_PREFIX);
    private static PlatformConfiguration _configurationIOS = new PlatformConfiguration(Constants.IOS_CONFIG_PREFIX);
    private static PlatformConfiguration _configurationAmazon = new PlatformConfiguration(Constants.AMAZON_CONFIG_PREFIX);
    
    private static PlatformConfiguration activeAndroidConfiguration
    {
        get
        {
#if TTP_ANDROID_STORE_AMAZON
            return _configurationAmazon;
#else
            return _configurationAndroid;
#endif
        }
    }
    
    static int CompareVersion(string currentVersion, string requiredVersion)
    {
        var currentVersionSplitted = currentVersion.ToLower().Split('.', 'f');
        var requiredVersionSplited = requiredVersion.ToLower().Split('.', 'f');
        var len                    = Math.Min(currentVersionSplitted.Length, requiredVersionSplited.Length);
        for (int i = 0; i < len; i++)
        {
            var ver1 = int.Parse(currentVersionSplitted[i]);
            var ver2 = int.Parse(requiredVersionSplited[i]);
            if (ver1 < ver2)
                return -1;
            else if (ver1 > ver2)
                return 1;
        }

        return 0;
    }

    private static void ResolveAndroidDependencies()
    {
        if (!IsAndroid()) return;
        RemoveAndroidDependenciesResolverXml();
        ProcessGradleTemplatesFiles();
        CreateJarDependencies(activeAndroidConfiguration.globalConfig.isAdMob, activeAndroidConfiguration.analyticsConfig.firebaseEchoMode);
    }
    
    [MenuItem("CLIK/Configure...")]
    private static void Init()
    {
        if (CompareVersion(Application.unityVersion, Constants.REQUIRED_UNITY_VERSION) < 0)
        {
            Debug.LogError(
                $"Current Unity version ({Application.unityVersion}) is lower than required ({Constants.REQUIRED_UNITY_VERSION})");
        }


        var firstTimeConfiguration = PlayerPrefs.GetInt(Constants.PLAYER_PREFS_KEY_FIRST_CONFIGURATION, 0);

        if (firstTimeConfiguration == 0 || !GlobalConfigFileExists())
        {
            if (!EditorUtility.DisplayDialog("CLIK Configuration", "Thanks for installing CLIK! To begin, " +
                                                                   "please choose the zip file that was provided by your publishing manager",
                "Choose ZIP", "Cancel"))
            {
                return;
            }

            if (!UnzipAndLoadConfiguration())
            {
                return;
            }
        }

        // Get existing open window or if none, make a new one:
        var window = GetWindow<CLIKConfiguration>(true, "CLIK Configuration");
        window.minSize = new Vector2(900f, 600f);
        window.Show();
    }
    
    #region "Menu - Android Store Selector"
    
    [MenuItem("CLIK/Android Store Selector/Switch to Google Play")]
    private static void SetAndroidStoreGooglePlay()
    {
        RemoveSymbol(Constants.SYMBOL_ANDROID_STORE_AMAZON);
        ResolveAndroidDependencies();
    }
    
    [MenuItem("CLIK/Android Store Selector/Switch to Google Play", true)]
    private static bool ValidateSetAndroidStoreGooglePlay()
    {
        return IsAndroid() && isSymbolExists(Constants.SYMBOL_ANDROID_STORE_AMAZON);
    }
    
    [MenuItem("CLIK/Android Store Selector/Switch to Amazon Appstore")]
    private static void SetAndroidStoreAmazonAppstore()
    {
        AddSymbol(Constants.SYMBOL_ANDROID_STORE_AMAZON);
        ResolveAndroidDependencies();
    }
    
    [MenuItem("CLIK/Android Store Selector/Switch to Amazon Appstore", true)]
    private static bool ValidateSetAndroidStoreAmazonAppstore()
    {
        return IsAndroid() && !isSymbolExists(Constants.SYMBOL_ANDROID_STORE_AMAZON);
    }
    
 #endregion
    
    [MenuItem("CLIK/Developer Mode/Set Developer Mode On")]
    private static void SetDeveloperModeOn()
    {
        AddSymbol("TTP_DEV_MODE");
        ToggleMetaFiles(false);
        ChangeManifestStatus(false);
        AddShareToDependencies();
    }
    
    [MenuItem("CLIK/Developer Mode/Set Developer Mode On", true)]
    private static bool ValidateSetDeveloperModeOn()
    {
        return !isSymbolExists("TTP_DEV_MODE");
    }
    
    [MenuItem("CLIK/Developer Mode/Set Developer Mode Off")]
    private static void SetDeveloperModeOff()
    {
        RemoveSymbol("TTP_DEV_MODE");
        ToggleMetaFiles(true);
        ChangeManifestStatus(true);
        RemoveShareFromDependencies();
    }
    
    [MenuItem("CLIK/Developer Mode/Set Developer Mode Off", true)]
    private static bool ValidateSetDeveloperModeOff()
    {
        return isSymbolExists("TTP_DEV_MODE");
    }
    
    [MenuItem("CLIK/Local Configuration/Export To JSON")]
    private static void LocalConfigurationExportToJSON()
    {
        Debug.Log("LocalConfigurationExportToJSON::");

        if (!File.Exists(TTPCore.TTP_LOCAL_CONFIGURATION_PATH))
        {
            Debug.Log("LocalConfigurationExportToJSON::ttpLocalConfiguration doesn't exist");
            return;
        }
        var localConfiguration = AssetDatabase.LoadAssetAtPath<TTPLocalConfigurationScriptableObject>(TTPCore.TTP_LOCAL_CONFIGURATION_PATH);
        var localConfigDic = new Dictionary<string, string>();
        foreach (var configData in localConfiguration.configData)
        {
            Debug.Log("LocalConfigurationExportToJSON::" + configData.name + "=" + configData.value);
            localConfigDic.Add(configData.name, configData.value);
        }

        var jsonData = TTPJson.Serialize(localConfigDic);
        var pathJson = EditorUtility.SaveFilePanel(
            "Save local configuration as JSON", 
            "",
            "localConfiguration.json",
            "json");
        if (pathJson.Length != 0)
        {
            File.WriteAllText(pathJson, jsonData);
        }
    }

    [MenuItem("CLIK/Local Configuration/Create Local Configuration")]
    private static void CreateLocalConfiguration()
    {
        var localConfigurationScriptableObject = ScriptableObject.CreateInstance<TTPLocalConfigurationScriptableObject>();
        var dirpath = CombineWithProjectPath("Tabtale", "TTPlugins", "CLIK", "Resources");
        if (!Directory.Exists(dirpath))
        {
            Directory.CreateDirectory(dirpath);
        }

        AssetDatabase.CreateAsset(localConfigurationScriptableObject, TTPCore.TTP_LOCAL_CONFIGURATION_PATH);
        AssetDatabase.SaveAssets();
    }
    
    internal static bool isSymbolExists(string symbol)
    {
        var symbols = GetSymbols();
        return symbols.Contains(symbol);
    }
    
    internal static void AddSymbol(string symbol) {
        var symbols = GetSymbols();
        if (!symbols.Remove(symbol)) {
            symbols.Add(symbol);
            SetSymbols(symbols);
        }
    }
        
    internal static void RemoveSymbol(string symbol) {
        var symbols = GetSymbols();
        if (symbols.Remove(symbol)) {
            SetSymbols(symbols);
        }
    }
    
    private static List<string> GetSymbols() {
        return PlayerSettings
            .GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup)
            .Split(';')
            .ToList();
    }
    
    private static void SetSymbols(List<string> symbols) {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup,
            string.Join(";", symbols.ToArray()));
    }
    
    private static void ToggleMetaFiles(bool shouldInclude)
    {
        var info = new DirectoryInfo(Path.Combine(Path.Combine("Assets", "Tabtale"), "TTPlugins"));
        bool needToSaveAssets = false;
        foreach (DirectoryInfo directoryInfo in info.GetDirectories())
        {
            if (directoryInfo.ToString().EndsWith("Share"))
            {
                Debug.Log("skipping folder " + directoryInfo);
                continue;
            }
            string iosPluginsPath = Path.Combine(Path.Combine(directoryInfo.ToString(), "Plugins"), "iOS");
            if (Directory.Exists(iosPluginsPath))
            {
                DirectoryInfo innerDirInfo = new DirectoryInfo(iosPluginsPath);
                var fileInfos = innerDirInfo.GetFiles();
                foreach (FileInfo file in fileInfos)
                {
                    if (file.Extension == ".mm" || file.Extension == ".m" || file.Extension == ".h")
                    {
                        needToSaveAssets = true;
                        string relativePath = file.ToString().Replace(Application.dataPath, "Assets");
                        Debug.Log(relativePath);
                        UpdateCompatiblePlatform(relativePath, shouldInclude);
                    }
                }
            }
        }
        
        string iosTestAppPluginsPath = Path.Combine(Path.Combine("Assets", "Plugins"), "iOS");
        if (Directory.Exists(iosTestAppPluginsPath))
        {
            string pathToFile = Path.Combine(iosTestAppPluginsPath, "TestAppUtils.mm");
            if (File.Exists(pathToFile))
            {
                Debug.Log(pathToFile);
                needToSaveAssets = true;
                UpdateCompatiblePlatform(pathToFile, shouldInclude);
            }
        }
        
        if (needToSaveAssets)
        {
            AssetDatabase.SaveAssets();
        }
    }

    private static void ChangeManifestStatus(bool useManifest)
    {
        Debug.Log("CLIKConfiguration::ChangeManifestStatus:useManifest=" + useManifest);
        if (useManifest)
        {
            Debug.Log("CLIKConfiguration::ChangeManifestStatus:manifest backup exists=" + File.Exists(Constants.ANDROID_MANIFEST_FILE_PATH_BACKUP));
            if (File.Exists(Constants.ANDROID_MANIFEST_FILE_PATH_BACKUP))
            {
                File.Move(Constants.ANDROID_MANIFEST_FILE_PATH_BACKUP, Constants.ANDROID_MANIFEST_FILE_PATH);
                AssetDatabase.SaveAssets();
            }
        }
        else
        {
            Debug.Log("CLIKConfiguration::ChangeManifestStatus:manifest exists=" + File.Exists(Constants.ANDROID_MANIFEST_FILE_PATH));
            if (File.Exists(Constants.ANDROID_MANIFEST_FILE_PATH))
            {
                File.Move(Constants.ANDROID_MANIFEST_FILE_PATH, Constants.ANDROID_MANIFEST_FILE_PATH_BACKUP);
                AssetDatabase.SaveAssets();
            }
        }
    }

    private static void AddShareToDependencies()
    {
        var pomData = PomFile.DeserializeFromFile(pomPath);
        Debug.Log("AddShareToDependencies:: pomData = " + pomData);
        var depsFile = DependenciesFile.DeserializeFromFile(depsPath);
        var artifacts = pomData.PomDependencies.Dependencies;
        
        depsFile.AndroidPackages = depsFile.AndroidPackages ?? new AndroidPackages();
        var artifactList = depsFile.AndroidPackages.AndroidPackage;
        
        var artifact = artifacts.FirstOrDefault(p => p.ArtifactId.Equals("TT_Plugins_Share"));
        if (artifact != null)
        {
            var package = new AndroidPackage
            {
                Spec = $"{artifact.GroupId}:{artifact.ArtifactId}:{artifact.Version}"
            };
            Debug.Log("AddShareToDependencies::Found artifact:" + package.Spec);
            artifactList.Add(package);
        }
        else
        {
            Debug.Log("AddShareToDependencies::Can\'t find artifact:TT_Plugins_Share");
        }
        
        depsFile.AndroidPackages.AndroidPackage = artifactList;
        depsFile.SerializeToFile();
    }
    
    private static void RemoveShareFromDependencies()
    {
        var pomData = PomFile.DeserializeFromFile(pomPath);
        Debug.Log("RemoveShareFromDependencies:: pomData = " + pomData);
        var depsFile = DependenciesFile.DeserializeFromFile(depsPath);
        var artifacts = pomData.PomDependencies.Dependencies;

        if (depsFile.AndroidPackages == null)
        {
            Debug.Log("RemoveShareFromDependencies:: no android packages added");
            return;
        }
        
        var artifactList = depsFile.AndroidPackages.AndroidPackage;
        foreach (var androidPackage in artifactList)
        {
            if (androidPackage.Spec.Contains("TT_Plugins_Share"))
            {
                Debug.Log("RemoveShareFromDependencies::TT_Plugins_Share has removed");
                artifactList.Remove(androidPackage);
                break;
            }
        }
        
        depsFile.AndroidPackages.AndroidPackage = artifactList;
        depsFile.SerializeToFile();
    }
    
    private static void UpdateCompatiblePlatform(string relativePath, bool shouldInclude)
    {
        PluginImporter pi = AssetImporter.GetAtPath(relativePath) as PluginImporter;
        pi.ClearSettings();
        if (shouldInclude)
        {
            pi.SetCompatibleWithAnyPlatform(false);
            pi.SetCompatibleWithPlatform(BuildTarget.iOS, true);
        }
        else
        {
            pi.SetExcludeFromAnyPlatform(BuildTarget.iOS, true);
            pi.SetExcludeFromAnyPlatform(BuildTarget.Android, true);
            pi.SetExcludeEditorFromAnyPlatform(true);
        }
    }
    
    private static bool IsUnityPurchasingEnabled()
    {
#if UNITY_PURCHASING
        return true;
#else
        return false;
#endif
    }

    public static bool IsAndroid()
    {
        var isAndroid = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
        return isAndroid;
    }

    private static bool IsBundleIdSpecified(PlatformConfiguration configuration)
    {
        return !string.IsNullOrEmpty(configuration.globalConfig.bundleId);
    }
    
    public static bool IsInvalidPlatfrom(BuildTarget buildTarget)
    { 
        return (!IsBundleIdSpecified(_configurationIOS) && buildTarget == BuildTarget.iOS) ||
             (!IsBundleIdSpecified(activeAndroidConfiguration) && buildTarget == BuildTarget.Android);
    }
    
    GUILayoutOption[] _layoutColumnOption = { GUILayout.Width(300), GUILayout.MaxWidth(300) };
    GUILayoutOption[] _layoutWindowOption = { GUILayout.Width(900), GUILayout.MaxWidth(900) };
    
    private void guiAppIdentifierColumn(bool exists, BuildTargetGroup targetGroup, PlatformConfiguration config) {
        EditorGUILayout.BeginVertical(_layoutColumnOption);
        if (exists)
        {
            var currentId = PlayerSettings.GetApplicationIdentifier(targetGroup);
            if (config.globalConfig.bundleId != currentId)
            {
                GUILayout.Label("Application Id does not match Configuration Bundle Id. Current Id = " + currentId,
                    _redLabel);
                if (GUILayout.Button("Change Application Id"))
                {
                    PlayerSettings.SetApplicationIdentifier(targetGroup, config.globalConfig.bundleId);
                }
            }
            else
            {
                GUILayout.Label("Bundle Id is correct");
            }
        }
        else
        {
            GUILayout.Label("");
        }
        EditorGUILayout.EndVertical();
    }

    private void guiPopUpMgrColumn(PlatformConfiguration config)
    {
        EditorGUILayout.BeginVertical(_layoutColumnOption);
        if (config.popUpMgrConfig.included)
        {
            config.popUpMgrConfig.popupsInterval = EditorGUILayout.LongField("Time Between Popups (sec)", config.popUpMgrConfig.popupsInterval);
            config.popUpMgrConfig.timeBetweenRvAndInter = EditorGUILayout.LongField("Time Between RV and Inter (sec)", config.popUpMgrConfig.timeBetweenRvAndInter);
            config.popUpMgrConfig.gameTimeToFirstPopup = EditorGUILayout.LongField("Game time to first popup (sec)", config.popUpMgrConfig.gameTimeToFirstPopup);
            config.popUpMgrConfig.sessionTimeToFirstPopup = EditorGUILayout.LongField("Session time to first popup (sec)", config.popUpMgrConfig.sessionTimeToFirstPopup);
            config.popUpMgrConfig.firstPopupAtSession = EditorGUILayout.LongField("First popup in session", config.popUpMgrConfig.firstPopupAtSession);
            config.popUpMgrConfig.levelToFirstPopup = EditorGUILayout.LongField("Level to first popup", config.popUpMgrConfig.levelToFirstPopup);
            config.popUpMgrConfig.resetPopupTimerOnAppOpen = EditorGUILayout.Toggle("Reset timer on App Open", config.popUpMgrConfig.resetPopupTimerOnAppOpen);
            EditorGUILayout.Separator();
            if (GUILayout.Button("Save Pop Up Mgr Configuration"))
            {
                config.SaveConfigurations();
            }
            EditorGUILayout.Separator();
        }
        else
        {
            GUILayout.Label("");
        }
        EditorGUILayout.EndVertical();
    }

    private void guiServicesColumn(bool exists, PlatformConfiguration config)
    {
        if (exists)
        {
            EditorGUILayout.BeginVertical(_layoutColumnOption);
            IndicateConfiguration("Analytics", config.analyticsConfig.IsValid(), config.analyticsConfig.included);
            IndicateConfiguration("Appsflyer", config.appsflyerConfig.IsValid(), config.appsflyerConfig.included);
            IndicateConfiguration("Adjust", config.adjustConfig.IsValid(), config.adjustConfig.included);
            IndicateConfiguration("Crash Tool", config.crashToolConfig.IsValid(), config.crashToolConfig.included);
            IndicateConfiguration("Banners", config.globalConfig.IsValid() && config.bannersConfig.IsValid(), config.bannersConfig.included);
            IndicateConfiguration("Interstitials", config.globalConfig.IsValid() && config.interstitialsConfig.IsValid(), config.interstitialsConfig.included);
            IndicateConfiguration("Rewarded Ads", config.globalConfig.IsValid() && config.rewardedAdsConfig.IsValid(), config.rewardedAdsConfig.included);
            IndicateConfiguration("Rewarded Interstitials", config.globalConfig.IsValid() && config.rewardedInterConfig.IsValid(), config.rewardedInterConfig.included);
            IndicateConfiguration("Open Ads", config.globalConfig.IsValid() && config.openAdsConfig.IsValid(), config.openAdsConfig.included);
            IndicateConfiguration("Stand", true, config.promotionConfig.included);
            IndicateConfiguration("Rate Us", true, config.rateUsConfig.included);
            IndicateConfiguration("Privacy Settings", true, config.privacySettingsConfig.included);
            IndicateConfiguration("Billing", true, config.billingConfig.included);
            EditorGUILayout.EndVertical();
        }
        else
        {
            GUILayout.Label("");
        }
    }
    private void OnGUI()
    {
        var androidConfigExists = IsBundleIdSpecified(_configurationAndroid);
        var iosConfigExists = IsBundleIdSpecified(_configurationIOS);
        var amazonConfigExists = IsBundleIdSpecified(_configurationAmazon);
        var androidMode = _configurationAndroid.globalConfig.testMode ? "Test" : "Production";
        var iosMode = _configurationIOS.globalConfig.testMode ? "Test" : "Production";
        var amazonMode = _configurationAmazon.globalConfig.testMode ? "Test" : "Production";
        
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Android", _guiStyleBoldLabel, _layoutColumnOption);
        EditorGUILayout.LabelField("iOS", _guiStyleBoldLabel, _layoutColumnOption);
        EditorGUILayout.LabelField("Amazon", _guiStyleBoldLabel, _layoutColumnOption);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Mode: " + (androidConfigExists ? androidMode: "NOT CONFIGURED"), _layoutColumnOption);
        GUILayout.Label("Mode: " + (iosConfigExists ? iosMode: "NOT CONFIGURED"), _layoutColumnOption);
        GUILayout.Label("Mode: " + (amazonConfigExists ? iosMode: "NOT CONFIGURED"), _layoutColumnOption);
        EditorGUILayout.EndHorizontal();
        
        if (CompareVersion(Application.unityVersion, Constants.REQUIRED_UNITY_VERSION) < 0)
            GUILayout.Label($"Current Unity version ({Application.unityVersion}) is lower than required ({Constants.REQUIRED_UNITY_VERSION})", _redLabel);
        
        EditorGUILayout.BeginHorizontal();
        guiAppIdentifierColumn(androidConfigExists, BuildTargetGroup.Android, _configurationAndroid);
        guiAppIdentifierColumn(iosConfigExists, BuildTargetGroup.iOS, _configurationIOS);
        guiAppIdentifierColumn(amazonConfigExists, BuildTargetGroup.Android, _configurationAmazon);
        EditorGUILayout.EndHorizontal();

        // Validation
        if (androidConfigExists || amazonConfigExists)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(_layoutColumnOption);
            if (androidConfigExists)
            {
                ShowValidationResults();
            }
            GUILayout.Label("");
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical(_layoutColumnOption);
            GUILayout.Label("");
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical(_layoutColumnOption);
            if (amazonConfigExists)
            {
                ShowValidationResults();
            }
            GUILayout.Label("");
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        
        // Pop Up Manager
        if (_configurationAndroid.popUpMgrConfig.included || _configurationIOS.popUpMgrConfig.included || _configurationAmazon.popUpMgrConfig.included)
        {
            EditorGUILayout.BeginHorizontal(_layoutWindowOption);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Pop Up Manager", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.BeginHorizontal();
        guiPopUpMgrColumn(_configurationAndroid);
        guiPopUpMgrColumn(_configurationIOS);
        guiPopUpMgrColumn(_configurationAmazon);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        // Services
        EditorGUILayout.BeginHorizontal();
        guiServicesColumn(androidConfigExists, _configurationAndroid);
        guiServicesColumn(iosConfigExists, _configurationIOS);
        guiServicesColumn(iosConfigExists, _configurationAmazon);
        EditorGUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Load Configuration", _layoutWindowOption))
        {
            UnzipAndLoadConfiguration();
        }

        GUILayout.Space(10);
    }

    
    private void ShowValidationResults()
    {
        var message = "";

        if (!SettingsValidator.ValidateIcons())
            message += "- You are not using adaptive icons for the Android application!\n";

        if (!SettingsValidator.ValidateGradleTemplates())
            message += "- You are not using Custom Base Gradle Template! Please enable this option in Project Settings.\n";

        if (!SettingsValidator.ValidateTargetArchitecture())
            message += "- You are using wrong Android target architecture(s)! Please use ARM 64.\n";

        if (!SettingsValidator.ValidateTargetMinSdkVersion())
            message += "- You are using wrong minimum API level. Please use API level 23.\n";

        if (!SettingsValidator.ValidateTargetSdkVersion())
            message += "- You are using wrong target API level. Please use API level 31.\n";

        if (!SettingsValidator.ValidateBuildAppBundleSetting())
            message += "- You are not using 'Build App Bundle (Google Play)' option!\n";

        if (message.Length != 0)
            GUILayout.Label(message, _redLabel, _layoutColumnOption);
    }
    

    private void GuiLine(int i_height = 1)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, i_height);
        rect.height = i_height;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
    }

    private interface IConfig
    {
        Dictionary<string, object> ToDict();
        string GetServiceName();
        void LoadFromFile(string platformCode);
    }
    
    [Serializable]
    private class FacebookConfig : IConfig
    {
        public bool included;
        public string fbAppID;
        public string fbAppName;
        public string fbClientID;
        
        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>()
            {
                {"fbAppID", fbAppID},
                {"fbAppName", fbAppName},
                {"fbClientID", fbClientID}
            };
        }

        public string GetServiceName()
        {
            return Constants.CONFIG_FN_FACEBOOK;
        }

        public void LoadFromFile(string platformCode = null)
        {
            LoadConfigFromFile(this, platformCode);
        }
    }

    private class BillingConfig : IConfig
    {
        public bool included = false;
        public string json;


        public string GetServiceName()
        {
            return Constants.CONFIG_FN_BILLING;
        }

        public void LoadFromFile(string platformCode)
        {
            if(TryReadConfigFile(platformCode, GetServiceName(), out var jsonOutput)){
                json = jsonOutput;
                if (TTPJson.Deserialize(json) is Dictionary<string, object> dict)
                {
                    if(dict.ContainsKey(Constants.CONFIG_KEY_INCLUDED) && dict[Constants.CONFIG_KEY_INCLUDED] is bool && (bool)dict[Constants.CONFIG_KEY_INCLUDED])
                    {
                        included = true;
                    }
                }


            }
        }

        public Dictionary<string, object> ToDict()
        {
            return TTPJson.Deserialize(json) as Dictionary<string, object>;
        }
    }

    private class GlobalConfig : IConfig
    {
        public string admobAppId;
        public string bundleId;
        public bool testMode;
        public string store;
        public string conversionModelType = "A";
        public bool isAdMob = true;

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>()
            {
                {Constants.CONFIG_KEY_GLOBAL_BUNDLE_ID, bundleId},
                {Constants.CONFIG_KEY_GLOBAL_IS_ADMOB, isAdMob},
                {Constants.CONFIG_KEY_GLOBAL_TEST_MODE, testMode},
                {Constants.CONFIG_KEY_GLOBAL_STORE, store},
                {Constants.CONFIG_KEY_GLOBAL_GAME_ENGINE, "unity"},
                {Constants.CONFIG_KEY_GLOBAL_AUDIENCE_MODE, "non-children"},
                {Constants.CONFIG_KEY_GLOBAL_ORIENTATION, "portrait"},
                {Constants.CONFIG_KEY_GLOBAL_CONVERSION_MODEL_TYPE, conversionModelType},
                {
                    Constants.CONFIG_KEY_GLOBAL_APPBUILDCONFIG, new Dictionary<string, object>()
                    {
                        {
                            Constants.CONFIG_KEY_GLOBAL_ADMOB, new Dictionary<string, object>()
                            {
                                {Constants.CONFIG_KEY_GLOBAL_APPLICATION, admobAppId}
                            }
                        }
                    }
                }
            };
        }

        public string GetServiceName()
        {
            return Constants.CONFIG_FN_GLOBAL;
        }


        public void LoadFromFile(string platformCode = null)
        {
            if (TryReadConfigFile(platformCode, GetServiceName(), out var json))
            {
                Debug.Log("LoadFromFile:: " + GetServiceName() + " json = " + json);
                Deserialize(json);
            }
            else
            {
                Debug.Log("LoadFromFile:: " + GetServiceName() + " json is empty");
            }
#if UNITY_IOS
            if (!string.IsNullOrEmpty(conversionModelType))
            {
                var conversionRulesUrl =
 "http://promo-images.ttpsdk.info/conversionJS/"+conversionModelType+"/conversion.js";
                TTPEditorUtils.DownloadStringToFile(conversionRulesUrl, "Assets/StreamingAssets/ttp/conversion/conversion.js");
            }
#endif
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(admobAppId);
        }

        private void Deserialize(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                if (TTPJson.Deserialize(json) is Dictionary<string, object> dict)
                {
                    if (dict.ContainsKey(Constants.CONFIG_KEY_GLOBAL_STORE) &&
                        dict[Constants.CONFIG_KEY_GLOBAL_STORE] is string ttpStore)
                    {
                        store = ttpStore;
                    }

                    if (dict.ContainsKey(Constants.CONFIG_KEY_GLOBAL_BUNDLE_ID) &&
                        dict[Constants.CONFIG_KEY_GLOBAL_BUNDLE_ID] is string bid)
                    {
                        bundleId = bid;
                    }

                    if (dict.ContainsKey(Constants.CONFIG_KEY_GLOBAL_TEST_MODE) &&
                        dict[Constants.CONFIG_KEY_GLOBAL_TEST_MODE] is bool tm)
                    {
                        testMode = tm;
                    }

                    if (dict.ContainsKey(Constants.CONFIG_KEY_GLOBAL_IS_ADMOB) &&
                        dict[Constants.CONFIG_KEY_GLOBAL_IS_ADMOB] is bool is_admob)
                    {
                        isAdMob = is_admob;
                    }

                    if (dict.ContainsKey(Constants.CONFIG_KEY_GLOBAL_APPBUILDCONFIG) &&
                        dict[Constants.CONFIG_KEY_GLOBAL_APPBUILDCONFIG] is Dictionary<string, object>
                            appBuildConfigDict &&
                        appBuildConfigDict.ContainsKey(Constants.CONFIG_KEY_GLOBAL_ADMOB) &&
                        appBuildConfigDict[Constants.CONFIG_KEY_GLOBAL_ADMOB] is Dictionary<string, object> admobDict &&
                        admobDict.ContainsKey(Constants.CONFIG_KEY_GLOBAL_APPLICATION) &&
                        admobDict[Constants.CONFIG_KEY_GLOBAL_APPLICATION] is string applicationStr)
                    {
                        admobAppId = applicationStr;
                    }

                    if (dict.ContainsKey(Constants.CONFIG_KEY_GLOBAL_CONVERSION_MODEL_TYPE) &&
                        dict[Constants.CONFIG_KEY_GLOBAL_CONVERSION_MODEL_TYPE] is string conversionModelTypeVal)
                    {
                        conversionModelType = conversionModelTypeVal;
                    }
                }
            }
        }
    }

    [Serializable]
    private class AppsflyerConfig : IConfig
    {
        public bool included;
        public string appsFlyerKey;

        private bool _isValid;

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>()
            {
                {Constants.CONFIG_KEY_APPSFLYER, appsFlyerKey}
            };
        }

        public string GetServiceName()
        {
            return Constants.CONFIG_FN_APPSFLYER;
        }

        public void LoadFromFile(string platformCode)
        {
            LoadConfigFromFile(this, platformCode);
            _isValid = !string.IsNullOrEmpty(appsFlyerKey);
        }

        public bool IsValid()
        {
            return _isValid;
        }
    }

    [Serializable]
    private class AdjustConfig : IConfig
    {
        public bool included;
        public string adjustToken;
        public bool adjustDebug;

        private bool _isValid;

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>()
            {
                {Constants.CONFIG_TOKEN_ADJUST, adjustToken},
                {Constants.CONFIG_DEBUG_ADJUST, adjustDebug}
            };
        }

        public string GetServiceName()
        {
            return Constants.CONFIG_FN_ADJUST;
        }

        public void LoadFromFile(string platformCode)
        {
            LoadConfigFromFile(this, platformCode);
            _isValid = !string.IsNullOrEmpty(adjustToken);
        }

        public bool IsValid()
        {
            return _isValid;
        }
    }
    
    [Serializable]
    private class CrashToolConfig : IConfig
    {
        public bool included;
        public string hockeyAppKey;

        private bool _isValid;

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>()
            {
                {Constants.CONFIG_KEY_HOCKEYAPP, hockeyAppKey}
            };
        }

        public string GetServiceName()
        {
            return Constants.CONFIG_FN_CRASHTOOL;
        }


        public void LoadFromFile(string platformCode)
        {
            LoadConfigFromFile(this, platformCode);
            _isValid = !string.IsNullOrEmpty(hockeyAppKey);
        }

        public bool IsValid()
        {
            return _isValid;
        }
    }

    [Serializable]
    private class ElephantConfig : IConfig
    {
        public bool included;
        private string serverDomain;

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>()
            {
                {Constants.CONFIG_KEY_SERVER_DOMAIN, serverDomain}
            };
        }

        public string GetServiceName()
        {
            return Constants.CONFIG_FN_ELEPHANT;
        }

        public void LoadFromFile(string platformCode)
        {
            LoadConfigFromFile(this, platformCode);
        }
    }

    private class PopUpMgrConfig : IConfig
    {
        public bool included;
        public long sessionTimeToFirstPopup;
        public long gameTimeToFirstPopup;
        public long levelToFirstPopup;
        public long firstPopupAtSession;
        public bool resetPopupTimerOnAppOpen;
        public long popupsInterval;
        public long timeBetweenRvAndInter;
        public bool cacheAdsOnShow;
        public bool appOpenFromBG;
        private Dictionary<string, object> popupsIntervalsBySession;
        private Dictionary<string, object> gameTimeToFirstPopupBySession;
        private Dictionary<string, object> sessionTimeToFirstPopupBySession;
        private Dictionary<string, object> resetPopupTimerOnRVBySession;
        private Dictionary<string, object> resetPopupTimerOnAppOpenBySession;

        public Dictionary<string, object> ToDict()
        {
            var dic = new Dictionary<string, object>
            {
                {Constants.CONFIG_KEY_INCLUDED, included},
                {Constants.CONFIG_KEY_POPUPMGR_POPUP_INTERVALS_BY_SESSION, new Dictionary<string,List<object>>(){ {"1", new List<object>(){popupsInterval}}}},
                {Constants.CONFIG_KEY_POPUPMGR_POPUP_TIME_BETWEEN_RV_AND_INTER_BY_SESSION, new Dictionary<string,long>(){ {"1", timeBetweenRvAndInter}}},
                {Constants.CONFIG_KEY_POPUPMGR_GAME_TIME_TO_FIRST_POPUP_BY_SESSION, new Dictionary<string,long>(){ {"1", gameTimeToFirstPopup}}},
                {Constants.CONFIG_KEY_POPUPMGR_SESSION_TIME_TO_FIRST_POPUP_BY_SESSION, new Dictionary<string,long>(){ {"1", sessionTimeToFirstPopup}}},
                {Constants.CONFIG_KEY_POPUPMGR_RESET_POPUP_TIMER_ON_APP_OPEN_BY_SESSION, new Dictionary<string,bool>(){ {"1", resetPopupTimerOnAppOpen}}},
                {Constants.CONFIG_KEY_POPUPMGR_FIRST_POPUP_AT_SESSION, firstPopupAtSession},
                {Constants.CONFIG_KEY_POPUPMGR_LEVEL_TO_FIRST_POPUP, levelToFirstPopup},
                {Constants.CONFIG_KEY_POPUPMGR_CACHE_ADS_ON_SHOW, cacheAdsOnShow},
                {Constants.CONFIG_KEY_POPUPMGR_APP_OPEN_FROM_BG, appOpenFromBG},
                {"byCountry", new Dictionary<string, object>
                {
                    {"Default", new Dictionary<string, object>
                    {
                        {Constants.CONFIG_KEY_POPUPMGR_POPUP_INTERVALS_BY_SESSION, new Dictionary<string,List<object>>(){ {"1", new List<object>(){popupsInterval}}}},
                        {Constants.CONFIG_KEY_POPUPMGR_POPUP_TIME_BETWEEN_RV_AND_INTER_BY_SESSION, new Dictionary<string,long>(){ {"1", timeBetweenRvAndInter}}},
                        {Constants.CONFIG_KEY_POPUPMGR_GAME_TIME_TO_FIRST_POPUP_BY_SESSION, new Dictionary<string,long>(){ {"1", gameTimeToFirstPopup}}},
                        {Constants.CONFIG_KEY_POPUPMGR_SESSION_TIME_TO_FIRST_POPUP_BY_SESSION, new Dictionary<string,long>(){ {"1", sessionTimeToFirstPopup}}},
                        {Constants.CONFIG_KEY_POPUPMGR_RESET_POPUP_TIMER_ON_APP_OPEN_BY_SESSION, new Dictionary<string,bool>(){ {"1", resetPopupTimerOnAppOpen}}},
                        {Constants.CONFIG_KEY_POPUPMGR_FIRST_POPUP_AT_SESSION, firstPopupAtSession},
                        {Constants.CONFIG_KEY_POPUPMGR_LEVEL_TO_FIRST_POPUP, levelToFirstPopup},
                        {Constants.CONFIG_KEY_POPUPMGR_CACHE_ADS_ON_SHOW, cacheAdsOnShow},
                        {Constants.CONFIG_KEY_POPUPMGR_APP_OPEN_FROM_BG, appOpenFromBG},
                    }}
                }}
            };
            return dic;
        }

        public string GetServiceName()
        {
            return Constants.CONFIG_FN_POPUPSMGR;
        }

        public void LoadFromFile(string platformCode)
        {
            if (TryReadConfigFile(platformCode, GetServiceName(), out var json))
            {
                Deserialize(json);
            }
        }

        private object GetInnerObject(Dictionary<string, object> dict, string key)
        {
            if (dict.ContainsKey(key) &&
                dict[key] is Dictionary<string, object>
                    innerDict)
            {
                if (innerDict.ContainsKey("1"))
                {
                    return innerDict["1"];
                }
            }

            return null;
        }

        private int GetInnerInt(Dictionary<string, object> dict, string key)
        {
            return Convert.ToInt32(GetInnerObject(dict, key));
        }

        private int GetInnerIntFromArray(Dictionary<string, object> dict, string key)
        {
            if (GetInnerObject(dict, key) is List<object> arr)
            {
                return Convert.ToInt32(arr[0]);
            }
            else
            {
                return 0;
            }
        }

        private bool GetInnerBool(Dictionary<string, object> dict, string key)
        {
            return Convert.ToBoolean(GetInnerObject(dict, key));
        }

        private void Deserialize(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                if (TTPJson.Deserialize(json) is Dictionary<string, object> dict)
                {
                    included = dict.ContainsKey(Constants.CONFIG_KEY_INCLUDED) && (bool)dict[Constants.CONFIG_KEY_INCLUDED];
                    popupsInterval = GetInnerIntFromArray(dict, Constants.CONFIG_KEY_POPUPMGR_POPUP_INTERVALS_BY_SESSION);
                    timeBetweenRvAndInter = GetInnerInt(dict, Constants.CONFIG_KEY_POPUPMGR_POPUP_TIME_BETWEEN_RV_AND_INTER_BY_SESSION);
                    gameTimeToFirstPopup = GetInnerInt(dict, Constants.CONFIG_KEY_POPUPMGR_GAME_TIME_TO_FIRST_POPUP_BY_SESSION);
                    sessionTimeToFirstPopup = GetInnerInt(dict, Constants.CONFIG_KEY_POPUPMGR_SESSION_TIME_TO_FIRST_POPUP_BY_SESSION);
                    resetPopupTimerOnAppOpen = GetInnerBool(dict, Constants.CONFIG_KEY_POPUPMGR_RESET_POPUP_TIMER_ON_APP_OPEN_BY_SESSION);
                    firstPopupAtSession = dict.ContainsKey(Constants.CONFIG_KEY_POPUPMGR_FIRST_POPUP_AT_SESSION)
                        ? (long) dict[Constants.CONFIG_KEY_POPUPMGR_FIRST_POPUP_AT_SESSION]
                        : 0;
                    levelToFirstPopup = dict.ContainsKey(Constants.CONFIG_KEY_POPUPMGR_LEVEL_TO_FIRST_POPUP)
                        ? (long) dict[Constants.CONFIG_KEY_POPUPMGR_LEVEL_TO_FIRST_POPUP]
                        : 0;
                    cacheAdsOnShow = dict.ContainsKey(Constants.CONFIG_KEY_POPUPMGR_CACHE_ADS_ON_SHOW)
                        ? (bool) dict[Constants.CONFIG_KEY_POPUPMGR_CACHE_ADS_ON_SHOW]
                        : false;
                    appOpenFromBG = dict.ContainsKey(Constants.CONFIG_KEY_POPUPMGR_APP_OPEN_FROM_BG)
                        ? (bool) dict[Constants.CONFIG_KEY_POPUPMGR_APP_OPEN_FROM_BG]
                        : false;
                }
            }
        }
    }

    [Serializable]
    private class PrivacySettingsConfig : IConfig
    {
        public bool included;
        public string consentFormVersion;
        public string consentFormURL;
        public string privacySettingsURL;
        public bool useTTPGDPRPopups;

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>()
            {
                {Constants.CONFIG_KEY_PRIVACY_SETTINGS_CONSENT_FORM_VERSION, consentFormVersion},
                {Constants.CONFIG_KEY_PRIVACY_SETTINGS_CONSENT_FORM_URL, consentFormURL},
                {Constants.CONFIG_KEY_PRIVACY_SETTINGS_URL, privacySettingsURL},
                {Constants.CONFIG_KEY_PRIVACY_SETTINGS_USE_TTP_POPUPS, useTTPGDPRPopups}
            };
        }

        public string GetServiceName()
        {
            return Constants.CONFIG_FN_PRIVACY_SETTINGS;
        }

        public void LoadFromFile(string platformCode)
        {
            LoadConfigFromFile(this, platformCode);
        }
    }

    private class AnalyticsConfig : IConfig
    {
        public bool included;
        public bool firebaseEchoMode;
        public string googleAppId;
        public string senderId;
        public string clientId;
        public string storageBucket;
        public string apiKey;
        public string projectId;

        private bool _isValid;

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>
            {
                {
                    Constants.CONFIG_KEY_FIREBASE,
                    new Dictionary<string, object>()
                    {
                        {Constants.CONFIG_KEY_FIREBASE_GOOGLE_ID, googleAppId},
                        {Constants.CONFIG_KEY_FIREBASE_SENDER_ID, senderId},
                        {Constants.CONFIG_KEY_FIREBASE_CLIENT_ID, clientId},
                        {Constants.CONFIG_KEY_FIREBASE_STORAGE_BUCKET, storageBucket},
                        {Constants.CONFIG_KEY_FIREBASE_API_KEY, apiKey},
                        {Constants.CONFIG_KEY_FIREBASE_PROJECT_ID, projectId},
                        {Constants.CONFIG_KEY_FIREBASE_ECHO_MODE, firebaseEchoMode}
                    }
                }
            };
        }

        public string GetServiceName()
        {
            return Constants.CONFIG_FN_ANALYTICS;
        }

        public void LoadFromFile(string platformCode = null)
        {
            if (TryReadConfigFile(platformCode, GetServiceName(), out var json))
            {
                Deserialize(json);
            }
        }

        private void Deserialize(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                if (TTPJson.Deserialize(json) is Dictionary<string, object> dict &&
                    dict.ContainsKey(Constants.CONFIG_KEY_FIREBASE) &&
                    dict[Constants.CONFIG_KEY_FIREBASE] is Dictionary<string, object> firebaseDict)
                {
                    included = dict.ContainsKey(Constants.CONFIG_KEY_INCLUDED) &&
                               (bool) dict[Constants.CONFIG_KEY_INCLUDED];
                    firebaseEchoMode = dict.ContainsKey(Constants.CONFIG_KEY_FIREBASE_ECHO_MODE) &&
                                       (bool) dict[Constants.CONFIG_KEY_FIREBASE_ECHO_MODE];
                    googleAppId = firebaseDict.ContainsKey(Constants.CONFIG_KEY_FIREBASE_GOOGLE_ID)
                        ? firebaseDict[Constants.CONFIG_KEY_FIREBASE_GOOGLE_ID] as string
                        : "";
                    senderId = firebaseDict.ContainsKey(Constants.CONFIG_KEY_FIREBASE_SENDER_ID)
                        ? firebaseDict[Constants.CONFIG_KEY_FIREBASE_SENDER_ID] as string
                        : "";
                    clientId = firebaseDict.ContainsKey(Constants.CONFIG_KEY_FIREBASE_CLIENT_ID)
                        ? firebaseDict[Constants.CONFIG_KEY_FIREBASE_CLIENT_ID] as string
                        : "";
                    storageBucket = firebaseDict.ContainsKey(Constants.CONFIG_KEY_FIREBASE_STORAGE_BUCKET)
                        ? firebaseDict[Constants.CONFIG_KEY_FIREBASE_STORAGE_BUCKET] as string
                        : "";
                    apiKey = firebaseDict.ContainsKey(Constants.CONFIG_KEY_FIREBASE_API_KEY)
                        ? firebaseDict[Constants.CONFIG_KEY_FIREBASE_API_KEY] as string
                        : "";
                    projectId = firebaseDict.ContainsKey(Constants.CONFIG_KEY_FIREBASE_PROJECT_ID)
                        ? firebaseDict[Constants.CONFIG_KEY_FIREBASE_PROJECT_ID] as string
                        : "";

                    _isValid = !string.IsNullOrEmpty(googleAppId) &&
                               !string.IsNullOrEmpty(senderId) &&
                               !string.IsNullOrEmpty(clientId) &&
                               !string.IsNullOrEmpty(storageBucket) &&
                               !string.IsNullOrEmpty(apiKey) &&
                               !string.IsNullOrEmpty(projectId);
                }
            }
        }

        public bool IsValid()
        {
            return _isValid;
        }
    }

    [Serializable]
    private class RateUsConfig : IConfig
    {
        public bool included;
        public string iconUrl = "";
        public int coolDown = 3;

        private bool _isValid;

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>()
            {
                {Constants.CONFIG_KEY_RATE_US_ICON_URL, iconUrl},
                {Constants.CONFIG_KEY_RATE_US_COOLDOWN, coolDown}
            };
        }

        public string GetServiceName()
        {
            return Constants.CONFIG_FN_RATEUS;
        }


        public void LoadFromFile(string platformCode)
        {
            LoadConfigFromFile(this, platformCode);
            _isValid = !string.IsNullOrEmpty(iconUrl);
            if (_isValid)
            {
                var extenstion =
                    iconUrl.Substring(iconUrl.LastIndexOf(".", System.StringComparison.InvariantCultureIgnoreCase) + 1);
                TTPEditorUtils.DownloadFile(iconUrl, "Assets/StreamingAssets/ttp/rateus/game_icon." + extenstion);
            }
        }
    }

    [Serializable]
    private class BannersConfig : IConfig
    {
        public bool included;
        public bool alignToTop;
        public long adDisplayTime;
        public string bannersAdMobKey;
        public string houseAdsServerDomain;

        private bool _isValid;

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>()
            {
                {Constants.CONFIG_KEY_BANNERS_ALIGN_TO_TOP, alignToTop},
                {Constants.CONFIG_KEY_BANNERS_AD_DISPLAY_TIME, adDisplayTime},
                {Constants.CONFIG_KEY_BANNERS_ADMOB_KEY, bannersAdMobKey},
                {Constants.CONFIG_KEY_BANNERS_HOUSEADS_SERVER_DOMAIN, houseAdsServerDomain},
            };
        }

        public string GetServiceName()
        {
            return Constants.CONFIG_FN_BANNERS;
        }


        public void LoadFromFile(string platformCode)
        {
            LoadConfigFromFile(this, platformCode);
            _isValid = !string.IsNullOrEmpty(bannersAdMobKey);
        }

        public bool IsValid()
        {
            return _isValid;
        }
    }

    [Serializable]
    private class OpenAdsConfig : IConfig
    {
        public bool included;
        public string appOpenAdMobKey;

        private bool _isValid;

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>()
            {
                {Constants.CONFIG_KEY_OPENADS, appOpenAdMobKey}
            };
        }

        public string GetServiceName()
        {
            return Constants.CONFIG_FN_OPENADS;
        }

        public void LoadFromFile(string platformCode)
        {
            LoadConfigFromFile(this, platformCode);
            _isValid = !string.IsNullOrEmpty(appOpenAdMobKey);
        }

        public bool IsValid()
        {
            return _isValid;
        }
    }

    [Serializable]
    private class InterstitialsConfig : IConfig
    {
        public bool included;
        public string interstitialsAdMobKey;

        private bool _isValid;

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>()
            {
                {Constants.CONFIG_KEY_INTERSTITIALS, interstitialsAdMobKey}
            };
        }

        public string GetServiceName()
        {
            return Constants.CONFIG_FN_INTERSTITIALS;
        }


        public void LoadFromFile(string platformCode)
        {
            LoadConfigFromFile(this, platformCode);
            _isValid = !string.IsNullOrEmpty(interstitialsAdMobKey);
        }

        public bool IsValid()
        {
            return _isValid;
        }
    }


    [Serializable]
    private class RewardedAdsConfig : IConfig
    {
        public bool included;
        public string rewardedAdsAdMobKey;

        private bool _isValid;

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>()
            {
                {Constants.CONFIG_KEY_RV, rewardedAdsAdMobKey}
            };
        }

        public string GetServiceName()
        {
            return Constants.CONFIG_FN_RV;
        }


        public void LoadFromFile(string platformCode)
        {
            LoadConfigFromFile(this, platformCode);
            _isValid = !string.IsNullOrEmpty(rewardedAdsAdMobKey);
        }

        public bool IsValid()
        {
            return _isValid;
        }
    }

    [Serializable]
    private class RewardedInterConfig : IConfig
    {
        public bool included;
        public string rewardedInterAdMobKey;

        private bool _isValid;

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>()
            {
                {Constants.CONFIG_KEY_RV_INTER, rewardedInterAdMobKey}
            };
        }

        public string GetServiceName()
        {
            return Constants.CONFIG_FN_RV_INTER;
        }


        public void LoadFromFile(string platformCode)
        {
            LoadConfigFromFile(this, platformCode);
            _isValid = !string.IsNullOrEmpty(rewardedInterAdMobKey);
        }

        public bool IsValid()
        {
            return _isValid;
        }
    }

    [Serializable]
    private class PromotionConfig : IConfig
    {
        public bool included;
        public string serverDomain;
        private bool _isValid;

        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>()
            {
                {Constants.CONFIG_KEY_SERVER_DOMAIN, serverDomain}
            };
        }

        public string GetServiceName()
        {
            return Constants.CONFIG_FN_PROMOTION;
        }

        public void LoadFromFile(string platformCode)
        {
            LoadConfigFromFile(this, platformCode);
            _isValid = !string.IsNullOrEmpty(serverDomain);
        }

        public bool IsValid()
        {
            return _isValid;
        }
    }


    private class PlatformConfiguration
    {
        public AppsflyerConfig appsflyerConfig = new AppsflyerConfig();
        public AdjustConfig adjustConfig = new AdjustConfig();
        public AnalyticsConfig analyticsConfig = new AnalyticsConfig();
        public BannersConfig bannersConfig = new BannersConfig();
        public InterstitialsConfig interstitialsConfig = new InterstitialsConfig();
        public CrashToolConfig crashToolConfig = new CrashToolConfig();
        public RewardedAdsConfig rewardedAdsConfig = new RewardedAdsConfig();
        public RewardedInterConfig rewardedInterConfig = new RewardedInterConfig();
        public PopUpMgrConfig popUpMgrConfig = new PopUpMgrConfig();
        public PrivacySettingsConfig privacySettingsConfig = new PrivacySettingsConfig();
        public RateUsConfig rateUsConfig = new RateUsConfig();
        public OpenAdsConfig openAdsConfig = new OpenAdsConfig();
        public PromotionConfig promotionConfig = new PromotionConfig();
        public GlobalConfig globalConfig = new GlobalConfig();
        public FacebookConfig facebookConfig = new FacebookConfig();
        public BillingConfig billingConfig = new BillingConfig();
        
        private List<IConfig> _configs;
        public string _platformCode;

        public PlatformConfiguration(string platformCode)
        {
            _platformCode = platformCode;
            _configs = new List<IConfig>();
            _configs.Add(globalConfig);
            _configs.Add(appsflyerConfig);
            _configs.Add(adjustConfig);
            _configs.Add(analyticsConfig);
            _configs.Add(bannersConfig);
            _configs.Add(interstitialsConfig);
            _configs.Add(crashToolConfig);
            _configs.Add(rewardedAdsConfig);
            _configs.Add(rewardedInterConfig);
            _configs.Add(popUpMgrConfig);
            _configs.Add(privacySettingsConfig);
            _configs.Add(rateUsConfig);
            _configs.Add(openAdsConfig);
            _configs.Add(promotionConfig);
            _configs.Add(facebookConfig);
            _configs.Add(billingConfig);
        }
        
        public void LoadConfigurationsFromFile()
        {
            foreach (var config in _configs)
            {
                config.LoadFromFile(_platformCode);
            }
        }
        
        public void SaveConfigurations()
        {
            SaveConfigurationToFile(popUpMgrConfig);
            UpdateAndroidRes(globalConfig, analyticsConfig);
            UpdateAndroidStringsXml();
        }

        private void SaveConfigurationToFile(IConfig config)
        {
            var json = TTPJson.Serialize(config.ToDict());
            Debug.Log("SaveConfigurationToFile:json=" + json);
            var fp = GetTTPTemplateConfigPath(_platformCode, config.GetServiceName());
            if (File.Exists(fp))
            {
                File.Delete(fp);
            }

            if (!Directory.Exists(GetTTPConfigPath()))
            {
                Directory.CreateDirectory(CombineWithProjectPath(GetTTPConfigPath()));
            }

            File.WriteAllText(fp, json);
        }
    }


    private static void LoadConfigFromFile(IConfig config, string platformCode)
    {
        if (TryReadConfigFile(platformCode, config.GetServiceName(), out var json))
        {
            if (!string.IsNullOrEmpty(json))
            {
                EditorJsonUtility.FromJsonOverwrite(json, config);
            }
        }
    }

    private static string GetApplovinKeyForBundle()
    {
        Debug.Log("GetApplovinKeyForBundle:: ");
        var isTestApp = Application.identifier == Constants.TEST_APP_BUNDLE_1 ||
                        Application.identifier == Constants.TEST_APP_BUNDLE_2 ||
                        Application.identifier == Constants.TEST_APP_BUNDLE_3 ||
                        Application.identifier == Constants.TEST_APP_BUNDLE_AMAZON;
        var isAlienGame = Application.identifier == Constants.ALIEN_APP_BUNDLE;
        if (isAlienGame)
        {
            return Constants.APPLOVIN_ALIEN_KEY;
        }
        else
        {
            return isTestApp ? Constants.APPLOVIN_QA_KEY : Constants.APPLOVIN_PROD_KEY;
        }
    }

    private static void UpdateAndroidRes(GlobalConfig globalConfig, AnalyticsConfig analyticsConfig)
    {
        Debug.Log("UpdateAndroidRes:: " + (globalConfig != null ? globalConfig.admobAppId : "globalConfig null"));
        Debug.Log("UpdateAndroidRes:: " +
                  (analyticsConfig != null ? analyticsConfig.senderId : "analyticsConfig null"));
        var dic = new Dictionary<string, string>();
        List<string> removeDic = null;
        if (!string.IsNullOrEmpty(globalConfig.admobAppId))
        {
            dic[Constants.RES_NAME_ADMOB_APP_ID] = globalConfig.admobAppId;
        }

        if (analyticsConfig.firebaseEchoMode)
        {
            removeDic = new List<string>()
            {
                Constants.RES_NAME_SENDER_ID,
                Constants.RES_NAME_STORAGE_BUCKET,
                Constants.RES_NAME_PROJECT_ID,
                Constants.RES_NAME_API_KEY,
                Constants.RES_NAME_APP_ID,
                Constants.RES_NAME_CLIENT_ID,
                Constants.RES_NAME_FIREBASE_DB
            };
        }
        else if (analyticsConfig.IsValid())
        {
            dic[Constants.RES_NAME_SENDER_ID] = analyticsConfig.senderId;
            dic[Constants.RES_NAME_STORAGE_BUCKET] = analyticsConfig.storageBucket;
            dic[Constants.RES_NAME_PROJECT_ID] = analyticsConfig.projectId;
            dic[Constants.RES_NAME_API_KEY] = analyticsConfig.apiKey;
            dic[Constants.RES_NAME_APP_ID] = analyticsConfig.googleAppId;
            dic[Constants.RES_NAME_CLIENT_ID] = analyticsConfig.clientId;
        }

        var xmlDoc = new XmlDocument();
        xmlDoc.Load(Constants.ANDROID_MANIFEST_RES_FILE_PATH);
        var resPath = xmlDoc.SelectSingleNode(Constants.ANDROID_MANIFEST_RES_PATH);
        
        if (removeDic != null)
        {
            foreach (string itemToRemove in removeDic)
            {
                var xmlNodeToRemove = GetExistingXmlNode(xmlDoc, resPath, itemToRemove);
                if (xmlNodeToRemove != null)
                {
                    var parentNode = xmlNodeToRemove.ParentNode;
                    if(parentNode != null)
                        parentNode.RemoveChild(xmlNodeToRemove);
                }
            }
        }
        
        foreach (var kvp in dic)
        {
            var node = GetXMlNode(xmlDoc, resPath, kvp.Key);
            var nameAttribute = xmlDoc.CreateAttribute("name");
            nameAttribute.Value = kvp.Key;
            var translatableAttribute = xmlDoc.CreateAttribute("translatable");
            translatableAttribute.Value = "false";
            node.Attributes.Append(nameAttribute);
            node.Attributes.Append(translatableAttribute);
            node.InnerText = kvp.Value;
            resPath.AppendChild(node);
        }

        Debug.Log("UpdateAndroidRes:: Save");
        xmlDoc.Save(Constants.ANDROID_MANIFEST_RES_FILE_PATH);
        Debug.Log("UpdateAndroidRes:: Saved");
    }

    private static void UpdateAndroidStringsXml()
    {
        if (activeAndroidConfiguration.facebookConfig.included)
        {
            Debug.Log("UpdateAndroidStringsXml:: facebook is included, will read keys");
            ReadAndUpdateFacebookSdkKeys();
        }

        var metaAppId = ReadMetaAppId();
        if (string.IsNullOrEmpty(metaAppId))
        {
            Debug.Log("UpdateAndroidStringsXml:: meta app id is null or empty");
            return;
        }

        if (!File.Exists(Constants.ANDROID_MANIFEST_STRINGS_FILE_PATH))
        {
            using (FileStream fs = File.Create(Constants.ANDROID_MANIFEST_STRINGS_FILE_PATH))
            {
                byte[] empty = new UTF8Encoding(true).GetBytes(
                    "<?xml version='1.0' encoding='utf-8'?>\n" + 
                    "<resources>\n</resources>");
                fs.Write(empty, 0, empty.Length);
                Debug.Log("UpdateAndroidStringsXml:: create `strings.xml` file");
            }
        }
        
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(Constants.ANDROID_MANIFEST_STRINGS_FILE_PATH);
        var resPath = xmlDoc.SelectSingleNode(Constants.ANDROID_MANIFEST_RES_PATH);
        
        var node = GetXMlNode(xmlDoc, resPath, Constants.META_APP_ID);
        var nameAttribute = xmlDoc.CreateAttribute("name");
        nameAttribute.Value = Constants.META_APP_ID;
        var translatableAttribute = xmlDoc.CreateAttribute("translatable");
        translatableAttribute.Value = "false";
        node.Attributes.Append(nameAttribute);
        node.Attributes.Append(translatableAttribute);
        node.InnerText = metaAppId;
        resPath.AppendChild(node);

        Debug.Log("UpdateAndroidStringsXml:: Save with meta app id = " + metaAppId);
        xmlDoc.Save(Constants.ANDROID_MANIFEST_STRINGS_FILE_PATH);
        Debug.Log("UpdateAndroidStringsXml:: Saved");
    }

    private static void ReadAndUpdateFacebookSdkKeys()
    {
        var facebookKeys = ReadFacebookSdkKeys();
        var facebookAutoLogEvents = ReadFacebookAutoLogEvent();

        if (string.IsNullOrEmpty(facebookKeys.Item1)){
            Debug.Log("ReadAndUpdateFacebookSdkKeys:: FacebookAppID null or empty");
            return;
        }

        if (string.IsNullOrEmpty(facebookKeys.Item2)){
            Debug.Log("ReadAndUpdateFacebookSdkKeys:: FacebookDisplayName null or empty");
            return;
        }

        if (string.IsNullOrEmpty(facebookKeys.Item3)){
            Debug.Log("ReadAndUpdateFacebookSdkKeys:: FacebookClientToken null or empty");
            return;
        }

        if (!File.Exists(Constants.ANDROID_MANIFEST_STRINGS_FILE_PATH))
        {
            using (FileStream fs = File.Create(Constants.ANDROID_MANIFEST_STRINGS_FILE_PATH))
            {
                byte[] empty = new UTF8Encoding(true).GetBytes(
                    "<?xml version='1.0' encoding='utf-8'?>\n" + 
                    "<resources>\n</resources>");
                fs.Write(empty, 0, empty.Length);
                Debug.Log("ReadAndUpdateFacebookSdkKeys:: create `strings.xml` file");
            }
        }
        
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(Constants.ANDROID_MANIFEST_STRINGS_FILE_PATH);
        var resPath = xmlDoc.SelectSingleNode(Constants.ANDROID_MANIFEST_RES_PATH);

        resPath.AppendChild(GenerateNode(Constants.FB_APP_ID, facebookKeys.Item1, xmlDoc, resPath));
        resPath.AppendChild(GenerateNode(Constants.FB_APP_NAME, facebookKeys.Item2, xmlDoc, resPath));
        resPath.AppendChild(GenerateNode(Constants.FB_CLIENT_TOKEN, facebookKeys.Item3, xmlDoc, resPath));
        resPath.AppendChild(GenerateNode(Constants.FB_AUTO_LOG_EVENTS_TOKEN, facebookAutoLogEvents == 1 ? "true" : "false", xmlDoc, resPath));

        Debug.Log("ReadAndUpdateFacebookSdkKeys:: Save with fb keys = " + facebookKeys);
        xmlDoc.Save(Constants.ANDROID_MANIFEST_STRINGS_FILE_PATH);
        Debug.Log("ReadAndUpdateFacebookSdkKeys:: Saved");
    }

    private static XmlNode GenerateNode(string name, string val, XmlDocument xmlDoc, XmlNode docNode)
    {
        XmlNode newNode = GetXMlNode(xmlDoc, docNode, name);
        newNode.InnerText = val;
        var nameAttribute = xmlDoc.CreateAttribute("name");
        nameAttribute.Value = name;
        var translatableAttribute = xmlDoc.CreateAttribute("translatable");
        translatableAttribute.Value = "false";
        newNode.Attributes.Append(nameAttribute);
        newNode.Attributes.Append(translatableAttribute);
        return newNode;
    }
    
    private static XmlNode GetExistingXmlNode(XmlDocument xmlDocument, XmlNode xmlNode, string nameAttrVal,
        string type = "string")
    {
        var nodes = xmlNode.SelectNodes(type);
        if (nodes != null)
        {
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes?["name"] != null && node.Attributes["name"].Value == nameAttrVal)
                {
                    return node;
                }
            }
        }

        return null;
    }

    private static XmlNode GetXMlNode(XmlDocument xmlDocument, XmlNode xmlNode, string nameAttrVal,
        string type = "string")
    {
        var existingNode = GetExistingXmlNode(xmlDocument, xmlNode, nameAttrVal, type);
        if (existingNode != null)
        {
            return existingNode;
        }

        return xmlDocument.CreateNode(XmlNodeType.Element, type, null);
    }
    
    private void OnEnable()
    {
        _greenIndicator.alignment = TextAnchor.MiddleRight;
        _greenIndicator.fixedHeight = 25;
        _greenIndicator.padding.right = 10;
        _greenIndicator.padding.top = 5;
        _minusIcon.fixedHeight = 25;
        _minusIcon.alignment = TextAnchor.MiddleRight;
        _minusIcon.padding.right = 10;
        _minusIcon.padding.top = 5;
        _okTexture = GetTexture("Assets/Tabtale/TTPlugins/CLIK/Editor/ok.png");
        _xTexture = GetTexture("Assets/Tabtale/TTPlugins/CLIK/Editor/x.png");
        _minusTexture = GetTexture("Assets/Tabtale/TTPlugins/CLIK/Editor/minus.png");
        _okGuiContent = new GUIContent(_okTexture, "Configured");
        _xGuiContent = new GUIContent(_xTexture, "Not Configured");
        _redLabel.normal.textColor = Color.red;
        _redLabel.padding.left = 10;
        _redLabel.wordWrap = true;
        _greenLabel.normal.textColor = Color.green;
        _greenLabel.padding.left = 10;
        _nonbreakingLabelStyle.wordWrap = false;
        _nonbreakingLabelStyle.normal.textColor = Color.white;
        _nonbreakingLabelStyle.alignment = TextAnchor.MiddleCenter;
        _guiStyleBoldLabel.fontSize = 16;
        _guiStyleBoldLabel.fontStyle = FontStyle.Bold;
        _guiStyleBoldLabel.normal.textColor = Color.white;
        _configurationAndroid.LoadConfigurationsFromFile();
        _configurationIOS.LoadConfigurationsFromFile();
        _configurationAmazon.LoadConfigurationsFromFile();
    }

    private Texture GetTexture(string path)
    {
        Texture t = new Texture2D(1, 1);
        ((Texture2D) t).LoadImage(System.IO.File.ReadAllBytes(path));
        ((Texture2D) t).Apply();
        return t;
    }


    private GUIContent _okGuiContent;
    private GUIContent _xGuiContent;
    private Texture _okTexture;
    private Texture _xTexture;
    private Texture _minusTexture;

    GUIStyle _nonbreakingLabelStyle = new GUIStyle();

    private GUIStyle _redLabel = new GUIStyle();
    private GUIStyle _greenLabel = new GUIStyle();
    private GUIStyle _greenIndicator = new GUIStyle();
    private GUIStyle _minusIcon = new GUIStyle();
    private GUIStyle _guiStyleBoldLabel = new GUIStyle();

    private static bool UnzipAndLoadConfiguration()
    {
        var zipPath = EditorUtility.OpenFilePanel("Choose Configuration Zip", "", "zip");
        return Configure(zipPath);
    }

    public static void LoadConfigurationsFromFiles()
    {
        _configurationAndroid?.LoadConfigurationsFromFile();
        _configurationIOS?.LoadConfigurationsFromFile();
        _configurationAmazon?.LoadConfigurationsFromFile();
    }

    private static void ShowDualConfigurationTransitionWarning()
    {
        Debug.Log("CLIKConfiguration::ShowDualConfigurationTransitionWarning");
        var message = "You have loaded a ZIP that is not compatible with CLIK 4.4+. Please load a dual configuration zip.";
        EditorUtility.DisplayDialog("Warning", message,"Got it");
    }
    
    public static bool Configure(string zipPath)
    {
        IOSResolver.PodToolExecutionViaShellEnabled = false;
        if (string.IsNullOrEmpty(zipPath))
        {
            Debug.LogError("Configure:: Zip does not exist at path - " + zipPath);
            return false;
        }

        if (Directory.Exists(GetTTPTemplateConfigPath()))
        {
            Debug.Log("Configure:: removing file at path - " + GetTTPTemplateConfigPath());
            Directory.Delete(GetTTPTemplateConfigPath(), true);
        }

        PlayerPrefs.SetInt(Constants.PLAYER_PREFS_KEY_FIRST_CONFIGURATION, 1);
        ZipUtil.Unzip(zipPath, GetTTPTemplateConfigPath());
        if (!GlobalConfigFileExists())
        {
            ShowDualConfigurationTransitionWarning();
        }
        _configurationAndroid = new PlatformConfiguration(Constants.GOOGLEPLAY_CONFIG_PREFIX);
        _configurationAndroid.LoadConfigurationsFromFile();
        _configurationIOS = new PlatformConfiguration(Constants.IOS_CONFIG_PREFIX);
        _configurationIOS.LoadConfigurationsFromFile();
        _configurationAmazon = new PlatformConfiguration(Constants.AMAZON_CONFIG_PREFIX);
        _configurationAmazon.LoadConfigurationsFromFile();
        ModulateClik(_configurationAndroid);
        ModulateClik(_configurationIOS);
        ModulateClik(_configurationAmazon);
#if !DISABLE_GRADLE_TEMPLATE_RESET
        ProcessGradleTemplatesFiles();
#endif
        
        CreateJarDependencies(activeAndroidConfiguration.globalConfig.isAdMob, activeAndroidConfiguration.analyticsConfig.firebaseEchoMode);

        return true;
    }
    
    private static string pomPath = "Assets/Tabtale/TTPlugins/TT_Plugins_Android.pom";
    private static string depsPath = "Assets/Editor/TTPDependencies.xml";

    private static void CreateJarDependencies(bool isAdMob, bool firebaseEchoMode)
    {
        Debug.Log("CreateJarDependencies:: " + isAdMob + ", " + firebaseEchoMode);
        var includedServices = CLIKConfiguration.GetInclusionScriptableObject(Constants.ANDROID_CONFIG_PREFIX);
        if (includedServices == null)
        {
            Debug.LogError("CreateJarDependencies:: included services is null! will not build correctly.");
            return;
        }

        var services = new List<string>
        {
            "TT_Plugins_Core", "TT_Plugins_Unity"
        };
        
        if (includedServices.appsFlyer)
        {
            services.Add("TT_Plugins_AppsFlyer");
        }

        if (includedServices.adjust)
        {
            services.Add("TT_Plugins_Adjust");
        }
        
        if (includedServices.facebook) 
        {
            services.Add("TT_Plugins_Facebook");
        }

        if (includedServices.crashTool)
        {
            services.Add("TT_Plugins_CrashTool");
        }

        if (includedServices.privacySettings)
        {
            services.Add("TT_Plugins_Privacy_Settings");
        }

        bool popUpMgrIncluded = false;
        if (includedServices.banners)
        {
            popUpMgrIncluded = true;
            services.Add("TT_Plugins_Banners_Admob");
            services.Add("TT_Plugins_Elephant");
        }

        if (includedServices.interstitials)
        {
            popUpMgrIncluded = true;
            services.Add("TT_Plugins_Interstitials_Admob");
        }

        if (includedServices.rvs)
        {
            popUpMgrIncluded = true;
            services.Add("TT_Plugins_RewardedAds_Admob");
        }

        if (includedServices.openAds)
        {
            popUpMgrIncluded = true;
            services.Add("TT_Plugins_OpenAds");
        }

        if (includedServices.rvInter && isAdMob)
        {
            popUpMgrIncluded = true;
            services.Add("TT_Plugins_RewardedInterstitials");
        }

        if (includedServices.promotion)
        {
            popUpMgrIncluded = true;
            services.Add("TT_Plugins_Promotion");
        }

        if (includedServices.rvs || includedServices.interstitials || includedServices.banners ||
            includedServices.openAds || includedServices.rvInter)
        {
            services.Add("TT_Plugins_ECPM");
        }

        if (popUpMgrIncluded)
        {
            services.Add("TT_Plugins_AdProviders");
            services.Add(isAdMob ? "TT_Plugins_AdManager_Admob" : "TT_Plugins_AdManager_Max");
            services.Add("TT_Plugins_PopupMgr");
        }

        if (includedServices.analytics)
        {
            services.Add("TT_Plugins_Analytics");
            if (firebaseEchoMode)
                services.Add("TT_Plugins_FirebaseEchoAgent");
            else
            {
                services.Add("TT_Plugins_FirebaseAgent");
                services.Add("TT_Plugins_Remote_Config");
            }
        }

        if (includedServices.billing)
        {
            services.Add("TT_Plugins_Billing");
        }

#if TTP_DEV_MODE
        services.Add("TT_Plugins_Share");
#endif
        var pomData = PomFile.DeserializeFromFile(pomPath);
        Debug.Log("CreateJarDependencies:: pomData = " + pomData);
        var depsFile = DependenciesFile.DeserializeFromFile(depsPath);
        var artifacts = pomData.PomDependencies.Dependencies;
        var artifactList = new List<AndroidPackage>();
        foreach (var service in services)
        {
            var artifact = artifacts.FirstOrDefault(p => p.ArtifactId.Equals(service));
            if (artifact != null)
            {
                var package = new AndroidPackage
                {
                    Spec = $"{artifact.GroupId}:{artifact.ArtifactId}:{artifact.Version}"
                };
                Debug.Log("Found artifact:" + package.Spec);
                artifactList.Add(package);
            }
            else
            {
                Debug.Log("Can\'t find artifact:" + service);
            }
        }

        depsFile.AndroidPackages = depsFile.AndroidPackages ?? new AndroidPackages();
        depsFile.AndroidPackages.AndroidPackage = artifactList;
        depsFile.SerializeToFile();
    }

    private static void ProcessGradleTemplatesFiles()
    {
        string baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
        Debug.Log("processGradleTemplatesFiles::baseDir=" + baseDir);
#if UNITY_EDITOR_WIN
        string pathToGradleTemplateFolder = TTPUtils.CombinePaths(new List<string>
        {
            baseDir, "Data", "PlaybackEngines", "AndroidPlayer", "Tools", "GradleTemplates"
        });
#else
        string pathToGradleTemplateFolder = TTPUtils.CombinePaths(new List<string>
        {
            baseDir, "PlaybackEngines", "AndroidPlayer", "Tools", "GradleTemplates"
        });
        if (!Directory.Exists(pathToGradleTemplateFolder))
        {
            pathToGradleTemplateFolder = TTPUtils.CombinePaths(new List<string>
            {
                baseDir, "PlaybackEngines", "AndroidPlayer", "SDK", "cmdline-tools", "2.1", "GradleTemplates"
            });
        }
#endif
        Debug.Log("processGradleTemplatesFiles::pathToGradleTemplateFolder=" + pathToGradleTemplateFolder);
        string pathToBaseProjectTemplate = Path.Combine(pathToGradleTemplateFolder, "baseProjectTemplate.gradle");
        bool baseProjectTemplate = File.Exists(pathToBaseProjectTemplate);
        Debug.Log("processGradleTemplatesFiles::baseProjectTemplate=" + baseProjectTemplate);
        string pathToLauncherTemplate = Path.Combine(pathToGradleTemplateFolder, "launcherTemplate.gradle");
        bool launcherTemplate = File.Exists(pathToLauncherTemplate);
        Debug.Log("processGradleTemplatesFiles::launcherTemplate=" + launcherTemplate);
        string pathToMainTemplate = Path.Combine(pathToGradleTemplateFolder, "mainTemplate.gradle");
        bool mainTemplate = File.Exists(pathToMainTemplate);
        Debug.Log("processGradleTemplatesFiles::mainTemplate=" + mainTemplate);

        if (!baseProjectTemplate || !launcherTemplate || !mainTemplate)
        {
            Debug.LogWarning("ProcessGradleTemplatesFiles:: will not copy templates since one them does not exist - \n" +
                             "Base Template - " + (baseProjectTemplate ? "Exists" : "Does Not Exists") + "\n" +
                             "Launcher Template - " + (launcherTemplate ? "Exists" : "Does Not Exists") + "\n" +
                             "Main Template - " + (mainTemplate ? "Exists" : "Does Not Exists"));
            return;
        }
        
        string destinationPath = "Assets/Plugins/Android";
        string destinationPathToBaseProjectTemplate = Path.Combine(destinationPath, "baseProjectTemplate.gradle");
        bool destinationBaseProjectTemplate = File.Exists(destinationPathToBaseProjectTemplate);
        Debug.Log("processGradleTemplatesFiles::destinationBaseProjectTemplate=" + destinationBaseProjectTemplate);
        string destinationPathToLauncherTemplate = Path.Combine(destinationPath, "launcherTemplate.gradle");
        bool destinationLauncherTemplate = File.Exists(destinationPathToLauncherTemplate);
        Debug.Log("processGradleTemplatesFiles::destinationLauncherTemplate=" + destinationLauncherTemplate);
        string destinationPathToMainTemplate = Path.Combine(destinationPath, "mainTemplate.gradle");
        bool destinationMainTemplate = File.Exists(destinationPathToMainTemplate);
        Debug.Log("processGradleTemplatesFiles::mainTemplate=" + destinationMainTemplate);
        
        // File.Copy(pathToBaseProjectTemplate, destinationPathToBaseProjectTemplate, true);
        File.Copy(pathToLauncherTemplate, destinationPathToLauncherTemplate, true);
        File.Copy(pathToMainTemplate, destinationPathToMainTemplate, true);

        string contentBaseProjectTemplate = File.ReadAllText(destinationPathToBaseProjectTemplate);
        Debug.Log("processGradleTemplatesFiles::contentBaseProjectTemplate=" + contentBaseProjectTemplate);
        
        string contentLauncherTemplate = File.ReadAllText(destinationPathToLauncherTemplate);
        Debug.Log("processGradleTemplatesFiles::contentLauncherTemplate=" + contentLauncherTemplate);
        
        string contentMainTemplate = File.ReadAllText(destinationPathToMainTemplate);
        Debug.Log("processGradleTemplatesFiles::contentMainTemplate=" + contentMainTemplate);
    }

    private void IndicateConfiguration(string labelText, bool valid, bool included)
    {
        if (!included)
            return;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(labelText, EditorStyles.boldLabel);
        GUILayout.Label(valid ? _okGuiContent : _xGuiContent, _greenIndicator);
        EditorGUILayout.EndHorizontal();
    }

    private static void ResetIncludedServices(TTPIncludedServicesScriptableObject includedServices)
    {
        includedServices.analytics = false;
        includedServices.appsFlyer = false;
        includedServices.adjust = false;
        includedServices.crashTool = false;
        includedServices.banners = false;
        includedServices.interstitials = false;
        includedServices.openAds = false;
        includedServices.rvs = false;
        includedServices.rvInter = false;
        includedServices.privacySettings = false;
        includedServices.rateUs = false;
        includedServices.promotion = false;
        includedServices.facebook = false;
        includedServices.billing = false;
    }

    internal static TTPIncludedServicesScriptableObject GetInclusionScriptableObject(string platformCode = null, bool reset = false)
    {
        string path;
        if (string.IsNullOrEmpty(platformCode))
        {
            path = "Assets/Tabtale/TTPlugins/CLIK/Resources/ttpIncludedServices.asset";
        }
        else
        {
            path = "Assets/Tabtale/TTPlugins/CLIK/Resources/ttpIncludedServices_" + platformCode + ".asset";
        }

        if (File.Exists(path))
        {
            Debug.Log("GetInclusionScriptableObject ::  1");
            var includedServices = AssetDatabase.LoadAssetAtPath<TTPIncludedServicesScriptableObject>(path);
            if (reset)
            {
                ResetIncludedServices(includedServices);
            }

            return includedServices;
        }

        Debug.Log("GetInclusionScriptableObject ::  2");
        var includedServicesScriptableObject =
            ScriptableObject.CreateInstance<TTPIncludedServicesScriptableObject>();
        var dirpath = CombineWithProjectPath("Tabtale", "TTPlugins", "CLIK", "Resources");
        if (!Directory.Exists(dirpath))
        {
            Directory.CreateDirectory(dirpath);
        }

        AssetDatabase.CreateAsset(includedServicesScriptableObject, path);
        AssetDatabase.SaveAssets();
        return includedServicesScriptableObject;
    }

    public static string GetTTPConfigPath(string config = null)
    {
        if (config == null)
        {
            return CombineWithProjectPath("StreamingAssets", "ttp", "configurations");
        }

        return CombineWithProjectPath("StreamingAssets", "ttp", "configurations", config + ".json");
    }

    public static string GetTTPTemplateConfigPath(string platform = null, string config = null)
    {
        if (platform == null)
        {
            return CombineWithProjectPath("StreamingAssets", "ttp", "templateconfig");
        }

        if (config == null)
        {
            return CombineWithProjectPath("StreamingAssets", "ttp", "templateconfig", platform);
        }

        return CombineWithProjectPath("StreamingAssets", "ttp", "templateconfig", platform, config + ".json");
    }

    private static string CombineWithProjectPath(params string[] pathComponents)
    {
        var path = Application.dataPath;
        foreach (string c in pathComponents)
        {
            path = Path.Combine(path, c);
        }

        return path;
    }

    private static bool TryReadConfigFile(string platformCode, string configName, out string output)
    {
        var path = GetTTPTemplateConfigPath(platformCode, configName);
        if (!File.Exists(path))
        {
            path = GetTTPConfigPath(configName);
        }
        Debug.Log("TryReadConfigFile:: " + configName + " path = " + path);
        if (File.Exists(path))
        {
            output = File.ReadAllText(path);
            return true;
        }

        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            path = CombineWithProjectPath("Plugins", "Android", "assets", "ttp", "configurations",
                configName + ".json");
            if (File.Exists(path))
            {
                output = File.ReadAllText(path);
                return true;
            }
        }

        output = null;
        return false;
    }

    public static bool GlobalConfigFileExists(string platformCode = null)
    {
        if (platformCode == null)
        {
            return File.Exists(GetTTPTemplateConfigPath(Constants.ANDROID_CONFIG_PREFIX, Constants.CONFIG_FN_GLOBAL)) ||
                   File.Exists(GetTTPTemplateConfigPath(Constants.IOS_CONFIG_PREFIX, Constants.CONFIG_FN_GLOBAL));
        }

        return File.Exists(GetTTPTemplateConfigPath(platformCode, Constants.CONFIG_FN_GLOBAL));
    }

    public static string GetPlatformCode(BuildTarget buildTarget)
    {
        return buildTarget == BuildTarget.Android ? Constants.ANDROID_CONFIG_PREFIX : Constants.IOS_CONFIG_PREFIX;
    }

    //return (fbAppID, fbAppName, fbClientID)
    private static (string, string, string) ReadFacebookSdkKeys()
    {
        Debug.Log("ReadFacebookSdkKeys:: ReadFacebookSdkKeys from facebook.json");

        if (TryReadConfigFile(Constants.ANDROID_CONFIG_PREFIX, "facebook", out var jsonOutput))
        {
            string json = jsonOutput;
            if (TTPJson.Deserialize(json) is Dictionary<string, object> dictionary)
            {
                if (dictionary.ContainsKey("fbAppID") && dictionary["fbAppID"] is string
                    && dictionary.ContainsKey("fbAppName") && dictionary["fbAppName"] is string
                    && dictionary.ContainsKey("fbClientID") && dictionary["fbClientID"] is string)
                {
                    Debug.Log("ReadFacebookSdkKeys:: found in facebook.json: fbAppID: " + dictionary["fbAppID"] + "; fbAppName: " + dictionary["fbAppID"] + "; fbClientID: " + dictionary["fbAppID"] );
                    return (dictionary["fbAppID"] as string,
                        dictionary["fbAppName"] as string,
                        dictionary["fbClientID"] as string);
                }
            }
        }
        return (null, null, null);
    }

    private static int ReadFacebookAutoLogEvent()
    {
        Debug.Log("ReadFacebookAutoLogEvent::");

        if (TryReadConfigFile(Constants.ANDROID_CONFIG_PREFIX, "facebook", out var facebookConfig))
        {
            string facebookConfigJson = facebookConfig;
            if (TTPJson.Deserialize(facebookConfigJson) is Dictionary<string, object> facebookDictionary)
            {
                if (facebookDictionary.ContainsKey("facebookAutoLogAppEventsEnabled") && facebookDictionary["facebookAutoLogAppEventsEnabled"] is bool)
                {
                    Debug.Log("ReadFacebookAutoLogEvent:: found in facebook.json: facebookAutoLogAppEventsEnabled: " +
                              facebookDictionary["facebookAutoLogAppEventsEnabled"]);
                    return ((bool)facebookDictionary["facebookAutoLogAppEventsEnabled"]) ? 1 : 0;
                }
            }
        }

        if (TryReadConfigFile(Constants.ANDROID_CONFIG_PREFIX, "additionalConfig", out var additionalConfig))
        {
            string additionalConfigJson = additionalConfig;
            if (TTPJson.Deserialize(additionalConfigJson) is Dictionary<string, object> additionalDictionary)
            {
                if (additionalDictionary.ContainsKey("facebookAutoLogAppEventsEnabled") && additionalDictionary["facebookAutoLogAppEventsEnabled"] is bool)
                {
                    Debug.Log("ReadFacebookAutoLogEvent:: found in additionalConfig.json: facebookAutoLogAppEventsEnabled: " +
                              additionalDictionary["facebookAutoLogAppEventsEnabled"]);
                    return ((bool)additionalDictionary["facebookAutoLogAppEventsEnabled"]) ? 1 : 0;
                }
            }
        }

        return -1;
    }
    
    private static string ReadMetaAppId() 
    {
        Debug.Log("ReadMetaAppId::");
        var fromGlobal = ReadMetaAppIdFromGlobal();
        if(fromGlobal != null)
            return fromGlobal;
        else return ReadMetaAppIdFromAdditional();
    }

    private static string ReadMetaAppIdFromAdditional() 
    {
        Debug.Log("ReadMetaAppId:: ReadMetaAppIdFromAdditional");

        if (TryReadConfigFile(Constants.ANDROID_CONFIG_PREFIX, "additionalConfig", out var jsonOutput))
        {
            string json = jsonOutput;
            if (TTPJson.Deserialize(json) is Dictionary<string, object> dictionary)
            {
                if (dictionary.ContainsKey("metaAppId") && dictionary["metaAppId"] is string)
                {
                    Debug.Log("ReadMetaAppId:: found in additional: " + dictionary["metaAppId"] as string);

                    return dictionary["metaAppId"] as string;
                }
            }
        }
        return null;
    }

    private static string ReadMetaAppIdFromGlobal() 
    {
        Debug.Log("ReadMetaAppId:: ReadMetaAppIdFromGlobal");

        if (TryReadConfigFile(Constants.ANDROID_CONFIG_PREFIX, "global", out var jsonOutput))
        {
            string json = jsonOutput;
            if (TTPJson.Deserialize(json) is Dictionary<string, object> dictionary)
            {
                if (dictionary.ContainsKey("appBuildConfig") &&
                    dictionary["appBuildConfig"] is Dictionary<string, object> appBuildConfigDict &&
                    appBuildConfigDict.ContainsKey("facebook") &&
                    appBuildConfigDict["facebook"] is Dictionary<string, object> facebookDict &&
                    facebookDict.ContainsKey("metaAppId") &&
                    facebookDict["metaAppId"] is string metaAppId &&
                    !String.IsNullOrEmpty(metaAppId))
                {
                    Debug.Log("ReadMetaAppId:: found in global: " + metaAppId);
                    return metaAppId;
                }
            }
        }
        return null;
    }
    
    private static void SaveInclusionScriptableObject(TTPIncludedServicesScriptableObject objToCpy, string platformCode = null)
    {
        var curObj = GetInclusionScriptableObject(platformCode);
        curObj.analytics = objToCpy.analytics;
        curObj.appsFlyer = objToCpy.appsFlyer;
        curObj.adjust = objToCpy.adjust;
        curObj.crashTool = objToCpy.crashTool;
        curObj.banners = objToCpy.banners;
        curObj.interstitials = objToCpy.interstitials;
        curObj.rvs = objToCpy.rvs;
        curObj.privacySettings = objToCpy.privacySettings;
        curObj.rateUs = objToCpy.rateUs;
        curObj.openAds = objToCpy.openAds;
        curObj.promotion = objToCpy.promotion;
        curObj.facebook = objToCpy.facebook;
        curObj.billing = objToCpy.billing;
        EditorUtility.SetDirty(curObj);
        AssetDatabase.SaveAssets();
        var saved = GetInclusionScriptableObject(platformCode);
        if (saved.privacySettings)
        {
            Debug.Log("SaveInclusionScriptableObject Privacy Settings is included");
        }
        else
        {
            Debug.Log("SaveInclusionScriptableObject No Privacy Settings");
        }
    }

    private static void ModulateClik(PlatformConfiguration _configuration)
    {
        Debug.Log("ModulateClik");
        var ttpIncludedServices = GetInclusionScriptableObject(_configuration._platformCode);
        ttpIncludedServices.appsFlyer = _configuration.appsflyerConfig.included;
        ttpIncludedServices.adjust = _configuration.adjustConfig.included;
        ttpIncludedServices.analytics = _configuration.analyticsConfig.included;
        ttpIncludedServices.crashTool = _configuration.crashToolConfig.included;
        ttpIncludedServices.banners = _configuration.bannersConfig.included;
        ttpIncludedServices.interstitials = _configuration.interstitialsConfig.included;
        ttpIncludedServices.rvs = _configuration.rewardedAdsConfig.included;
        ttpIncludedServices.rvInter = _configuration.rewardedInterConfig.included;
        ttpIncludedServices.privacySettings = _configuration.privacySettingsConfig.included;
        ttpIncludedServices.rateUs = _configuration.rateUsConfig.included;
        ttpIncludedServices.openAds = _configuration.openAdsConfig.included;
        ttpIncludedServices.promotion = _configuration.promotionConfig.included;
        ttpIncludedServices.facebook = _configuration.facebookConfig.included;
        ttpIncludedServices.billing = _configuration.billingConfig.included;

        var dic = new Dictionary<string, bool>()
        {
            {"ttpIncludedServices.appsFlyer", _configuration.appsflyerConfig.included},
            {"ttpIncludedServices.adjust", _configuration.adjustConfig.included},
            {"ttpIncludedServices.analytics", _configuration.analyticsConfig.included},
            {"ttpIncludedServices.crashTool", _configuration.crashToolConfig.included},
            {"ttpIncludedServices.banners", _configuration.bannersConfig.included},
            {"ttpIncludedServices.interstitials", _configuration.interstitialsConfig.included},
            {"ttpIncludedServices.rvs", _configuration.rewardedAdsConfig.included},
            {"ttpIncludedServices.rvInter", _configuration.rewardedInterConfig.included},
            {"ttpIncludedServices.privacySettings", _configuration.privacySettingsConfig.included},
            {"ttpIncludedServices.rateUs", _configuration.rateUsConfig.included},
            {"ttpIncludedServices.openAds", _configuration.openAdsConfig.included},
            {"ttpIncludedServices.promotion", _configuration.promotionConfig.included},
            {"ttpIncludedServices.facebook", _configuration.facebookConfig.included},
            {"ttpIncludedServices.billing", _configuration.billingConfig.included}
        };

        var msg = "";
        foreach (var kvp in dic)
        {
            msg += kvp.Key + "=" + kvp.Value + "\n";
        }

        Debug.Log(msg);
        SaveInclusionScriptableObject(ttpIncludedServices, _configuration._platformCode);
    }

    private static bool IsMediationProviderAdmob(string store)
    {
        Debug.Log("IsMediationProviderAdmob::");
        var appConfigFilePath = Path.Combine(Application.dataPath, "StreamingAssets/app_config.json");
        if (!File.Exists(appConfigFilePath))
        {
            Debug.Log("IsMediationProviderAdmob:: Failed - app_config.json does not exists in path: " 
                + appConfigFilePath + " - will use global config");

            var globalConfig = new GlobalConfig();
            globalConfig.LoadFromFile(store);

            Debug.Log("IsMediationProviderAdmob:: read from global config: " + globalConfig.isAdMob);
            return globalConfig.isAdMob;
        }

        var appsDbJson = File.ReadAllText(appConfigFilePath);
        
        var appConfig = TTPJson.Deserialize(appsDbJson) as IDictionary<string, object>;
        if (appConfig != null && 
            appConfig.ContainsKey(store) && 
            appConfig[store] is IDictionary<string, object> &&
            ((IDictionary<string, object>)appConfig[store]).ContainsKey("mediationProvider") &&
            ((IDictionary<string, object>)appConfig[store])["mediationProvider"] is string)
        {
            var mediationProvider = ((IDictionary<string, object>)appConfig[store])["mediationProvider"] as string;
            Debug.Log("IsMediationProviderAdmob:: " + mediationProvider);
            return mediationProvider == "adMob";
        }
        return true;
    }
    
    public static void BuilderDetermineIncludedServices(string serverDomain, string bundleId)
    {
        Debug.Log("BuilderDetermineIncludedServices:: " + serverDomain);
        if (!serverDomain.StartsWith("http"))
        {
            serverDomain = "http://" + serverDomain;
        }
        
        var store = IsAndroid() 
            ? bundleId.ToLower().Contains("amazon") ? Constants.AMAZON_STORE_NAME : Constants.ANDROID_STORE_NAME
            : Constants.IOS_STORE_NAME;
        
        var url = (serverDomain ?? "http://dashboard.ttpsdk.info") + "/clik-packages/" + store + "/" + bundleId;
        Debug.Log("BuilderDetermineIncludedServices::url=" + url);
        string resStr = null;
        if (!IsAndroid())
        {
            IOSResolver.PodToolExecutionViaShellEnabled = false;
        }
        
        if (TTPEditorUtils.DownloadStringToFile(url, "Assets/StreamingAssets/clik-packages.json", out resStr))
        {
            Debug.Log("BuilderDetermineIncludedServices:: download result: " + resStr ?? "null");
            if (!string.IsNullOrEmpty(resStr))
            {
                var platformCode = IsAndroid() ? Constants.ANDROID_CONFIG_PREFIX : Constants.IOS_CONFIG_PREFIX;
                Debug.Log("BuilderDetermineIncludedServices::platformCode1=" + platformCode);
                var includedServices = GetInclusionScriptableObject(platformCode, true);
                if (TTPJson.Deserialize(resStr) is List<object> serviceList)
                {
                    foreach (var serviceName in serviceList)
                    {
                        if (serviceName is string serviceStr)
                        {
                            switch (serviceStr)
                            {
                                case Constants.PACAKAGES_APPSFLYER:
                                    includedServices.appsFlyer = true;
                                    break;
                                case Constants.PACAKAGES_ADJUST:
                                    includedServices.adjust = true;
                                    break;
                                case Constants.PACAKAGES_ANALYTICS:
                                    includedServices.analytics = true;
                                    break;
                                case Constants.PACAKAGES_CRASHTOOL:
                                    includedServices.crashTool = true;
                                    break;
                                case Constants.PACAKAGES_BANNERS:
                                    includedServices.banners = true;
                                    break;
                                case Constants.PACAKAGES_RV_INTER:
                                    includedServices.rvInter = true;
                                    break;
                                case Constants.PACAKAGES_OPENADS:
                                    includedServices.openAds = true;
                                    break;
                                case Constants.PACAKAGES_RV:
                                    includedServices.rvs = true;
                                    break;
                                case Constants.PACAKAGES_INTERSTITIALS:
                                    includedServices.interstitials = true;
                                    break;
                                case Constants.PACAKAGES_PRIVACY_SETTINGS:
                                    includedServices.privacySettings = true;
                                    break;
                                case Constants.PACAKAGES_RATEUS:
                                    includedServices.rateUs = true;
                                    break;
                                case Constants.PACAKAGES_PROMOTION:
                                    includedServices.promotion = true;
                                    break;
                                case Constants.PACAKAGES_BILLING:
                                    includedServices.billing = true;
                                    break;
                                case Constants.PACAKAGES_FACEBOOK:
                                    includedServices.facebook = true;
                                    break;

                            }
                        }
                    }
                    
                    Debug.Log("BuilderDetermineIncludedServices::platformCode2=" + platformCode);
                    SaveInclusionScriptableObject(includedServices, platformCode);
                    if (IsAndroid())
                    {
                        RemoveAndroidDependenciesResolverXml();
                        ProcessGradleTemplatesFiles();
                        CreateJarDependencies(IsMediationProviderAdmob(store), activeAndroidConfiguration.analyticsConfig.firebaseEchoMode);
                    }
                }
            }
        }
    }

    private static void RemoveAndroidDependenciesResolverXml()
    {
        var pathToAndroidResolverDependenciesXml = TTPUtils.CombinePaths(new List<string>()
        {
            Application.dataPath,
            "..",
            "ProjectSettings",
            "AndroidResolverDependencies.xml"
        });
        
        if (File.Exists(pathToAndroidResolverDependenciesXml))
        {
            Debug.Log("Removing " + pathToAndroidResolverDependenciesXml);
            File.Delete(pathToAndroidResolverDependenciesXml);
        }
        
    }

    private class AndroidGradlePreprocess : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder
        {
            get { return 101; }
        }

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var ttpVersion = File.ReadAllText("Assets/Tabtale/TTPlugins/version.txt");
            var ttpVersionGradleProperty = "ttp_version=" + ttpVersion ?? "null";
            var gradleClassPath = "3.4.3";
#if UNITY_2022_3_OR_NEWER
            gradleClassPath = "7.1.2";
#elif UNITY_2020_3_OR_NEWER
            gradleClassPath = "4.2.2";
#elif UNITY_2020_1_OR_NEWER
            gradleClassPath = "3.6.4";
#endif
            var androidSdkVersion34 = (AndroidSdkVersions)34;
#if !UNITY_2022_3_OR_NEWER
            if (PlayerSettings.Android.targetSdkVersion >= androidSdkVersion34)
            {
                gradleClassPath = "7.4.2";
            }
#endif
            Debug.Log("OnPostGenerateGradleAndroidProject::gradleClassPath=" + gradleClassPath +
                      " targetSdkVersion=" + PlayerSettings.Android.targetSdkVersion);

            List<string> gradlePropList = new List<string>
            {
                "\nandroid.useAndroidX=true",
                "android.enableJetifier=true",
                "android.enableD8.desugaring=true",
#if !UNITY_2020_3_OR_NEWER
                "android.enableIncrementalDesugaring=false",
#endif
                "clik_gradle_classpath=" + gradleClassPath,
                ttpVersionGradleProperty
            };

            File.AppendAllLines(Path.Combine(path, "gradle.properties"), gradlePropList.ToArray());
            File.AppendAllLines(Path.Combine(path, Path.Combine("..", "gradle.properties")), gradlePropList.ToArray()); 

            var buildGradlePath = Path.Combine(path, "build.gradle");
            Debug.Log(path + ", " + Application.productName + ", \n" + buildGradlePath);
            
            var buildGradleContentDebug = File.ReadAllText(buildGradlePath);
            Debug.Log("OnPostGenerateGradleAndroidProject::buildGradleContentDebug:\n" + buildGradleContentDebug);
            
            var gradleLines = File.ReadLines(buildGradlePath).ToList();
            var indexOfAndroidConfigurations = -1;
            var indexOfGradleDependencies = -1;
            for (int i = 0; i < gradleLines.Count; i++)
            {
                if (gradleLines[i].Contains("AndroidConfigurations"))
                {
                    indexOfAndroidConfigurations = i;
                }
                else if (gradleLines[i].Contains("dependencies") && indexOfGradleDependencies == -1)
                {
                    indexOfGradleDependencies = i;
                }
            }

            Debug.Log("OnPostGenerateGradleAndroidProject::indexOfAndroidConfigurations=" + indexOfAndroidConfigurations +
                      ", indexOfGradleDependencies=" + indexOfGradleDependencies);
            
            if (indexOfAndroidConfigurations >= 0 && indexOfGradleDependencies >= 0)
            {
                var androidConfiguration = gradleLines[indexOfAndroidConfigurations];
                gradleLines.RemoveAt(indexOfAndroidConfigurations);
                gradleLines.Insert(indexOfGradleDependencies + 1, androidConfiguration);
                File.WriteAllLines(buildGradlePath, gradleLines);
            }
            AddCrashlyticsDependenciesIfIncluded(path);
            
#if UNITY_2020_3
            if (PlayerSettings.Android.targetSdkVersion > AndroidSdkVersions.AndroidApiLevel30)
            {
                UpdateGradleForAndroidTarget31(path);
            }
#endif
            
            string pathToModuleAndroidManifest = TTPUtils.CombinePaths(new List<string> {path, "src", "main", "AndroidManifest.xml"});
            FixManifest(pathToModuleAndroidManifest);
            
            if (TTPEditorUtils.IsTTPBatchMode(true))
            {
                ChangeGradle();
            }
            
            if (PlayerSettings.Android.targetSdkVersion >= androidSdkVersion34)
            {
                ModifyProjectForAndroid14(path);
            }
        }
        
        private static void AddCrashlyticsDependenciesIfIncluded(string path)
         {

             var includedServices = GetInclusionScriptableObject(Constants.ANDROID_CONFIG_PREFIX);
             if (!includedServices.crashTool)
             {
                 Debug.Log("OnPostGenerateGradleAndroidProject:AddCrashlyticsDependenciesIfIncluded crashtool is not included");
                 return;
             }

             Debug.Log("OnPostGenerateGradleAndroidProject:AddCrashlyticsDependenciesIfIncluded start");
             var topBuildGradlePath = Path.Combine(path, Path.Combine("..", "build.gradle"));
             var topGradleLines = File.ReadLines(topBuildGradlePath).ToList();
             Debug.Log("OnPostGenerateGradleAndroidProject:AddCrashlyticsDependenciesIfIncluded topBuildGradlePath = " + topBuildGradlePath + ", topBuildGradleContentDebug = " + File.ReadAllText(topBuildGradlePath));
             int index = topGradleLines.FindIndex(line => line.Contains("clik_gradle_classpath"));
             if (index != -1)
             {
                 Debug.Log("OnPostGenerateGradleAndroidProject::AddCrashlyticsDependenciesIfIncluded: Added: classpath 'com.google.firebase:firebase-crashlytics-gradle:2.9.9");
                 topGradleLines.Insert(index + 1, "       classpath 'com.google.firebase:firebase-crashlytics-gradle:2.9.9'");
             }
             File.WriteAllLines(topBuildGradlePath, topGradleLines);

             var buildGradleLauncherPath = Path.Combine(path, Path.Combine("..", Path.Combine("launcher", "build.gradle")));
             var launcherGradleLines = File.ReadLines(buildGradleLauncherPath).ToList();
             Debug.Log("OnPostGenerateGradleAndroidProject:AddCrashlyticsDependenciesIfIncluded buildGradleLauncherPath = " + buildGradleLauncherPath + ", buildGradleLauncherContentDebug = " + File.ReadAllText(buildGradleLauncherPath));
             index = launcherGradleLines.FindIndex(line => line.Contains("apply plugin: 'com.android.application'"));
             if (index != -1)
             {
                 Debug.Log("OnPostGenerateGradleAndroidProject::AddCrashlyticsDependenciesIfIncluded: Added: apply plugin: 'com.google.firebase.crashlytics'");
                 launcherGradleLines.Insert(index + 1, "apply plugin: 'com.google.firebase.crashlytics'");
             }
             File.WriteAllLines(buildGradleLauncherPath, launcherGradleLines);
         }

        private static void ModifyProjectForAndroid14(string path)
        {
#if !UNITY_2022_3_OR_NEWER
            Debug.Log("OnPostGenerateGradleAndroidProject::ModifyProjectForAndroid14:");
            var pathToMainProject = Path.Combine(path, "..");
            
            var updateLines = false;
            var gradlePropertiesFilePath = Path.Combine(pathToMainProject, "gradle.properties");
            var lines = File.ReadAllLines(gradlePropertiesFilePath).ToList();
            var lineToRemove = lines.FirstOrDefault(line => line.Contains("android.enableR8"));
            if (lineToRemove != null)
            {
                lines.Remove(lineToRemove);
                updateLines = true;
                Debug.Log("OnPostGenerateGradleAndroidProject::ModifyProjectForAndroid14:The line containing 'android.enableR8' has been removed.");
            }
            else
            {
                Debug.Log("OnPostGenerateGradleAndroidProject::ModifyProjectForAndroid14:No line containing 'android.enableR8' was found.");
            }

            if (updateLines)
            {
                File.WriteAllLines(gradlePropertiesFilePath, lines);
            }
            
            var mainSettingsGradlePath = Path.Combine(pathToMainProject, "settings.gradle");
            var mainSettingsGradleContent = File.ReadAllText(mainSettingsGradlePath);
            string buildToolsPattern = @"[\s]classpath\(""com\.android\.tools:.*""\)";
            string buildToolsReplacement = "classpath(\"com.android.tools:r8:8.3.37\")";
            mainSettingsGradleContent = Regex.Replace(mainSettingsGradleContent, buildToolsPattern, buildToolsReplacement);
            File.WriteAllText(mainSettingsGradlePath, mainSettingsGradleContent);
            Debug.Log("OnPostGenerateGradleAndroidProject::ModifyProjectForAndroid14:mainSettingsGradleContent:\n" + mainSettingsGradleContent);
            string gradlewWrapperPropPath = TTPUtils.CombinePaths(new List<string> {pathToMainProject, "gradle", "wrapper", "gradle-wrapper.properties"});
            Debug.Log("OnPostGenerateGradleAndroidProject::ModifyProjectForAndroid14:gradlewWrapperPropPath=" + gradlewWrapperPropPath);
            string gradlewWrapperPropContent = File.ReadAllText(gradlewWrapperPropPath);
            string distributionUrlReplacement = @"distributionUrl=https\://services.gradle.org/distributions/gradle-7.5-bin.zip";
            string distributionUrlPattern = @"distributionUrl.*";
            gradlewWrapperPropContent = Regex.Replace(gradlewWrapperPropContent, distributionUrlPattern, distributionUrlReplacement);
            File.WriteAllText(gradlewWrapperPropPath, gradlewWrapperPropContent);
            Debug.Log("OnPostGenerateGradleAndroidProject::ModifyProjectForAndroid14:gradlewWrapperPropContent:\n" + gradlewWrapperPropContent);
            
            if (CompareVersion(Application.unityVersion, "2021.3.23f1") <= 0)
            {
                Debug.Log("OnPostGenerateGradleAndroidProject::ModifyProjectForAndroid14:Unity <= 2021.3.23f1");
                var rootBuildGradlePath = Path.Combine(pathToMainProject, "build.gradle");
                lines = File.ReadAllLines(rootBuildGradlePath).ToList();
                int index = lines.FindIndex(line => line.Contains("com.android.tools.build:gradle"));
                if (index != -1)
                {
                    lines.Insert(index + 1, "        classpath 'com.android.tools:r8:8.3.37'");
                    File.WriteAllLines(rootBuildGradlePath, lines);
                    Debug.Log("OnPostGenerateGradleAndroidProject::ModifyProjectForAndroid14:Added com.android.tools:r8:8.3.37 for Unity 2020 support");
                }
            }
#endif
        }

        private static void ChangeGradle()
        {
            string gradlePathFromEditorPrefs = EditorPrefs.GetString("GradlePath");
            string gradlePathExternalToolsSettings = AndroidExternalToolsSettings.gradlePath;
            Debug.Log("OnPostGenerateGradleAndroidProject::ChangeGradle:Before:gradlePathFromEditorPrefs=" + gradlePathFromEditorPrefs);
            Debug.Log("OnPostGenerateGradleAndroidProject::ChangeGradle:Before:gradlePathExternalToolsSettings=" + gradlePathExternalToolsSettings);
#if UNITY_2022_3_OR_NEWER
            string gradlePath = "";
#else
            string gradlePath = "/Users/tabtale/gradle-6.7.1";
#endif
            
#if UNITY_CLOUD_BUILD
            var    workspacePath = Environment.GetEnvironmentVariable("USERPROFILE");
            gradlePath    = Path.Combine(workspacePath, "gradle-6.7.1");
#endif
            AndroidExternalToolsSettings.gradlePath = gradlePath;
            EditorPrefs.SetString("GradlePath", gradlePath);
            gradlePathFromEditorPrefs = EditorPrefs.GetString("GradlePath");
            gradlePathExternalToolsSettings = AndroidExternalToolsSettings.gradlePath;
            Debug.Log("OnPostGenerateGradleAndroidProject::ChangeGradle:After:gradlePathFromEditorPrefs=" + gradlePathFromEditorPrefs);
            Debug.Log("OnPostGenerateGradleAndroidProject::ChangeGradle:After:gradlePathExternalToolsSettings=" + gradlePathExternalToolsSettings);
        }

        private static void ReplaceBuildToolsVersion(string buildGradlePath)
        {
            Debug.Log("OnPostGenerateGradleAndroidProject::ReplaceBuildToolsVersion:buildGradlePath=" + buildGradlePath);
            if (!File.Exists(buildGradlePath))
            {
                Debug.Log("OnPostGenerateGradleAndroidProject::ReplaceBuildToolsVersion:gradle doesn't exist");
                return;
            }

            Boolean rewriteFile = false;
            var gradleLines = File.ReadLines(buildGradlePath).ToList();
            for (int i = 0; i < gradleLines.Count; i++)
            {
                string currentLine = gradleLines[i];
                if (currentLine.Contains("buildToolsVersion"))
                {
                    Debug.Log("OnPostGenerateGradleAndroidProject::ReplaceBuildToolsVersion:" + currentLine);
                    if (!currentLine.Contains("30.0.3"))
                    {
                        currentLine = "buildToolsVersion '30.0.3'";
                        gradleLines[i] = currentLine;
                        rewriteFile = true;    
                    }
                    break;
                }
            }

            if (rewriteFile)
            {
                File.WriteAllLines(buildGradlePath, gradleLines);
                Debug.Log("OnPostGenerateGradleAndroidProject::ReplaceBuildToolsVersion:buildToolsVersion has fixed");
            }
        }
        
        private static void UpdateGradleForAndroidTarget31(string path)
        {
            Debug.Log("OnPostGenerateGradleAndroidProject::UpdateGradleForAndroidTarget31:path=" + path);
            var buildGradlePath = Path.Combine(path, "build.gradle");
            ReplaceBuildToolsVersion(buildGradlePath);
            buildGradlePath = Path.Combine(path, Path.Combine("AndroidConfigurations.androidlib", "build.gradle"));
            ReplaceBuildToolsVersion(buildGradlePath);
            buildGradlePath = Path.Combine(path, Path.Combine("..", Path.Combine("launcher", "build.gradle")));
            ReplaceBuildToolsVersion(buildGradlePath);
        }

        private static void FixManifest(string pathToManifest)
        {
            Debug.Log("OnPostGenerateGradleAndroidProject::FixManifest:pathToManifest=" + pathToManifest);
            /*var xmlDoc = new XmlDocument();
            xmlDoc.Load(pathToManifest);
            XmlElement launchActivityNode =
                (XmlElement)xmlDoc.SelectSingleNode("/manifest/application/activity[intent-filter/action/@android:name='android.intent.action.MAIN' and " +
                                                    "intent-filter/category/@android:name='android.intent.category.LAUNCHER']");
            if (launchActivityNode != null)
            {
                string launchModeValue = launchActivityNode.GetAttribute("launchMode", "android");
                Debug.Log("OnPostGenerateGradleAndroidProject::FixManifest:launchModeValue=" + launchModeValue);
                launchActivityNode.SetAttribute("launchMode", "android", "singleTop");
            }*/
            string androidManifest = File.ReadAllText(pathToManifest);
            androidManifest = androidManifest.Replace("singleTask", "singleTop");
            File.WriteAllText(pathToManifest, androidManifest);
            //xmlDoc.Save(pathToManifest);
            Debug.Log("OnPostGenerateGradleAndroidProject::FixManifest:androidManifest=" + androidManifest);
        }
        
        private static string DetermineIncludedServices(string mainGradle, bool isAdMob)
        {
            var includedServices = GetInclusionScriptableObject(Constants.ANDROID_CONFIG_PREFIX);
            if (includedServices == null)
            {
                Debug.LogError("CreatePodDependencies:: included services is null! will not build correctly.");
                return null;
            }
            
            var exclusionsList = new List<string>
            {
                "TT_Plugins_Banners_Mopub",
                "TT_Plugins_Banners_MoPub",
                "TT_Plugins_RewardedAds_Mopub",
                "TT_Plugins_RewardedAds_MoPub",
                "TT_Plugins_Interstitials_Mopub",
                "TT_Plugins_Interstitials_MoPub",
                "TT_Plugins_CrossDevicePersistency",
                "TT_Plugins_CrossPromotion",
                "TT_Plugins_TTAnalyticsAgent",
                "TT_Plugins_DeltaDnaAgent",
                "TT_Plugins_FlurryAgent",
                "TT_Plugins_NativeCampaign",
                "TT_Plugins_Share",
                "TT_Plugins_Social"

            };
            if (!includedServices.analytics)
            {
                exclusionsList.Add("TT_Plugins_Analytics");
                exclusionsList.Add("TT_Plugins_FirebaseAgent");
            }
            if (!includedServices.appsFlyer)
            {
                exclusionsList.Add("TT_Plugins_AppsFlyer");
            }
            if (!includedServices.adjust)
            {
                exclusionsList.Add("TT_Plugins_Adjust");
            }
            if (!includedServices.banners)
            {
                exclusionsList.Add("TT_Plugins_Banners_Admob");
                exclusionsList.Add("TT_Plugins_Elephant");
            }
            if (!includedServices.rvs)
            {
                exclusionsList.Add("TT_Plugins_RewardedAds_Admob");
            }
            if (!includedServices.interstitials)
            {
                exclusionsList.Add("TT_Plugins_Interstitials_Admob");
            }
            if (!includedServices.openAds)
            {
                exclusionsList.Add("TT_Plugins_OpenAds");
            }
            if (!includedServices.rvInter || !isAdMob)
            {
                exclusionsList.Add("TT_Plugins_RewardedInterstitials");
            }

            if (!includedServices.promotion)
            {
                exclusionsList.Add("TT_Plugins_Promotion");
            }

            if (!includedServices.billing)
            {
                exclusionsList.Add("TT_Plugins_Billing");
            }
            if (!includedServices.banners && !includedServices.rvs && !includedServices.openAds && !includedServices.rvInter && !includedServices.promotion)
            {
                exclusionsList.Add("TT_Plugins_PopupMgr");
            }
            if (!includedServices.crashTool)
            {
                exclusionsList.Add("TT_Plugins_CrashTool");
            }
            if (!includedServices.privacySettings)
            {
                exclusionsList.Add("TT_Plugins_Privacy_Settings");
            }
            
            if (!includedServices.openAds && !includedServices.interstitials && !includedServices.rvs && !includedServices.banners && !includedServices.rvInter)
            {
                exclusionsList.Add("TT_Plugins_ECPM");
                exclusionsList.Add("TT_Plugins_AdManager_Max");
                exclusionsList.Add("TT_Plugins_AdManager_Admob");
            }
            else
            {
                exclusionsList.Add(isAdMob ? "TT_Plugins_AdManager_Max" : "TT_Plugins_AdManager_Admob");
            }
            var exclusions = "";
            foreach (var excludedService in exclusionsList)
            {
                exclusions += "exclude module: '" + excludedService + "'\n";
            }
            var implementationStr = "implementation ('com.tabtale.tt_plugins.android:TT_Plugins:'+ttp_version){\n"
                                    + exclusions
                                    + "}\n";
#if TTP_DEV_MODE
            implementationStr = "";
#endif
            return mainGradle.Replace("@@TT_PLUGINS_DEPENDENCIES@@", implementationStr);
        }
    }

#if UNITY_IOS
    private class PostProcess : IPostprocessBuildWithReport
    {
        public int callbackOrder { get { return 101; } }
        public void OnPostprocessBuild(BuildReport report)
        {
            Debug.Log("CLIKConfiguration::iOS:OnPostprocessBuild: ");
            var plistPath = Path.Combine(report.summary.outputPath, "Info.plist");
            var plist = new PlistDocument();
            var globalConfig = new GlobalConfig();
            if (TTPEditorUtils.IsTTPBatchMode(true))
            {
                Debug.Log("CLIKConfiguration::iOS:OnPostprocessBuild:batch mode");
                globalConfig.LoadFromFile();
            }
            else
            {
                Debug.Log("CLIKConfiguration::iOS:OnPostprocessBuild:not batch mode");
                globalConfig.LoadFromFile(Constants.IOS_CONFIG_PREFIX);
            }
            plist.ReadFromFile(plistPath);
            Debug.Log("CLIKConfiguration::iOS:OnPostprocessBuild:admobAppId=" + globalConfig.admobAppId);
            plist.root.SetString("GADApplicationIdentifier", globalConfig.admobAppId);

            File.WriteAllText(plistPath, plist.WriteToString());

            SetVersioningInBuildSettings(report.summary.outputPath);
        }

        private void SetVersioningInBuildSettings(string path)
        {
            Debug.Log("CLIKConfiguration::iOS:OnPostprocessBuild:SetVersioningInBuildSettings: ");
            var pbxProjectPath = PBXProject.GetPBXProjectPath(path);
            var pbxProject = new PBXProject();
            pbxProject.ReadFromString(File.ReadAllText(pbxProjectPath));
            var unityFrameworkTarget = pbxProject.GetUnityFrameworkTargetGuid();
            pbxProject.SetBuildProperty(unityFrameworkTarget, "MARKETING_VERSION", Application.version);
            File.WriteAllText(pbxProjectPath, pbxProject.WriteToString());
        }
    }
#endif
    private class ConfigurationSaverPreprocess : IPreprocessBuildWithReport
    {
        public int callbackOrder => 99;

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("CLIKConfiguration::OnPreprocessBuild " + report.summary.platform);

            var analyticsConfig = new AnalyticsConfig();
            var globalConfig = new GlobalConfig();
            if (report.summary.platform == BuildTarget.Android)
            {
                if (TTPEditorUtils.IsTTPBatchMode(true))
                {
                    Debug.Log("CLIKConfiguration::OnPreprocessBuild:batch mode");
                    globalConfig.LoadFromFile();
                    analyticsConfig.LoadFromFile();
                }
                else
                {
                    Debug.Log("CLIKConfiguration::OnPreprocessBuild:not batch mode");
                    globalConfig.LoadFromFile(Constants.ANDROID_CONFIG_PREFIX);
                    analyticsConfig.LoadFromFile(Constants.ANDROID_CONFIG_PREFIX);
                }
                UpdateAndroidRes(globalConfig, analyticsConfig);
                UpdateAndroidStringsXml();
            }

            if (report.summary.platform == BuildTarget.iOS)
            {
                bool isAdmob;
                if (TTPEditorUtils.IsTTPBatchMode(true))
                {
                    Debug.Log("CLIKConfiguration::OnPreprocessBuild:batch mode");
                    globalConfig.LoadFromFile();
                    analyticsConfig.LoadFromFile();
                    isAdmob = IsMediationProviderAdmob(Constants.IOS_STORE_NAME);
                }
                else
                {
                    Debug.Log("CLIKConfiguration::OnPreprocessBuild:not batch mode");
                    globalConfig.LoadFromFile(Constants.IOS_CONFIG_PREFIX);
                    analyticsConfig.LoadFromFile(Constants.IOS_CONFIG_PREFIX);
                    isAdmob = globalConfig.isAdMob;
                }
                Debug.Log("CLIKConfiguration::OnPreprocessBuild:isAdmob=" + isAdmob);
                IOSResolver.PodfileStaticLinkFrameworks = false;
                CreatePodDependencies(isAdmob, analyticsConfig.firebaseEchoMode);
            }

#if UNITY_2019_4
            if(report.summary.platform == BuildTarget.Android && PlayerSettings.Android.targetSdkVersion > AndroidSdkVersions.AndroidApiLevel30)
            {
                EditorUtility.DisplayDialog("Build Warning", "You are attempting to build for Android Target SDK 31 or higher and Unity 2019. This is not supported by CLIK.", "Got it");
            }
#endif
            
            ProcessLocalConfiguration();
        }

        private static void ProcessLocalConfiguration()
        {
            Debug.Log("OnPreprocessBuild::ProcessLocalConfiguration: ");

            var dirPathJson = "Assets/StreamingAssets/ttpGameConfig/keyvalue";

            if (!File.Exists(TTPCore.TTP_LOCAL_CONFIGURATION_PATH))
            {
                Debug.Log("OnPreprocessBuild::ProcessLocalConfiguration:ttpLocalConfiguration doesn't exist");
                return;
            }

            if (!Directory.Exists(dirPathJson))
            {
                Directory.CreateDirectory(dirPathJson);
            }

            var localConfiguration = AssetDatabase.LoadAssetAtPath<TTPLocalConfigurationScriptableObject>(TTPCore.TTP_LOCAL_CONFIGURATION_PATH);
            var localConfigDic = new Dictionary<string, string>();
            foreach (var configData in localConfiguration.configData)
            {
                localConfigDic.Add(configData.name, configData.value);
            }

            var jsonData = TTPJson.Serialize(localConfigDic);
            Debug.Log("OnPreprocessBuild::ProcessLocalConfiguration:jsonData=" + jsonData);
            File.WriteAllText(Path.Combine(dirPathJson, "defaults.json"), jsonData);
        }
        
        private static void CreatePodDependencies(bool isAdMob, bool firebaseEchoMode)
        {
            var includedServices = GetInclusionScriptableObject(Constants.IOS_CONFIG_PREFIX);
            if (includedServices == null)
            {
                Debug.LogError("CreatePodDependencies:: included services is null! will not build correctly.");
                return;
            }
            
            var services = new List<string>
            {
                "TT_Plugins_Core"
            };
            
            if (includedServices.facebook)
            {
                services.Add("TT_Plugins_Facebook");
            }
            
            if (includedServices.appsFlyer)
            {
                services.Add("TT_Plugins_AppsFlyer");
            }

            if (includedServices.adjust)
            {
                services.Add("TT_Plugins_Adjust");
            }
            
            if (includedServices.crashTool)
            {
                services.Add("TT_Plugins_CrashTool");
            }

            if (includedServices.privacySettings)
            {
                services.Add("TT_Plugins_Privacy_Settings");
            }

            bool popUpMgrIncluded = false;
            if (includedServices.banners)
            {
                popUpMgrIncluded = true;
                services.Add("TT_Plugins_Banners_Admob");
                services.Add("TT_Plugins_Elephant");
            }

            if (includedServices.interstitials)
            {
                popUpMgrIncluded = true;
                services.Add("TT_Plugins_Interstitials_Admob");
            }

            if (includedServices.rvs)
            {
                popUpMgrIncluded = true;
                services.Add("TT_Plugins_RewardedAds_Admob");
            }

            if (includedServices.openAds)
            {
                popUpMgrIncluded = true;
                services.Add("TT_Plugins_OpenAds");
            }

            if (includedServices.rvInter && isAdMob)
            {
                popUpMgrIncluded = true;
                services.Add("TT_Plugins_RewardedInterstitials");
            }

            if (includedServices.promotion)
            {
                popUpMgrIncluded = true;
                services.Add("TT_Plugins_Promotion");
            }
            
            if (includedServices.rvs || includedServices.interstitials || includedServices.banners ||
                includedServices.openAds || includedServices.rvInter)
            {
                services.Add("TT_Plugins_ECPM");
            }

            if (popUpMgrIncluded)
            {
                services.Add("TT_Plugins_AdProviders");
                services.Add("TT_Plugins_PopupMgr");
                services.Add(isAdMob ? "TT_Plugins_AdManager_Admob" : "TT_Plugins_AdManager_Max");
            }

            if (includedServices.analytics)
            {
                services.Add("TT_Plugins_Analytics");
                if (firebaseEchoMode)
                    services.Add("TT_Plugins_FirebaseEchoAgent");
                else
                {
                    services.Add("TT_Plugins_FirebaseAgent");
                    services.Add("TT_Plugins_Remote_Config");
                }
            }

            if (includedServices.billing)
            {
                services.Add("TT_Plugins_Billing");
            }
#if TTP_DEV_MODE
            services.Clear();
#endif
            var json = File.ReadAllText("Assets/Tabtale/TTPlugins/TT_Plugins.json");
            if (json != null)
            {
                var dic = TTPJson.Deserialize(json) as Dictionary<string, object>;
                var xmlDoc = new XmlDocument();
                xmlDoc.Load("Assets/Editor/TTPDependencies.xml");
                var iosPods = xmlDoc.SelectSingleNode("//dependencies/iosPods");
                if (iosPods != null)
                {
                    var toRemove = new List<string>();
                    foreach (var kvp in dic)
                    {
                        if (kvp.Key.Contains("TT_Plugins") && !services.Contains(kvp.Key))
                        {
                            toRemove.Add(kvp.Key);
                        }
                    }

                    foreach (var key in toRemove)
                    {
                        XmlNode nodeToRemove = null;
                        foreach (XmlNode childNode in iosPods.ChildNodes)
                        {
                            if (childNode.Attributes?["name"] != null && childNode.Attributes["name"].Value == key + "_Pod")
                            {
                                Debug.Log("removing " + key);
                                nodeToRemove = childNode;
                            }
                        }

                        if (nodeToRemove != null)
                            iosPods.RemoveChild(nodeToRemove);
                        dic.Remove(key);
                    }

                    foreach (var kvp in dic)
                    {
                        Debug.Log(kvp.Key + ", " + kvp.Value);
                        if (services.Contains(kvp.Key))
                        {
                            var podName = kvp.Key + "_Pod";
                            var node = GetXMlNode(xmlDoc, iosPods, podName, "iosPod");
                            var nameAttribute = xmlDoc.CreateAttribute("name");
                            var versionAttribute = xmlDoc.CreateAttribute("version");
                            var minTargetSdkAttribute = xmlDoc.CreateAttribute("minTargetSdk");
                            nameAttribute.Value = podName;
                            versionAttribute.Value = kvp.Value as string;
                            minTargetSdkAttribute.Value = "10.0";
                            node.Attributes.Append(nameAttribute);
                            node.Attributes.Append(versionAttribute);
                            node.Attributes.Append(minTargetSdkAttribute);
                            iosPods.AppendChild(node);
                        }
                    }
                }

                xmlDoc.Save("Assets/Editor/TTPDependencies.xml");
            }
        }
    }
}
