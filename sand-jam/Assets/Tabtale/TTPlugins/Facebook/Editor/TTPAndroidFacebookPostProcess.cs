#if !CRAZY_LABS_CLIK
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Tabtale.TTPlugins
{
    public class TTPAndroidFacebookPostProcess
    {
        [PostProcessBuild(0)]
        private static void OnPostProcess(BuildTarget target, string path)
        {
#if UNITY_ANDROID
            Debug.Log("TTPFacebookUnity: TTPAndroidFacebookPostProcess:: OnPostProcess: for project path: " + path);

#if UNITY_ANDROID && !UNITY_2021_1_OR_NEWER
            var pathToFacebookConfig = "Assets/Plugins/Android/assets/ttp/configurations/facebook.json";
            var pathToAdditionalConfig = "Assets/Plugins/Android/assets/ttp/configurations/additionalConfig.json";
#else
            var pathToFacebookConfig = "Assets/StreamingAssets/ttp/configurations/facebook.json";
            var pathToAdditionalConfig = "Assets/StreamingAssets/ttp/configurations/additionalConfig.json";
#endif

            var config = TTPFacebookUtils.TryOpenConfig(pathToFacebookConfig);
            var additionalConfig = TTPFacebookUtils.TryOpenConfig(pathToAdditionalConfig);

            List<string> pathsToStrings = new List<string>()
            {
                TTPUtils.CombinePaths(new List<string>()
                    { path, "RootProject", "app", "launcher", "src", "main", "res", "values", "strings.xml" }),
                TTPUtils.CombinePaths(new List<string>()
                    { path, "RootProject", "app", "src", "main", "res", "values", "strings.xml" }),
                TTPUtils.CombinePaths(new List<string>()
                    { path, "launcher", "src", "main", "res", "values", "strings.xml" }),
                TTPUtils.CombinePaths(new List<string>()
                    { path, "RootProject", "app", "unityLibrary", "src", "main", "res", "values", "strings.xml" }),
                TTPUtils.CombinePaths(new List<string>()
                    { path, "unityLibrary", "src", "main", "res", "values", "strings.xml" })
            };
            XmlDocument stringsXml = null;
            string pathToString = null;
            foreach (string possibleStringsPath in pathsToStrings)
            {
                stringsXml = TTPFacebookUtils.TryOpenXml(possibleStringsPath);
                pathToString = possibleStringsPath;
                if (stringsXml != null)
                    break;
            }

            if (config != null)
            {
                Debug.Log("TTPFacebookUnity: TTPAndroidFacebookPostProcess:: OnPostProcess: config is not null");
                
                string facebookAppID = TTPFacebookUtils.TryGetValue(config, "fbAppID");
                string facebookClientToken = TTPFacebookUtils.TryGetValue(config, "fbClientID");
                string facebookDisplayName = TTPFacebookUtils.TryGetValue(config, "fbAppName");
                int facebookAutoLogAppEventsEnabled =
                    TTPFacebookUtils.TryGetBooleanValue(config, "facebookAutoLogAppEventsEnabled");
                if (facebookAutoLogAppEventsEnabled == -1 && additionalConfig != null)
                {
                    facebookAutoLogAppEventsEnabled =
                        TTPFacebookUtils.TryGetBooleanValue(additionalConfig, "facebookAutoLogAppEventsEnabled");
                }

                Debug.Log("TTPFacebookUnity: TTPAndroidFacebookPostProcess:: OnPostProcess: got keys: " 
                          + facebookAppID 
                          + "; " + facebookClientToken 
                          + "; " + facebookDisplayName);
                
                if (stringsXml != null)
                { 
                    var docNode = stringsXml.DocumentElement; 
                    if (docNode != null) 
                    { 
                        XmlElement nodeAppId = (XmlElement)docNode.SelectSingleNode($"string[@name=FacebookAppID]"); 
                        if (nodeAppId == null) 
                        { 
                            nodeAppId = stringsXml.CreateElement("string"); 
                            nodeAppId.SetAttribute("name", "FacebookAppID"); 
                            docNode.AppendChild(nodeAppId); 
                        }
                        nodeAppId.InnerText = facebookAppID;
                        
                        XmlElement nodeClientToken = (XmlElement)docNode.SelectSingleNode($"string[@name=FacebookClientToken]"); 
                        if (nodeClientToken == null) 
                        { 
                            nodeClientToken = stringsXml.CreateElement("string"); 
                            nodeClientToken.SetAttribute("name", "FacebookClientToken"); 
                            docNode.AppendChild(nodeClientToken); 
                        }
                        nodeClientToken.InnerText = facebookClientToken;
                        
                        XmlElement nodeName = (XmlElement)docNode.SelectSingleNode($"string[@name=FacebookDisplayName]"); 
                        if (nodeName == null) 
                        { 
                            nodeName = stringsXml.CreateElement("string"); 
                            nodeName.SetAttribute("name", "FacebookDisplayName"); 
                            docNode.AppendChild(nodeName); 
                        }
                        nodeName.InnerText = facebookDisplayName;
                        
                        XmlElement nodeAutoLog = (XmlElement)docNode.SelectSingleNode($"string[@name=FacebookAutoLogEvents]"); 
                        if (nodeAutoLog == null) 
                        { 
                            nodeAutoLog = stringsXml.CreateElement("string"); 
                            nodeAutoLog.SetAttribute("name", "FacebookAutoLogEvents"); 
                            docNode.AppendChild(nodeAutoLog); 
                        }
                        nodeAutoLog.InnerText = facebookAutoLogAppEventsEnabled == 1 ? "true" : "false";

                        // Save the XML document
                        stringsXml.Save(pathToString);
                        
                        Debug.Log("TTPFacebookUnity: TTPAndroidFacebookPostProcess:: OnPostProcess: saved attrs in XML");
                    }
                }
            }
            Debug.Log("TTPFacebookUnity: TTPAndroidFacebookPostProcess:: OnPostProcess: config is null");
#endif         
        }
    }
}
#endif