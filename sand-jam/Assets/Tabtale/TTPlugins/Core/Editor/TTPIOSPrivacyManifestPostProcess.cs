using System.Collections.Generic;
using System.IO;
using UnityEditor.iOS.Xcode;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Tabtale.TTPlugins
{
#if UNITY_IOS
    public class TTPIOSPrivacyManifestPostProcess
    {
        private const string PRIVACY_IAPS_KEY = "iosPrivacyAPIs";

        [PostProcessBuild(44044)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            Debug.Log("TTPIOSPrivacyManifestPostProcess:OnPostProcessBuild");
            var privacyApisDictionary = GetPrivacyApisFromAdditionalConfig();
            if (privacyApisDictionary != null && privacyApisDictionary.Keys.Count > 0)
            {
                AddPrivacyApisFromAdditionalConfig(privacyApisDictionary, path);
            }

            AddFileToTarget(path);
        }

        private static void AddPrivacyApisFromAdditionalConfig(Dictionary<string, List<string>> privacyApisDictionary,
            string path)
        {
            var privacyApisPath = Path.Combine(path, "Data/Raw/TTPPrivacy/PrivacyInfo.xcprivacy");
            var privacyPlistFile = new PlistDocument();
            privacyPlistFile.ReadFromFile(privacyApisPath);
            var rootDict = privacyPlistFile.root;
            var nSPrivacyAccessedAPITypes = rootDict["NSPrivacyAccessedAPITypes"];
            var manifestDictionary = MapManifest(nSPrivacyAccessedAPITypes);
            foreach (var privacyApiItemKey in privacyApisDictionary.Keys)
            {
                if (!manifestDictionary.ContainsKey(privacyApiItemKey))
                {
                    var apiTypesArray = nSPrivacyAccessedAPITypes.AsArray();
                    var tempDictionary = apiTypesArray.AddDict();
                    tempDictionary.SetString("NSPrivacyAccessedAPIType", privacyApiItemKey);
                    var newReasonsArray = tempDictionary.CreateArray("NSPrivacyAccessedAPITypeReasons");
                    foreach (var reason in privacyApisDictionary[privacyApiItemKey])
                    {
                        Debug.Log("TTPIOSPrivacyManifestPostProcess::OnPostProcessBuild adding reason=" + reason +
                                  " for the key=" + privacyApiItemKey);
                        newReasonsArray.AddString(reason);
                    }
                }
                else
                {
                    var nSPrivacyAccessedAPITypesArray = nSPrivacyAccessedAPITypes.AsArray();
                    foreach (var item in nSPrivacyAccessedAPITypesArray.values)
                    {
                        var kvpItems = item.AsDict().values;
                        var reasonKey = kvpItems["NSPrivacyAccessedAPIType"].AsString();
                        var reasonValues = kvpItems["NSPrivacyAccessedAPITypeReasons"].AsArray().values;
                        if (reasonKey == privacyApiItemKey)
                        {
                            var reasonValuesStrings = new List<string>();
                            var privacyJsonReasons = privacyApisDictionary[privacyApiItemKey];

                            foreach (var itemValue in reasonValues)
                            {
                                reasonValuesStrings.Add(itemValue.AsString());
                            }

                            foreach (var reason in privacyJsonReasons)
                            {
                                if (!reasonValuesStrings.Contains(reason))
                                {
                                    Debug.Log(
                                        "TTPIOSPrivacyManifestPostProcess::OnPostProcessBuild adding reason=" +
                                        reason + " for the key=" + reasonKey);
                                    var itemArray = item.AsDict().values["NSPrivacyAccessedAPITypeReasons"]
                                        .AsArray();
                                    itemArray.AddString(reason);
                                }
                            }
                        }
                    }
                }
            }

            File.WriteAllText(privacyApisPath, privacyPlistFile.WriteToString());
        }

        private static void AddFileToTarget(string path)
        {
            Debug.Log("TTPIOSPrivacyManifestPostProcess:addFileToTarget: adding prifacy manifest file to main target");
            var pbxProjectPath = UnityEditor.iOS.Xcode.PBXProject.GetPBXProjectPath(path);
            var pbxProject = new UnityEditor.iOS.Xcode.PBXProject();
            pbxProject.ReadFromString(System.IO.File.ReadAllText(pbxProjectPath));
            var fileGuid = pbxProject.AddFile("Data/Raw/TTPPrivacy/PrivacyInfo.xcprivacy", "PrivacyInfo.xcprivacy");
            pbxProject.AddFileToBuild(GetTargetGUID(pbxProject), fileGuid);
            File.WriteAllText(pbxProjectPath, pbxProject.WriteToString());
        }

        private static string GetTargetGUID(PBXProject proj)
        {
#if UNITY_2019_3_OR_NEWER
            return proj.GetUnityMainTargetGuid();
#else
            return proj.TargetGuidByName("Unity-iPhone");
#endif
        }

        private static Dictionary<string, List<string>> MapManifest(PlistElement apiTypes)
        {
            var mappedManifest = new Dictionary<string, List<string>>();
            var apiTypesArray = apiTypes.AsArray();
            foreach (var item in apiTypesArray.values)
            {
                var kvpItems = item.AsDict().values;
                var reasonKey = kvpItems["NSPrivacyAccessedAPIType"].AsString();
                var reasonValues = kvpItems["NSPrivacyAccessedAPITypeReasons"].AsArray().values;
                var reasons = new List<string>();
                foreach (var itemValue in reasonValues)
                {
                    reasons.Add(itemValue.AsString());
                }

                mappedManifest.Add(reasonKey, reasons);
            }

            return mappedManifest;
        }

        private static Dictionary<string, List<string>> GetPrivacyApisFromAdditionalConfig()
        {
            var configurationJson = TTPUtils.ReadStreamingAssetsFile("ttp/configurations/additionalConfig.json");
            if (string.IsNullOrEmpty(configurationJson))
            {
                Debug.Log(
                    "TTPIOSPrivacyManifestPostProcess:GetPrivacyApisFromAdditionalConfig configurationJson is null or empty");
                return null;
            }

            var configuration = TTPJson.Deserialize(configurationJson) as Dictionary<string, object>;
            if (configuration == null)
            {
                Debug.Log(
                    "TTPIOSPrivacyManifestPostProcess:GetPrivacyApisFromAdditionalConfig json serialization failed, please check your json structure");
                return null;
            }

            if (!configuration.ContainsKey(PRIVACY_IAPS_KEY) ||
                configuration[PRIVACY_IAPS_KEY] == null ||
                !(configuration[PRIVACY_IAPS_KEY] is List<object>)
               )
            {
                Debug.Log(
                    "TTPIOSPrivacyManifestPostProcess:GetPrivacyApisFromAdditionalConfig configurationJson does not contain key, value is null or is not List of objects");
                return null;
            }

            var privacyConfiguration = configuration[PRIVACY_IAPS_KEY] as List<object>;
            if (privacyConfiguration.Count == 0)
            {
                Debug.Log(
                    "TTPIOSPrivacyManifestPostProcess:GetPrivacyApisFromAdditionalConfig privacy configuration is empty");
                return null;
            }

            var privacyDictionary = new Dictionary<string, List<string>>();
            foreach (var item in privacyConfiguration)
            {
                if (!(item is Dictionary<string, object>))
                {
                    continue;
                }

                var dict = (Dictionary<string, object>)item;
                foreach (var kvp in dict)
                {
                    if (kvp.Value is List<object>)
                    {
                        var stringList = new List<string>();
                        foreach (var itemObject in (List<object>)kvp.Value)
                        {
                            if (itemObject is string)
                            {
                                stringList.Add(itemObject.ToString());
                            }
                            else
                            {
                                Debug.Log(
                                    "TTPIOSPrivacyManifestPostProcess:GetPrivacyApisFromAdditionalConfig the given reason " +
                                    itemObject.ToString() + " is not a string. Skipping this reason...");
                            }
                        }

                        privacyDictionary.Add(kvp.Key, stringList);
                    }
                }
            }

            return privacyDictionary;
        }
    }

#endif
}