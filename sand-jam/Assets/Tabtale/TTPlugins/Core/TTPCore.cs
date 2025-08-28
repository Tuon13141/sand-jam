using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine.Scripting;

namespace Tabtale.TTPlugins {

    /// <summary>
    /// This class provides initialization of all plugins
    /// </summary>
	public class TTPCore {

        /// <summary>
        /// Event for pausing game music
        /// </summary>
        public static event System.Action<bool> PauseGameMusicEvent;

        /// <summary>
        /// Event for starting a new session
        /// </summary>
        public static event System.Action OnNewTTPSessionEvent;

        public static event System.Action<Dictionary<string, object>> OnRemoteConfigUpdateEvent;

        public static event System.Action OnPopupShownEvent;
        public static event System.Action OnPopupClosedEvent;

        public const string TTP_LOCAL_CONFIGURATION_PATH =
            "Assets/Tabtale/TTPlugins/CLIK/Resources/ttpLocalConfiguration.asset";

        /// <summary>
        /// Indicates developing mode status
        /// </summary>
        public static bool DevMode
        {
            get
            {
#if TTP_DEV_MODE || UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        const string STORED_PURCHASES_PLAYER_PREFS_KEY = "ttpStoredPurchases";
        const string NUMBER_OF_PURCHASES_PLAYER_PREFS_KEY = "ttpInAppPurchases";
        const string NO_ADS_PURCHASED_PLAYER_PREFS_KEY = "ttpNoAsdPurchased";
        const string LAST_STORED_PURCHASE_DATE_PLAYER_PREFS_KEY = "ttpLastStoredPurchaseDate";

        private static TTPIncludedServicesScriptableObject _includedServices;

        public static TTPIncludedServicesScriptableObject IncludedServices
        {
            private set { _includedServices = value; }
            get { return _includedServices; }
        }

        /// <summary>
        /// Define always to use required location
        /// </summary>
        /// <param name="location">Loaction for service</param>
        public static void setGeoServiceAlwaysReturnLocation(string location)
		{
            Debug.Log("TTP_Core::setGeoServiceAlwaysReturnLocation" + location);
#if UNITY_IOS && !TTP_DEV_MODE
            if (Impl != null){
                IosImpl iOSImpl = (IosImpl)Impl;
                iOSImpl.SetGeoServiceAlwaysReturnedLocation(location);
                Debug.Log("TTP_Core::setGeoServiceAlwaysReturnLocation location setted.");
			}
			else
			{
                Debug.LogError("TTP_Core::setGeoServiceAlwaysReturnLocation Impl is null.");
            }

#else
            Debug.LogError("Feature unavailible unless in iOS");
#endif
        }

        /// <summary>
        /// Remove stored always-to-use location
        /// </summary>
        public static void clearGeoServiceAlwaysReturnLocation()
        {
#if UNITY_IOS && !TTP_DEV_MODE
            if (Impl != null)
            {
                IosImpl iOSImpl = (IosImpl)Impl;
                iOSImpl.ClearGeoServiceAlwaysReturnedLocation();
                Debug.Log("TTP_Core::clearGeoServiceAlwaysReturnLocation location setted.");
            }
            else
            {
                Debug.LogError("TTP_Core::clearGeoServiceAlwaysReturnLocation Impl is null.");
            }

#else
            Debug.LogError("Feature unavailible unless in iOS");
#endif
        }

        [Obsolete("This method is not needed anymore, ttplugins is responsible for handling it now")]
        public static event System.Action<bool> OnShouldAskForIDFA;
        
        [Obsolete("This method is not needed anymore, ttplugins is responsible for handling it now")]
        public static void AskForIDFA()
        {
            Debug.LogError("TTP_Core::AskForIDFA:This method is not needed anymore, ttp is responsible for handling it now");
        }
        
        [Obsolete("This method is not needed anymore, ttplugins is responsible for handling it now")]
        public static void RefuseToAskForIDFA()
        {
            Debug.LogError("TTP_Core::RefuseToAskForIDFA:This method is not needed anymore, ttp is responsible for handling it now");
        }
        
        private static bool _initialized = false;

        /// <summary>
        /// Setting up and initialize all services defined in the service list
        /// </summary>
        public static void Setup()
        {
            if (!_initialized)
            {

                Debug.Log("TTPCore::Setup");
                IncludedServices = Resources.Load<TTPIncludedServicesScriptableObject>("ttpIncludedServices");
                if (IncludedServices == null)
                {
#if UNITY_IOS
                    IncludedServices = Resources.Load<TTPIncludedServicesScriptableObject>("ttpIncludedServices_ios");
#elif UNITY_ANDROID
                    IncludedServices = Resources.Load<TTPIncludedServicesScriptableObject>("ttpIncludedServices_gp");
#endif
                }

                foreach (string clsName in CLASS_LIST)
                {
                    System.Type type = System.Type.GetType(clsName);
                    if (type != null)
                    {
                        TTPluginsGameObject.AddComponent(type);
                        Debug.Log("TTPCore::Setup: Added " + clsName + " to TTPluginsGameObject");
                    }
                    else
                    {
                        Debug.Log("TTPCore::Setup: Couldn't find " + clsName);
                    }
                }
                TTPluginsGameObject.AddComponent(typeof(CoreDelegate));
#if TTP_DEV_MODE
                TTPLogger.Create();
#endif
                ((ITTPCore)Impl).Setup();
                
                //For supporting obsolete flow, so the game will not get stuck because of using obsolete ATT API
                if (OnShouldAskForIDFA != null)
                    OnShouldAskForIDFA.Invoke(false);
                
                InitDeltaDnaAgent();
                InitBilling();
                InitBanners();
#if TTP_INTERSTITIALS && !TTP_DEV_MODE
                InitInterstitialCallbackProxy();
#endif
#if TTP_REWARDED_ADS && !TTP_DEV_MODE
                InitRewardedAdsCallbackProxy();
#endif
#if TTP_GAMEPROGRESSION
                InitGameProgression();
#endif
#if TTP_OPENADS && TTP_DEV_MODE
                NotifyOpenAdsFinished();
#endif
#if TTP_ANALYTICS && TTP_DEV_MODE
                if (!IsRemoteConfigExistAndEnabled())
                {
                    NotifyOnRemoteFetchCompletedEvent();
                }
#endif
#if UNITY_EDITOR
                NotifyAboutLocalConfiguration();
#endif
                _initialized = true;
#if !TTP_RATEUS
#if UNITY_ANDROID
                Debug.Log("TTPServiceManager: getInstanceForClassName, ClassNotFoundException: Tabtale.TTPlugins.TTPRateUs ");
#elif UNITY_IOS
                Debug.Log("createInstanceForService:: TTPRateUs does not exists!");
#endif
#endif
            }
            else
            {
                Debug.LogWarning("TTPCore::Setup:: was called already in this lifecycle.");
            }
        }

#if UNITY_EDITOR
        private static void NotifyAboutLocalConfiguration()
        {
            Debug.Log("TTPCore::NotifyAboutLocalConfiguration: ");
            if (TTPTestAB.IsSavedConfigForABTestExist())
            {
                Debug.Log("TTPCore::NotifyAboutLocalConfiguration:skipped, saved ab test config exists");
                return;
            }
            if (!File.Exists(TTP_LOCAL_CONFIGURATION_PATH))
            {
                Debug.Log("TTPCore::NotifyAboutLocalConfiguration:skipped, ttpLocalConfiguration doesn't exist");
                return;
            }
            if (OnRemoteConfigUpdateEvent == null)
            {
                Debug.Log("TTPCore::NotifyAboutLocalConfiguration:skipped, no subscribers for OnRemoteConfigUpdateEvent");
                return;
            }
            var localConfiguration = AssetDatabase.LoadAssetAtPath<TTPLocalConfigurationScriptableObject>(TTP_LOCAL_CONFIGURATION_PATH);
            var localConfigDic = new Dictionary<string, object>();
            foreach (var configData in localConfiguration.configData)
            {
                localConfigDic.Add(configData.name, configData.value);
            }
            OnRemoteConfigUpdateEvent(localConfigDic);
        }
#endif

#if TTP_OPENADS && TTP_DEV_MODE
        private static void NotifyOpenAdsFinished()
        {
            Debug.Log("TTPCore::NotifyOpenAdsFinished:");
            System.Type openAds = System.Type.GetType("Tabtale.TTPlugins.TTPOpenAds");
            if (openAds != null)
            {
                MethodInfo method = openAds.GetMethod("NotifyOpenAdsHasFinished", BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, new object[] { null });
                }
                else
                {
                    Debug.LogWarning("TTPCore::NotifyOpenAdsFinished: method NotifyOpenAdsHasFinished not found");
                }
            }
            else
            {
                Debug.Log("TTPCore::NotifyOpenAdsFinished: TTPOpenAds not found");
            }
        }
#endif

#if TTP_ANALYTICS && TTP_DEV_MODE
        private static void NotifyOnRemoteFetchCompletedEvent()
        {
            Debug.Log("TTPCore::NotifyOnRemoteFetchCompletedEvent:");
            System.Type analytics = System.Type.GetType("Tabtale.TTPlugins.TTPAnalytics");
            if (analytics != null)
            {
                MethodInfo method = analytics.GetMethod("NotifyOnRemoteFetchCompletedEvent", BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, new object[] { false });
                }
                else
                {
                    Debug.LogWarning("TTPCore::NotifyOpenAdsFinished: method NotifyOnRemoteFetchCompletedEvent not found");
                }
            }
            else
            {
                Debug.Log("TTPCore::NotifyOpenAdsFinished: TTPAnalytics not found");
            }
        }
#endif

