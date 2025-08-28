using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tabtale.TTPlugins
{
    public class DemoScene : MonoBehaviour
    {
        public Button RewardedAdButton;
        public Button PromotionButton;

        void Start()
        {
            //before you begin, make sure you have loaded a configuration zip, and set up unity iap if needed.
            //instructions - (link to clik support)

            //events should be registered to before calling TTPCore.Setup().
            TTPCore.OnRemoteConfigUpdateEvent += TTPCore_OnRemoteConfigUpdateEvent;
            TTPCore.PauseGameMusicEvent += TTPCore_PauseGameMusicEvent;
            TTPBanners.OnStatusChangeEvent += TTPBanners_OnStatusChangeEvent;
            TTPRewardedAds.ReadyEvent += TTPRewardedAds_ReadyEvent;
            TTPPromotion.ReadyEvent += TTPPromotion_ReadyEvent;
            TTPCore.Setup();
            //any other API should be used after calling TTPCore.Setup().
            SetExampleUserProperty();
        }

        private void TTPPromotion_ReadyEvent(bool ready)
        {
            PromotionButton.interactable = ready;
        }

        private void TTPRewardedAds_ReadyEvent(bool ready)
        {
            RewardedAdButton.interactable = ready;
        }

        private void TTPBanners_OnStatusChangeEvent(int heightInPx)
        {
            Debug.Log("Banners height in pixels changed to " + heightInPx);
        }

        private void TTPCore_PauseGameMusicEvent(bool shouldPause)
        {
            if (shouldPause)
            {
                Debug.Log("An ad or pop up has been shown! Game music should be paused.");
            }
            else
            {
                Debug.Log("Game music can be continued.");
            }
        }

        private void TTPCore_OnRemoteConfigUpdateEvent(Dictionary<string, object> config)
        {
            Debug.Log("Remote configuration obtained (via Firebase Remote Config)");
            Debug.Log(TTPJson.Serialize(config));
        }

        public void OnClickShowBanner()
        {
            TTPBanners.Show();
        }

        public void OnClickHideBanner()
        {
            //you do not need to call hide before calling other TTP ads to show, it is taken care of internally.
            TTPBanners.Hide();
        }

        public void OnClickShowInter()
        {
            TTPInterstitials.Show("locationForAnalyticsInter", () =>
            {
                //this callback is always called after calling Show(), even if the interstitial fails to show because it was not loaded or any other reason.
                //It can be called safely without checking if the interstitial is ready to show.
                Debug.Log("Interstitial closed!");
            });
        }

        public void OnClickShowRV()
        {
            TTPRewardedAds.Show("locationForAnalyticsRV", (shouldReward) =>
            {
                Debug.Log("RV Shown, should reward? " + shouldReward);
            });
        }

        public void OnAnalyticsLogEventClicked()
        {
            //event name, keys and values should be in camel case.
            var name = "eventName";
            var eventParms = new Dictionary<string, object>
        {
            {"eventKey","eventParam" }
        };
            TTPAnalytics.LogEvent(name, eventParms, false);
        }

        public void OnClickShowPromotion()
        {
            TTPPromotion.ShowStand("locationForAnalyticsPromotion", null);
        }

        private void SetExampleUserProperty()
        {
            //user properties are included in firebase across entire sessions in all events once added.
            var key = "userPropertyKey";
            var value = "userPropertyValue";
            TTPAnalytics.SetUserProperties(new Dictionary<string, string>() { { key, value } });
        }
    }

}

