using UnityEngine;

namespace Tabtale.TTPlugins
{
    public class TTPBackgroundController : MonoBehaviour
    {
        [SerializeField] private GameObject backgroundImage;

        private RectTransform backgroundRect;

        private static int backgroundLastHeight = 0;

        private static TTPBackgroundController Instance;

        private void Awake()
        {
            DebugLog("Awake - start");
            Instance = this;
        }

        public void Setup()
        {
            DebugLog("Setup - start");
            backgroundRect = backgroundImage.GetComponent<RectTransform>();
#if TTP_BANNERS
            Tabtale.TTPlugins.TTPBanners.OnStatusChangeEvent += ShowBannersBackground;
#endif
            DebugLog("Setup -> Register Event");

            if (backgroundLastHeight > 0)
            {
                DebugLog("Setup -> Last Height: " + backgroundLastHeight);
                ShowBannersBackground(backgroundLastHeight);
            }

            if (Application.isEditor)
            {
                DebugLog("Setup -> Editor Impl");
                ShowBannersBackground(200);
            }
            DebugLog("Setup - end");
        }

        private void OnDestroy()
        {
#if TTP_BANNERS
            Tabtale.TTPlugins.TTPBanners.OnStatusChangeEvent -= ShowBannersBackground;
#endif
            DebugLog("OnDestroy -> UnRegister Event");
        }

        private void ShowBannersBackground(int adHeight)
        {
            DebugLog("ShowBannersBackground -> adHeight = " + adHeight);
#if UNITY_EDITOR || UNITY_ANDROID
            return;
#endif
            backgroundLastHeight = adHeight;
            float bannerHeight = adHeight;
            DebugLog("ShowBannersBackground -> bannerHeight = " + bannerHeight);

            // Make banner background higher then banner
            float newHeight = 0;
            if (bannerHeight > 0)
                newHeight = bannerHeight + 20f;
            DebugLog("ShowBannersBackground -> newHeight = " + newHeight);

            // Set banner height
            backgroundRect.sizeDelta = new Vector2(backgroundRect.sizeDelta.x, newHeight);
            DebugLog("ShowBannersBackground -> backgroundRect.sizeDelta = " + backgroundRect.sizeDelta);

            // Displace background anchor position
            float heightOffset = 0;
            if (newHeight > 0)
                heightOffset = (newHeight / 2f) - 10f;
            DebugLog("ShowBannersBackground -> heightOffset = " + heightOffset);

            backgroundRect.anchoredPosition = new Vector2(backgroundRect.anchoredPosition.x, heightOffset);
            DebugLog("ShowBannersBackground -> backgroundRect.anchoredPosition = " + backgroundRect.anchoredPosition);

            backgroundImage.SetActive(true);
        }

        private void DebugLog(string log)
        {
            Debug.Log ("TTPBackgroundController:: " + log);
        }
    }
}