        /// <summary>
        /// Indicates that back button pressed
        /// </summary>
        /// <returns>True - if back button pressed</returns>
        public static bool OnBackPressed()
        {
            if(Impl != null)
            {
                return ((ITTPCore)Impl).OnBackPressed();
            }
            return false;
        }

        /// <summary>
        /// Indicates that internet is available
        /// </summary>
        /// <returns>True - if internet is available</returns>
        public static bool IsConnectedToTheInternet()
        {
            if (Impl != null)
                return ((ITTPCore)Impl).IsConnectedToTheInternet();
            return false;
        }

        public static bool IsRemoteConfigExistAndEnabled()
        {
            if (Impl != null)
            {
                return ((ITTPCore) Impl).IsRemoteConfigExistAndEnabled();
            }

            return false;
        }

        public static long GetSessionNumber()
        {
            if (Impl != null)
            {
                return ((ITTPCore) Impl).GetSessionNumber();
            }

            return 0;
        }
        
        public static string GetEventFromLog(string agent, string eventName) {
            if (Impl != null)
            {
                return ((ITTPCore) Impl).GetEventFromLog(agent, eventName);
            }
            return "";
        }
        
        public static void ReportIAPToConversion(string currency, float price, string productId, bool consumable)
        {
#if UNITY_IOS && !TTP_DEV_MODE
            var iosImpl = Impl as IosImpl;
            if (iosImpl != null)
            {
                iosImpl.ReportIAPToConversion(currency, price, productId, consumable);
            }
#endif
        }

