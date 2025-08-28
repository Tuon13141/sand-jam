#if !CRAZY_LABS_CLIK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tabtale.TTPlugins
{

    [InitializeOnLoad]
    public class AdjustConfigurationDownloader
    {
        private const string ADJUST_URL_ADDITION = "/adjust/";
        private const string ADJUST_JSON_FN = "adjust";

        static AdjustConfigurationDownloader()
        {
            TTPMenu.OnDownloadConfigurationCommand += DownloadConfiguration;
        }

        private static void DownloadConfiguration(string domain, string store)
        {
            string url = domain + ADJUST_URL_ADDITION + store + "/" + PlayerSettings.applicationIdentifier;
            bool result = TTPMenu.DownloadConfiguration(url, ADJUST_JSON_FN);
            if (!result)
            {
                Debug.LogWarning("AdjustConfigurationDownloader:: DownloadConfiguration: failed to download configuration for adjust.");
            }
            
        }
    }
}
#endif