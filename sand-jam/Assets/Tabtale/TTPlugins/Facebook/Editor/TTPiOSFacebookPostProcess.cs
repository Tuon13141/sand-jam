using System;
using System.IO;
using Tabtale.TTPlugins;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Tabtale.TTPlugins
{
    public class TTPiOSFacebookPostProcess
    {
        [PostProcessBuild(40006)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
    #if UNITY_IOS        
            Debug.Log("TTPFacebookUnity: TTPiOSFacebookPostProcess:: OnPostProcessBuild: for project path: " + path);
            Debug.Log("TTPFacebookUnity: TTPiOSFacebookPostProcess:: OnPostProcessBuild: for target: " + target);
            
            var pathToFacebookConfig = "Assets/StreamingAssets/ttp/configurations/facebook.json";
            var pathToAdditionalConfig = "Assets/StreamingAssets/ttp/configurations/additionalConfig.json";

            var config = TTPFacebookUtils.TryOpenConfig(pathToFacebookConfig);
            var additionalConfig = TTPFacebookUtils.TryOpenConfig(pathToAdditionalConfig);

            if (config != null)
            {
                string facebookAppID = TTPFacebookUtils.TryGetValue(config, "fbAppID");
                string facebookClientToken = TTPFacebookUtils.TryGetValue(config, "fbClientID");
                string facebookDisplayName = TTPFacebookUtils.TryGetValue(config, "fbAppName");
                int facebookAutoLogAppEventsEnabled =
                    TTPFacebookUtils.TryGetBooleanValue(config, "facebookAutoLogAppEventsEnabled");
                string facebookUrlSchemes = TTPFacebookUtils.TryGetValue(config, "facebookUrlSchemes");

                if (facebookAutoLogAppEventsEnabled == -1 && additionalConfig != null)
                { 
                    facebookAutoLogAppEventsEnabled =
                        TTPFacebookUtils.TryGetBooleanValue(additionalConfig, "facebookAutoLogAppEventsEnabled");
                }

                if (String.IsNullOrEmpty(facebookUrlSchemes) && additionalConfig != null)
                {
                    facebookUrlSchemes = TTPFacebookUtils.TryGetValue(additionalConfig, "facebookUrlSchemes");
                }

                var plistPath = Path.Combine(path, "Info.plist");
                Debug.Log("TTPFacebookUnity: TTPiOSFacebookPostProcess:: OnPostProcessBuild: plist path: " + plistPath);

                var plist = new PlistDocument();
                plist.ReadFromFile(plistPath);
                var plistRootDict = plist.root;
                
                Debug.Log("TTPFacebookUnity: TTPiOSFacebookPostProcess:: OnPostProcessBuild: got plist: " + plistRootDict);
                
                plistRootDict.SetString("FacebookAppID", facebookAppID);
                plistRootDict.SetString("FacebookClientToken", facebookClientToken);
                plistRootDict.SetString("FacebookDisplayName", facebookDisplayName);
                plistRootDict.SetBoolean("FacebookAutoLogAppEventsEnabled", facebookAutoLogAppEventsEnabled == 1);
                if (!String.IsNullOrEmpty(facebookUrlSchemes))
                {
                    var splitUrls = facebookUrlSchemes.Split(',');
                    if (splitUrls.Length > 0)
                    {
                        var plistArray = plistRootDict.CreateArray("URL types").AddDict().CreateArray("URL Schemes"); 
                        foreach (var url in splitUrls)
                        {
                            plistArray.AddString(url);
                        }
                    }
                }

                File.WriteAllText(plistPath, plist.WriteToString());

                Debug.Log("TTPFacebookUnity: TTPiOSFacebookPostProcess:: OnPostProcessBuild: wrote to plist");
            }
    #endif
        }
    }
}