        public static void WriteEventToEventsFileHandler(string eventName, string agent, string paramsJSON)
            {
            var impl = Impl as ITTPCore;
            impl.WriteEventToEventsFileHandler(eventName, agent, paramsJSON);
        }

        public static void ReportPurchase(float price, string currency = "USD", Action<float, string> completion = null)
        {
            ReportPurchase(price, false, currency, completion);
        }

        public static void ReportPurchase(float price, bool isNoAdsItem = false, string currency = "USD", Action<float, string> completion = null)
        {
            TTPForeignExchangeManager.GetInstance().Exchange(currency, price, (convertedPrice, error) =>
            {
                completion?.Invoke(convertedPrice, error);
                if (convertedPrice != TTPForeignExchangeManager.FAILED)
                {
                    LogPurchaseReportedEvent(convertedPrice);

                    float storedPurchases = GetStoredPurchases();
                    Debug.Log("TTPCore::ReportPurchase: prev stored purchases - " + storedPurchases + "USD");

                    float newStoredPurchases = storedPurchases + convertedPrice;
                    PlayerPrefs.SetFloat(STORED_PURCHASES_PLAYER_PREFS_KEY, newStoredPurchases);
                    Debug.Log("TTPCore::ReportPurchase: updated stored purchases - " + newStoredPurchases + "USD");

                    TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                    string secondsSinceEpoch = t.TotalSeconds.ToString();
                    PlayerPrefs.SetString(LAST_STORED_PURCHASE_DATE_PLAYER_PREFS_KEY, secondsSinceEpoch);
#if TTP_POPUPMGR
                    ReportPurchaseDate((long)t.TotalSeconds);
#endif
                    
                    int numberOfStoredPurchases = PlayerPrefs.GetInt(NUMBER_OF_PURCHASES_PLAYER_PREFS_KEY, 0);
                    numberOfStoredPurchases += 1;
                    PlayerPrefs.SetInt(NUMBER_OF_PURCHASES_PLAYER_PREFS_KEY, numberOfStoredPurchases);
                    
                    SetUserPropertiesAfterPurchase(isNoAdsItem);
                }
            });
        }
        
        private static void SetUserPropertiesAfterPurchase(bool isNoAdsItem)
        {
#if TTP_ANALYTICS
            // Sets the ttpInAppNoAds user property to true on a purchase of no ads item
            if (isNoAdsItem)
            {
             var noAdsUserProperty = new Dictionary<string, string> 
             {
                 { "ttpInAppNoAds", "true" }
             };
             TTPAnalytics.SetUserProperties(noAdsUserProperty); 
            }
            else
            {
#if TTP_BILLING
                var noAdsUserProperty = new Dictionary<string, string>
                {
                    { "ttpInAppNoAds", TTPBilling.IsNoAdsPurchased() ? "true" : "false" }
                };
#else
                var noAdsUserProperty = new Dictionary<string, string>
                {
                    { "ttpInAppNoAds", "false" }
                };
#endif
                TTPAnalytics.SetUserProperties(noAdsUserProperty); 
            }

            int numberOfStoredPurchases = PlayerPrefs.GetInt(NUMBER_OF_PURCHASES_PLAYER_PREFS_KEY, 0);
            var numberOfStoredPurchasesMap = new Dictionary<string, string>
            {
                { "ttpInAppPurchases", numberOfStoredPurchases.ToString() }
            };
            TTPAnalytics.SetUserProperties(numberOfStoredPurchasesMap);

            float totalPurchasesRevenue = GetStoredPurchases();
            var totalIAPRevenueMap = new Dictionary<string, string>
            {
                { "ttpInAppRevenue", totalPurchasesRevenue.ToString(CultureInfo.InvariantCulture) }
            };
            TTPAnalytics.SetUserProperties(totalIAPRevenueMap);
#endif
        }

        public static float GetStoredPurchases()
        {
            var storedPurchases = PlayerPrefs.GetFloat(STORED_PURCHASES_PLAYER_PREFS_KEY, 0);
            if (storedPurchases == 0)
            {
                Debug.LogWarning("TTPCore::GetStoredPurchases: there are no stored purchases");
            }
            return storedPurchases;
        }

