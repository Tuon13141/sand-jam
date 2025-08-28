using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Tabtale.TTPlugins
{
    public class TTPTestABConfigLoader
    {
        private const string TAG = "TTPTestABConfigLoader::";
        private const string BASE_URL = "https://intervalopt-prod.appspot.com/api/mobile/experiment";
        private const string BASE_URL_PARAMS = "?type=GLD_TEST&state=IN_DEV&bundleId=";
        private const string ABTEST_KEY = "abTestDomain";
        private static string _abTestDomain;
        public static void Load(Action<Dictionary<string, object>> callback)
        {
            Debug.Log(TAG + "Load:");
            
            const BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Static;
            var method = typeof(TTPCore).GetMethod("GetTTPGameObject", bindingAttr);
            if (method != null)
            {
                var gameObject = method.Invoke(null, null) as GameObject;
                if (gameObject != null)
                {
                    var mono = gameObject.GetComponent<MonoBehaviour>();
                    mono.StartCoroutine(DownloadExperimentsConfig(callback));
                }
            }
        }

        private static IEnumerator DownloadExperimentsConfig(Action<Dictionary<string, object>> callback)
        {
            var url = ConfigURL();
            Debug.Log(TAG + "DownloadExperimentsConfig: start - "  + url);

            var loaded = new UnityWebRequest(url);
            loaded.downloadHandler = new DownloadHandlerBuffer();
            yield return loaded.SendWebRequest();

            var loadedJson = loaded.downloadHandler.text;
            Debug.Log(TAG + "DownloadExperimentsConfig: loadedJson - "  + loadedJson);

            var result = ConfigForActualVersionAndPlatform(loadedJson);
            callback(result);
        }

        private static string ConfigURL()
        {
            string configurationJson = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetConfigurationJson("additionalConfig");
            if (!string.IsNullOrEmpty(configurationJson))
            {
                var configuration = TTPJson.Deserialize(configurationJson) as Dictionary<string,object>;
                if (configuration != null && configuration.ContainsKey(ABTEST_KEY) && configuration[ABTEST_KEY] is string)
                {
                    _abTestDomain = configuration[ABTEST_KEY] as string;
                }
                else
                {
                    Debug.Log(TAG + "ConfigURL:abTestDomain doesn't exist in config=" + configurationJson);
                }
            }
            else
            {
                Debug.Log(TAG + "ConfigURL:additional config doesn't exist");
            }
            Debug.Log(TAG + "ConfigURL:_abTestDomain=" + _abTestDomain);
            return (string.IsNullOrEmpty(_abTestDomain)? BASE_URL : _abTestDomain) + BASE_URL_PARAMS + Application.identifier;
        }

        private static Dictionary<string, object> ConfigForActualVersionAndPlatform(string configString)
        {
            Debug.Log(TAG + "ConfigForActualVersionAndPlatform:configString=" + configString);
            var version = Application.version;
            var configs = TTPJson.Deserialize(configString) as List<object>;
            if (configs == null || configs.Count == 0)
            {
                Debug.Log(TAG + "ConfigForActualVersionAndPlatform: no entries in the loaded config");
                return null;
            }

            var actualConfigDic = new Dictionary<string, object>();
            
            foreach (var configObject in configs)
            {
                var config = configObject as Dictionary<string, object>;
                if (config == null) { continue; }

                var versions = config["versions"] as List<object>;
                if (versions == null) { continue; }

                string configName = config["name"] as string;
                
                foreach (var versionObject in versions)
                {
                    var configVersion = versionObject as string;
                    if (configVersion == null) { continue; }

                    if (!configVersion.Equals(version))
                    {
                        continue;
                    }
                    
                    var platform = config["platform"];
                    if (platform == null) { continue; }

                    var activePlatform = RuntimePlatform.OSXEditor;
                    #if UNITY_ANDROID
                    activePlatform = RuntimePlatform.Android;
                    #elif UNITY_IOS
                    activePlatform = RuntimePlatform.IPhonePlayer;
                    #endif
                    
                    
                    var configForVersion = config["config"] as Dictionary<string, object>;

                    if (platform.Equals("IOS_PLATFORM") &&
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        activePlatform == RuntimePlatform.IPhonePlayer)
                    {
                        Debug.Log(TAG + "ConfigForActualVersionAndPlatform: iOS " + version + " => " + TTPJson.Serialize(configForVersion));
                        actualConfigDic[configName] = configForVersion;
                        break;
                    }
        
                    if (platform.Equals("ANDROID_PLATFORM") &&
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        activePlatform == RuntimePlatform.Android)
                    {
                        Debug.Log(TAG + "ConfigForActualVersionAndPlatform: Android " + version + " => " + TTPJson.Serialize(configForVersion));
                        actualConfigDic[configName] = configForVersion;
                        break;
                    }
                }
            }

            if (actualConfigDic.Count == 0)
            {
                Debug.Log(TAG + "ConfigForVersion: there is no config for " + version);
                return null;
            }
            
            Debug.Log(TAG + "ConfigForActualVersionAndPlatform: actualConfigDic => " + TTPJson.Serialize(actualConfigDic));
            return actualConfigDic;
        }
    }
}