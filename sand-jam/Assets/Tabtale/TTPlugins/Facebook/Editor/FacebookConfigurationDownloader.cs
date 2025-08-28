#if !CRAZY_LABS_CLIK
using UnityEditor;
using UnityEngine;

namespace Tabtale.TTPlugins
{
    [InitializeOnLoad]
    public class FacebookConfigurationDownloader
    {
        private const string FACEBOOK_URL_ADDITION = "/facebook/";
        private const string FACEBOOK_JSON_FN = "facebook";

        static FacebookConfigurationDownloader()
        {
            Debug.Log("TTPFacebookUnity: FacebookConfigurationDownloader:: FacebookConfigurationDownloader");
            TTPMenu.OnDownloadConfigurationCommand += DownloadConfiguration;
        }

        private static void DownloadConfiguration(string domain, string store)
        {
            Debug.Log("TTPFacebookUnity: FacebookConfigurationDownloader:: DownloadConfiguration: domain: " + domain);
            
            string url = domain + FACEBOOK_URL_ADDITION + store + "/" + PlayerSettings.applicationIdentifier;
            Debug.Log("TTPFacebookUnity: FacebookConfigurationDownloader:: DownloadConfiguration: configurtion url: " + url);
            bool result = TTPMenu.DownloadConfiguration(url, FACEBOOK_JSON_FN);
            if (!result)
            {
                Debug.LogWarning("TTPFacebookUnity: FacebookConfigurationDownloader:: DownloadConfiguration: failed to download configuration for facebook.");
            } else 
                Debug.Log("TTPFacebookUnity: FacebookConfigurationDownloader:: DownloadConfiguration: success");
        }
    }
}
#endif