        public static double GetSecondsSinceEpochForLastStoredPurchase()
        {
            var lastPurchaseDate = PlayerPrefs.GetString(LAST_STORED_PURCHASE_DATE_PLAYER_PREFS_KEY, "");
            if (string.IsNullOrEmpty(lastPurchaseDate))
            {
                Debug.LogWarning("TTPCore::secondsSinceEpochForLastStoredPurchase: there are no stored purchases");
                return 0;
            }

            double t = 0.0;
            lastPurchaseDate = lastPurchaseDate.Replace(',', '.');
            if (!double.TryParse(lastPurchaseDate, NumberStyles.Any, CultureInfo.InvariantCulture, out t))
            {
                Debug.LogError("TTPCore::secondsSinceEpochForLastStoredPurchase: failed to parse: " + lastPurchaseDate);
                return 0;
            }
            return t;
        }

        private static void LogPurchaseReportedEvent(float usdAmount)
        {
            Debug.Log("TTPCore::LogPurchaseReportedEvent:usdAmount=" + usdAmount);

            System.Type analyticsClsType = System.Type.GetType("Tabtale.TTPlugins.TTPAnalytics");
            if (analyticsClsType != null)
            {
                System.Reflection.MethodInfo method = analyticsClsType.GetMethod("LogEvent", new Type[] 
                { 
                    typeof(long), typeof(string), typeof(IDictionary<string, object>), typeof(bool), typeof(bool) 
                });
                if (method != null)
                {
                    const long firebaseAnalyticsConst = 1 << 2;
                    const string eventName = "purchaseReported";
                    IDictionary<string, object> logEventParams = new Dictionary<string, object>()
                    {
                        {"usdAmount", usdAmount}
                    };
                    method.Invoke(null, new object[] { firebaseAnalyticsConst, eventName, logEventParams, false, true });
                }
                else
                {
                    Debug.LogWarning("TTPCore::CallAnalyticsByReflection:: could not find method - LogEvent");
                }
            }
            else
            {
                Debug.LogWarning("TTPCore::CallAnalyticsByReflection:: could not find TTPAnalytics class");
            }
        }

        /// <summary>
        /// Initialize billing service
        /// </summary>
        private static void InitBilling()
        {
            System.Type billingType = System.Type.GetType("Tabtale.TTPlugins.TTPBilling");
#if TTP_BILLING
            TTPBilling.OnBillingInitEvent += OnBillingInitialized;
#else
            SetUserPropertiesAfterPurchase(false);
#endif
            if(billingType != null)
            {
                MethodInfo method = billingType.GetMethod("InternalInit", BindingFlags.NonPublic | BindingFlags.Static);
                if(method != null)
                {
                    method.Invoke(null, null);
                }
                else
                {
                    Debug.LogWarning("TTPCore::InitBilling: method InternalInit not found");
                }
            }
            else
            {
                Debug.Log("TTPCore::InitBilling: TTPBilling not found");
            }
        }
        
#if TTP_BILLING
        private static void OnBillingInitialized(BillerErrors errors)
        {
            SetUserPropertiesAfterPurchase(false);
        }
#endif

        /// <summary>
        /// Initialize delta dna agent
        /// </summary>
        private static void InitDeltaDnaAgent()
        {
            System.Type deltaDnaAgentType = System.Type.GetType("Tabtale.TTPlugins.TTPDeltaDnaAgent");
            if (deltaDnaAgentType != null)
            {
                MethodInfo method = deltaDnaAgentType.GetMethod("InternalInit", BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, null);
                }
                else
                {
                    Debug.LogWarning("TTPCore::InitDeltaDnaAgent: method InternalInit not found");
                }
            }
            else
            {
                Debug.Log("TTPCore::InitDeltaDnaAgent: TTPDeltaDnaAgent not found");
            }
        }

        private static void InitBanners()
        {
            Debug.Log("TTPCore::InitBanners:");
            var banners = System.Type.GetType("Tabtale.TTPlugins.TTPBanners");
            if(banners != null)
            {
                Debug.Log("TTPCore::InitBanners: found class");
                var method = banners.GetMethod("SetupBannersBackgroundIOS", BindingFlags.NonPublic | BindingFlags.Static);
                if(method != null)
                {
                    Debug.Log("TTPCore::InitBanners: found method, invoking");
                    method.Invoke(null, new object[] { GetTTPGameObject() });
                }
            }
        }

#if TTP_GAMEPROGRESSION
        private static void InitGameProgression()
        {
            Debug.Log("TTPCore::InitGameProgression:");
            System.Type unityNativeAds = System.Type.GetType("Tabtale.TTPlugins.TTPGameProgression");
            if (unityNativeAds != null)
            {
                MethodInfo method = unityNativeAds.GetMethod("InternalInit", BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, null);
                }
                else
                {
                    Debug.LogWarning("TTPCore::InitGameProgression: method InternalInit not found");
                }
            }
            else
            {
                Debug.Log("TTPCore::InitGameProgression: TTPGameProgression not found");
            }
        }
#endif

#if TTP_INTERSTITIALS
        private static void InitInterstitialCallbackProxy()
        {
            Debug.Log("TTPCore::InitInterstitialCallbackProxy:");
            System.Type unityNativeAds = System.Type.GetType("Tabtale.TTPlugins.TTPInterstitials");
            if (unityNativeAds != null)
            {
                MethodInfo method = unityNativeAds.GetMethod("InitCallbackProxy", BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, null);
                }
                else
                {
                    Debug.LogWarning("TTPCore::InitInterstitialCallbackProxy: method InitCallbackProxy not found");
                }
            }
            else
            {
                Debug.Log("TTPCore::InitInterstitialCallbackProxy: TTPInterstitials not found");
            }
        }
#endif

#if TTP_REWARDED_ADS
        private static void InitRewardedAdsCallbackProxy()
        {
            Debug.Log("TTPCore::InitRewardedAdsCallbackProxy:");
            System.Type unityNativeAds = System.Type.GetType("Tabtale.TTPlugins.TTPRewardedAds");
            if (unityNativeAds != null)
            {
                MethodInfo method = unityNativeAds.GetMethod("InitCallbackProxy", BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, null);
                }
                else
                {
                    Debug.LogWarning("TTPCore::InitRewardedAdsCallbackProxy: method InitCallbackProxy not found");
                }
            }
            else
            {
                Debug.Log("TTPCore::InitRewardedAdsCallbackProxy: TTPRewardedAds not found");
            }
        }
#endif
        
#if TTP_POPUPMGR
        private static void ReportPurchaseDate(long date)
        {
            Debug.Log("TTPCore::ReportPurchaseDate:");
            System.Type popupMgr = System.Type.GetType("Tabtale.TTPlugins.TTPPopupMgr");
            if (popupMgr == null)
            {
                Debug.Log("TTPCore::ReportPurchaseDate: TTPPopupMgr not found");
                return;
            }
            
            MethodInfo method = popupMgr.GetMethod("ReportPurchaseDate", BindingFlags.NonPublic | BindingFlags.Static);
            if (method != null)
            {
                method.Invoke(null, new object[] { date });
            }
            else
            {
                Debug.LogWarning("TTPCore::ReportPurchaseDate: method TTPPopupMgr::ReportPurchaseDate not found");
            }
        }        
#endif
        
        /// <summary>
        /// Private list of all services is used for reflection in setup process
        /// </summary>
        private static string[] CLASS_LIST = {
            "Tabtale.TTPlugins.TTPPrivacySettings+PrivacySettingsDelegate",
            "Tabtale.TTPlugins.TTPBilling+BillingDelegate",
            "Tabtale.TTPlugins.TTPInterstitials+InterstitialsDelegate",
            "Tabtale.TTPlugins.TTPRewardedAds+RewardedAdsDelegate",
            "Tabtale.TTPlugins.TTPAnalytics+AnalyticsDelegate",
            "Tabtale.TTPlugins.TTPBanners+BannersDelegate",
            "Tabtale.TTPlugins.TTPPromotion+PromotionDelegate",
            "Tabtale.TTPlugins.TTPNativeCampaign+NativeCampaignDelegate",
            "Tabtale.TTPlugins.TTPCrossPromotion+CrossPromotionDelegate",
            "Tabtale.TTPlugins.TTPSocial+SocialDelegate",
            "Tabtale.TTPlugins.TTPCrossDevicePersistency+CDPDelegate",
            "Tabtale.TTPlugins.TTPOnDemandsResources+OnDemandResourcesDelegate",
            "Tabtale.TTPlugins.TTPDeltaDnaAgent+DeltaDnaAgentDelegate",
            "Tabtale.TTPlugins.TTPOpenAds+OpenAdsDelegate",
            "Tabtale.TTPlugins.TTPRewardedInterstitials+RewardedInterstitialsDelegate",
            "Tabtale.TTPlugins.TTPFirebaseEchoAgent",
            "Tabtale.TTPlugins.TTPAdjust+AdjustDelegate"
        };

        private interface ITTPCore {
            void   Setup();
            bool   OnBackPressed();
            bool   IsConnectedToTheInternet();
            bool   IsRemoteConfigExistAndEnabled();
            long   GetSessionNumber();
            string GetEventFromLog(string agent, string eventName);
            void WriteEventToEventsFileHandler(string eventName, string agent, string paramsJSON);
        }

		public interface ITTPInternalService {
		}

		public interface ITTPCoreInternal {
#if UNITY_ANDROID
			AndroidJavaObject GetServiceJavaObject(string serviceClassPath);
            AndroidJavaObject GetCurrentActivity();
            AndroidJavaObject GetServiceManager();
#endif
#if UNITY_IOS
            void CallCrash();
#endif
            string GetPackageInfo();
            string GetConfigurationJson(string serviceName);

		}

#if UNITY_IOS && !TTP_DEV_MODE
	    private class IosImpl : ITTPCore, ITTPCoreInternal, ITTPInternalService {

	        [DllImport("__Internal")]
 	        private static extern void ttpCrashApp();

            [DllImport("__Internal")]
 	        private static extern void ttpSetGeoServiceAlwaysReturnedLocation(string location);

            [DllImport("__Internal")]
 	        private static extern void ttpClearGeoServiceAlwaysReturnedLocation();

	        [DllImport("__Internal")]
            private static extern void ttpSetup();

            [DllImport("__Internal")]
            private static extern string ttpGetPackageInfo();

            [DllImport("__Internal")]
            private static extern string ttpGetConfigurationJson(string serviceName);

            [DllImport("__Internal")]
            private static extern bool ttpIsConnected();

            [DllImport("__Internal")]
            private static extern bool ttpIsRemoteConfigExistAndEnabled();

            [DllImport("__Internal")]
            private static extern long ttpGetSessionNumber();

            [DllImport("__Internal")]
            private static extern void ttpReportIAPToConversion(string currency, float price, string productId, bool consumable);

            [DllImport("__Internal")]
            private static extern string ttpGetEventFromLog(string agent, string eventName);

            [DllImport("__Internal")]
            private static extern void ttpWriteEventToEventsFileHandler(string eventName, string agent, string paramsJSON);

            public void Setup()
            {
                ttpSetup();
            }

            public string GetPackageInfo()
            {
                return ttpGetPackageInfo();
            }

            public bool OnBackPressed() { return false; }

            public string GetConfigurationJson(string serviceName)
            {
                return ttpGetConfigurationJson(serviceName);
            }

            public void CallCrash()
            {
                Debug.Log("TTPCore::IosImpl:CallCrash");
		        ttpCrashApp();
            }

            public bool IsConnectedToTheInternet()
            {
                return ttpIsConnected();
            }

            public void SetGeoServiceAlwaysReturnedLocation(string location)
            {
                ttpSetGeoServiceAlwaysReturnedLocation(location);
            }

            public void ClearGeoServiceAlwaysReturnedLocation()
            {
                ttpClearGeoServiceAlwaysReturnedLocation();
            }

            public bool IsRemoteConfigExistAndEnabled()
            {
                return ttpIsRemoteConfigExistAndEnabled();
            }

            public long GetSessionNumber()
            {
                return ttpGetSessionNumber();
            }

            public void ReportIAPToConversion(string currency, float price, string productId, bool consumable)
            {
                ttpReportIAPToConversion(currency, price, productId, consumable);
            }
            
            public string GetEventFromLog(string agent, string eventName) {
                return ttpGetEventFromLog(agent, eventName);
            }

            public void WriteEventToEventsFileHandler(string eventName, string agent, string paramsJSON) {
                ttpWriteEventToEventsFileHandler(eventName, agent, paramsJSON);
            }
        }
#endif

#if UNITY_ANDROID
		private class AndroidImpl : ITTPCore, ITTPCoreInternal, ITTPInternalService {

			private AndroidJavaObject _serviceManager;
            private AndroidJavaObject _communicationObject;

            private AndroidJavaObject ServiceManager
            {
                get
                {
                    if (_serviceManager == null)
                    {
                        if (_communicationObject != null)
                            _serviceManager = _communicationObject.Call<AndroidJavaObject>("getServiceManager");
                        else
                            Debug.LogError("TTPCore::AndroidImpl:GetServiceJavaObject could not get instance of TTPCommunicationInterface.");
                    }
                    if(_serviceManager == null)
                        Debug.LogError("TTPCore::AndroidImpl:GetServiceJavaObject could not get instance of native android service manager.");
                    return _serviceManager;
                }
            }

			public AndroidImpl() {
                Debug.Log("TTPCore::AndroidImpl created");
                AndroidJavaClass communicationInterface = new AndroidJavaClass("com.tabtale.ttplugins.ttpcore.TTPCommunicationInterface");
                _communicationObject = communicationInterface.CallStatic<AndroidJavaObject>("getInstance");
                if (_communicationObject == null)
                {
                    Debug.LogError("TTPCore::AndroidImpl: could not find class of native android service manager.");
                }
            }

			public AndroidJavaObject GetServiceJavaObject(string serviceGetMethod) {
                if(ServiceManager != null)
                {
                    return _serviceManager.Call<AndroidJavaObject>(serviceGetMethod);
                }
                return null;
			}

			public AndroidJavaObject GetServiceManager() {
                return ServiceManager;
			}

            public void Setup()
            {
                Debug.Log("TTPCore::AndroidImpl::Setup");
                if (_communicationObject != null){
                    _communicationObject.Call("setup");
                }
                else {
                    Debug.LogError("TTPCore::AndroidImpl:Setup could not get instance of TTPUnityMainActivity.");
                }
            }

            public string GetPackageInfo()
            {
                if (_communicationObject != null)
                {
                    return _communicationObject.Call<string>("getPackageInfo");
                }
                else
                {
                    Debug.LogError("TTPCore::AndroidImpl:GetPackageInfo could not get instance of TTPUnityMainActivity.");
                }
                return "";
            }

            public bool OnBackPressed()
            {
                Debug.Log("TTPCore::AndroidImpl:OnBackPressed");
                bool rateUsHandledBackPress = false;
                bool nativeHandledBackPress = false;
                System.Type rateUsClsType = System.Type.GetType("Tabtale.TTPlugins.TTPRateUs");
                if (rateUsClsType != null)
                {
                    MethodInfo method = rateUsClsType.GetMethod("HandleAndroidBackPressed", BindingFlags.NonPublic | BindingFlags.Static);
                    if (method != null)
                    {
                        rateUsHandledBackPress = (bool)method.Invoke(null, null);
                    }
                }
                if (ServiceManager != null)
                {
                    nativeHandledBackPress = ServiceManager.Call<bool>("onBackPressed");
                }
                return rateUsHandledBackPress || nativeHandledBackPress;
            }

            public string GetConfigurationJson(string serviceName)
            {
                if (ServiceManager != null)
                {
                    AndroidJavaObject ttpConfiguration = ServiceManager.Call<AndroidJavaObject>("getTtpConfiguration");
                    if(ttpConfiguration != null)
                    {
                        AndroidJavaObject jsonObject = ttpConfiguration.Call<AndroidJavaObject>("getConfiguration", serviceName);
                        if(jsonObject != null)
                        {
                            return jsonObject.Call<string>("toString");
                        }
                    }
                }
                return null;
            }

            public AndroidJavaObject GetCurrentActivity()
            {
                if (ServiceManager != null)
                {
                    AndroidJavaObject ttpActivity = ServiceManager.Call<AndroidJavaObject>("getActivity");
                    return ttpActivity;
                }
                return null;
            }

            public bool IsConnectedToTheInternet()
            {
                if (ServiceManager != null)
                {
                    AndroidJavaObject ttpConnectivityManager = ServiceManager.Call<AndroidJavaObject>("getConnectivityManager");
                    if(ttpConnectivityManager != null)
                    {
                        return ttpConnectivityManager.Call<bool>("isConnectedToTheInternet");
                    }
                }
                return false;
            }

            public bool IsRemoteConfigExistAndEnabled()
            {
                if (ServiceManager != null)
                {
                    AndroidJavaObject ttpRemoteConfig = ServiceManager.Call<AndroidJavaObject>("getRemoteConfig");
                    if(ttpRemoteConfig != null)
                    {
                        return ttpRemoteConfig.Call<bool>("isEnabled");;
                    }
                }
                return false;
            }

            public long GetSessionNumber()
            {
                if (ServiceManager != null)
                {
                    AndroidJavaObject ttpSessionMgr = ServiceManager.Call<AndroidJavaObject>("getSessionMgr");
                    if(ttpSessionMgr != null)
                    {
                        return ttpSessionMgr.Call<long>("getSessionNumber");
                    }
                }
                return 0;
            }
            
            public string GetEventFromLog(string agent, string eventName) {
                if (ServiceManager != null)
                {
                    return ServiceManager.Call<string>("getEventsFromLogFile", agent, eventName);
                }
                return "";
            }

            public void WriteEventToEventsFileHandler(string eventName, string agent, string paramsJSON) {
                if(ServiceManager != null) {
                    ServiceManager.Call("writeEventToEventsFileHandler", eventName, agent, paramsJSON);
                }
            }
        }
#endif

        private class EditorImpl : ITTPCore, ITTPInternalService, ITTPCoreInternal
        {
            public void Setup() {

				System.Type privacySettingsClsType = System.Type.GetType("Tabtale.TTPlugins.TTPPrivacySettings");
				if (privacySettingsClsType != null)
				{
					MethodInfo method = privacySettingsClsType.GetMethod("TriggerOnConsentModeReady", BindingFlags.NonPublic | BindingFlags.Static);
					if (method != null)
					{
						method.Invoke(null, null);
					}
				}

#if UNITY_EDITOR
                System.Type bannersClsType = System.Type.GetType("Tabtale.TTPlugins.TTPBanners");
                if (bannersClsType != null)
                {
                    MethodInfo method = bannersClsType.GetMethod("TriggerOnBannersReady", BindingFlags.NonPublic | BindingFlags.Static);
                    if (method != null)
                    {
                        method.Invoke(null, null);
                    }
                }
#endif

            }
            public bool OnBackPressed() {
                bool rateUsHandledBackPress = false;
                System.Type rateUsClsType = System.Type.GetType("Tabtale.TTPlugins.TTPRateUs");
                if (rateUsClsType != null)
                {
                    MethodInfo method = rateUsClsType.GetMethod("HandleAndroidBackPressed", BindingFlags.NonPublic | BindingFlags.Static);
                    if (method != null)
                    {
                        rateUsHandledBackPress = (bool)method.Invoke(null, null);
                    }
                }
                return rateUsHandledBackPress;
            }
#if UNITY_ANDROID
            public AndroidJavaObject GetServiceJavaObject(string serviceClassPath) { return null;}
            public AndroidJavaObject GetServiceManager() { return null; }
#endif
#if UNITY_IOS
            public void CallCrash()
            {
                Debug.Log("TTPCore::EditorImpl:CallCrash");
            }
#endif
            public string GetPackageInfo()
            {
                return "";
            }
            public string GetConfigurationJson(string serviceName)
            {
#if UNITY_EDITOR
                var platformCode = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ? "gp" : "ios";
                return TTPUtils.ReadStreamingAssetsFile("ttp/templateconfig/" + platformCode + "/" + serviceName + ".json");
#else
                return TTPUtils.ReadStreamingAssetsFile("ttp/configurations/" + serviceName + ".json");
#endif
            }

            public AndroidJavaObject GetCurrentActivity()
            {
                return null;
            }

            public bool IsConnectedToTheInternet()
            {
                return true;
            }

            public bool IsRemoteConfigExistAndEnabled()
            {
                return false;
            }

            public long GetSessionNumber()
            {
                return 0;
            }
            
            public string GetEventFromLog(string agent, string eventName) {
                return "";
            }

            public void WriteEventToEventsFileHandler(string eventName, string agent, string paramsJSON)
            {
                
            }
        }

		private static ITTPInternalService _impl;
		public static ITTPInternalService Impl {
			get {

                if (_impl == null) {
                    if (DevMode)
                    {
                        _impl = new EditorImpl();
                    }
                    else if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android ||
					    UnityEngine.Application.platform == UnityEngine.RuntimePlatform.IPhonePlayer) {
#if UNITY_ANDROID
						_impl = new AndroidImpl ();
#endif
#if UNITY_IOS && !TTP_DEV_MODE
						_impl = new IosImpl();
#endif
                    }
                    else {
						_impl = new EditorImpl ();
					}
				}
				if (_impl == null) {
					Debug.LogError ("TTPCore::Impl: failed to create native impl");
				}
				return _impl;
			}
		}

        /// <summary>
        /// Use this class to have events coming back from native code
        /// </summary>
        public class TTPGameObject : MonoBehaviour
        {
            /// <summary>
            /// Apllication became active event
            /// </summary>
            public event System.Action OnApplicationFocusEvent;

            /// <summary>
            /// Application paused event
            /// </summary>
            public event System.Action OnApplicationPauseEvent;

            private void Start()
            {
                DontDestroyOnLoad(this);
            }

            private void OnApplicationFocus(bool focus)
            {
                Debug.Log("TTPCore::TTPGameObject:OnApplicationFocus:focus=" + focus);
                if (focus && OnApplicationFocusEvent != null)
                    OnApplicationFocusEvent();
            }

            private void OnApplicationPause(bool pause)
            {
                Debug.Log("TTPCore::TTPGameObject:OnApplicationPause:pause=" + pause);
                if (pause && OnApplicationPauseEvent != null)
                    OnApplicationPauseEvent();
            }

            private void OnDestroy()
            {
                Debug.Log("TTPCore::TTPGameObject:OnDestroy:");
            }
        }

        private static GameObject _ttpGameObject;
        private static GameObject TTPluginsGameObject
        {
            get
            {
                if (_ttpGameObject == null)
                {
                    _ttpGameObject = new GameObject("TTPluginsGameObject");
                    _ttpGameObject.AddComponent<TTPGameObject>();
                }
                return _ttpGameObject;
            }
        }
        [Preserve]
        static GameObject GetTTPGameObject()
        {
            return TTPluginsGameObject;
        }

        private static TTPSoundMgr _soundMgr;

        /// <summary>
        /// A singleton of sound manager
        /// </summary>
        public static ITTPInternalService SoundMgr
        {
            get
            {
                if (_soundMgr == null)
                    _soundMgr = new TTPSoundMgr();
                return _soundMgr;
            }
        }

        /// <summary>
        /// This class provides sound management for interstitials, banners and rewarded ads
        /// </summary>
        public class TTPSoundMgr : ITTPInternalService
        {
            /// <summary>
            /// Enumeration of callers interstitials, banners and rewarded ads
            /// </summary>
            public enum Caller
            {
                INTERSTITIAL, REWARDED_ADS, BANNERS, OPEN_ADS, REWARDED_INTER
            }

            private Dictionary<Caller, bool> _musicPauseDic;

            /// <summary>
            /// Notify t pause or resume music for required caller
            /// </summary>
            /// <param name="pause">True - if needs to pause music</param>
            /// <param name="caller">Kind of caller interstitials, banners and rewarded ads</param>
            public void PauseGameMusic(bool pause, Caller caller)
            {
                if (_musicPauseDic == null)
                {
                    _musicPauseDic = new Dictionary<Caller, bool>();
                }
                _musicPauseDic[caller] = pause;
                bool allUnpaused = true;
                foreach (KeyValuePair<Caller, bool> kvPair in _musicPauseDic)
                {
                    Debug.Log("TTPSoundMgr::PauseGameMusic: current music pause dictionary entry :: " + kvPair.Key + ", " + kvPair.Value);
                    if (kvPair.Value)
                    {
                        allUnpaused = false;
                    }

                }
                bool shouldCallEvent = pause || (!pause && allUnpaused);
                if (shouldCallEvent)
                {
                    if (PauseGameMusicEvent != null)
                        PauseGameMusicEvent(pause);
                }
            }
        }

        /// <summary>
        /// This class provides notifications about changes using events.
        /// Add this class as a unity component for compatibility with SendUnityMessage.
        /// </summary>
        public class CoreDelegate : MonoBehaviour
        {
            /// <summary>
            /// Notify about new session
            /// </summary>
            /// <param name="message">A message for new session</param>
            public void OnNewTTPSession(string message)
            {
                Debug.Log("CoreDelegate:: OnNewTTPSession");
                if(OnNewTTPSessionEvent != null)
                {
                    OnNewTTPSessionEvent();
                }

            }

            public void OnRemoteConfigUpdate(string message)
            {
                Debug.Log("CoreDelegate::OnRemoteConfigUpdate:message=" + message);
                if(OnRemoteConfigUpdateEvent == null) return;
                if(string.IsNullOrEmpty(message)) return;
                var remoteConfig = TTPJson.Deserialize(message) as Dictionary<string, object>;
                OnRemoteConfigUpdateEvent(remoteConfig);

            }

            public void OnPopupShown(string message)
            {
                Debug.Log("CoreDelegate::onPopupShown:");
                if (OnPopupShownEvent != null)
                {
                    OnPopupShownEvent();
                }
            }

            public void OnPopupClosed(string message)
            {
                Debug.Log("CoreDelegate::OnPopupClosed:");
                if (OnPopupClosedEvent != null)
                {
                    OnPopupClosedEvent();
                }
            }
        }
    }
}
