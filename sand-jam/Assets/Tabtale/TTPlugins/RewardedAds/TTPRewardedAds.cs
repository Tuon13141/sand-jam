#if TTP_REWARDED_ADS
using System.Collections;
using System.Collections.Generic;
using AOT;
using UnityEngine;
using UnityEngine.Scripting;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tabtale.TTPlugins
{
    /// <summary>
    /// This class controls rewarded ads behaviour. Mediation provider can be AdMob or moPub
    /// </summary>
    public class TTPRewardedAds
    {
        // The name of default location should be the same as on Android and iOS
        public const string DEFAULT_LOCATION = "default";
        
        /// <summary>
        /// Interstitials are ready for showing event
        /// </summary>
        public static event System.Action<bool> ReadyEvent;

        /// <summary>
        /// Interstitials are ready for showing event in specific location
        /// </summary>
        public static event System.Action<bool, string> ReadyEventLocation;
        
        /// <summary>
        /// Notifies ad revenue after ad was loaded
        /// </summary>
        public static event System.Action<double> OnAdRevenueReceivedCallback;

        private static bool _allowRevenuePaidEventSubscribers = true;
        
        private static event System.Action<TTPILRDData> _onRevenuePaidEvent;
        public static event System.Action<TTPILRDData> OnRevenuePaidEvent
        {
            add
            {
                if (!_allowRevenuePaidEventSubscribers)
                {
                    Debug.LogError("TTPRewardedAds::OnRevenuePaidEvent: ERROR! You have to subscribe to OnRevenuePaidEvent before call TTP Setup.");
                    return;
                }
                
                Debug.Log("TTPRewardedAds::Subscriber added to OnRevenuePaidEvent");
                _onRevenuePaidEvent += value;
            }
            remove
            {
                Debug.Log("TTPRewardedAds::Subscriber removed from OnRevenuePaidEvent");
                _onRevenuePaidEvent -= value;
            }
        }

        /// <summary>
        /// Get result if view has been already shown
        /// </summary>
        private static System.Action<bool, TTPILRDData> _onResultActionILRD;


        /// <summary>
        /// Get result if view has been already shown
        /// </summary>
        private static System.Action<bool> _onResultAction;
        
#if UNITY_IOS && !UNITY_EDITOR && !TTP_DEV_MODE
        private delegate void UnityBackgroundRevenueCallback(string message);
        
        [DllImport("__Internal")]
        private static extern void ttpRewardedAdsSetBackgroundRevenueCallback(UnityBackgroundRevenueCallback backgroundRevenueCallback);
#endif

#if !UNITY_EDITOR && !TTP_DEV_MODE
        // Used by reflection when we do initialization of Core
        static void InitCallbackProxy()
        {
            Debug.Log("TTPRewardedAds::InitCallbackProxy:");
            _allowRevenuePaidEventSubscribers = false;
            if (_onRevenuePaidEvent == null)
            {
                Debug.Log("TTPRewardedAds::InitCallbackProxy:No subscribers for OnRevenuePaidEvent, skipping adding background callback");
                return;
            }
#if UNITY_ANDROID
            if (Impl != null && Impl.GetType() == typeof(AndroidImpl))
            {
                ((AndroidImpl)Impl).SetBackgroundRevenueCallbackIfNeeded();
            }
#endif
#if UNITY_IOS
            if (Impl != null && Impl.GetType() == typeof(IosImpl))
            {
                ((IosImpl)Impl).SetBackgroundRevenueCallbackIfNeeded();
            }
#endif
        }
#endif
        
        static void SendRevenueCallback(string message)
        {
            TTPLogger.Log("TTPRewardedAds::SendRevenueCallback:");
            if (_onRevenuePaidEvent != null)
            {
                TTPILRDData ilrdData = null;
                if (!string.IsNullOrEmpty(message))
                {
                    ilrdData = JsonUtility.FromJson<TTPILRDData>(message);
                }
                _onRevenuePaidEvent.Invoke(ilrdData);
            }
        }
        
        /// <summary>
        /// Show rewarded ads view in the scene
        /// </summary>
        /// <param name="location">Location for showing</param>
        /// <param name="onResultAction">Action delegate for result</param>
        /// <returns>True - if plugin is implemented</returns>
        public static bool ShowWithILRD(string location, System.Action<bool, TTPILRDData> onResultAction)
        {
            TTPLogger.Log("TTPRewardedAds::ShowWithILRD:location=" + location);
            _onResultActionILRD = onResultAction;
            _onResultAction = null;
            if (Impl != null)
            {
                if (Impl.Show(location))
                {
                    return true;
                }
                else
                {
                    _onResultActionILRD.Invoke(false, null);
                }
            }
            return false;
        }

        /// <summary>
        /// Show rewarded ads view in the scene
        /// </summary>
        /// <param name="location">Location for showing</param>
        /// <param name="onResultAction">Action delegate for result</param>
        /// <returns>True - if plugin is implemented</returns>
        public static bool Show(string location, System.Action<bool> onResultAction)
        {
            TTPLogger.Log("TTPRewardedAds::Show:location=" + location);
            _onResultAction = onResultAction;
            _onResultActionILRD = null;
            if (Impl != null)
            {
                if (Impl.Show(location))
                {
                    return true;
                }
                else
                {
                    _onResultAction.Invoke(false);
                }
            }
            return false;
        }
        
        /// <summary>
        /// Indicates that rewarded ads are ready to show for default location
        /// </summary>
        /// <returns>True - rewarded ads are ready</returns>
        public static bool IsReady()
        {
            return IsReady(DEFAULT_LOCATION);
        }

        /// <summary>
        /// Indicates that rewarded ads are ready to show for specific location
        /// </summary>
        /// <returns>True - rewarded ads are ready</returns>
        public static bool IsReady(string location)
        {
            TTPLogger.Log("TTPRewardedAds::IsReady:location=" + location);
            if (Impl != null)
            {
                return Impl.IsReady(location);
            }
            return false;
        }
        
        private interface ITTPRewardedAds
        {
            bool Show(string location);
            bool IsReady(string location);
        }

        private static ITTPRewardedAds _impl;
        private static ITTPRewardedAds Impl
        {
            get
            {
                if (_impl == null)
                {
                    if (TTPCore.IncludedServices != null && !TTPCore.IncludedServices.rvs)
                    {
                        _impl = new EmptyImpl();
                    }
                    else if (TTPCore.DevMode)
                    {
                        _impl = new EditorImpl();
                    }
                    else if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android ||
                        UnityEngine.Application.platform == UnityEngine.RuntimePlatform.IPhonePlayer)
                    {
#if UNITY_ANDROID && !UNITY_EDITOR
                        _impl = new AndroidImpl ();
#endif
#if UNITY_IOS && !UNITY_EDITOR && !TTP_DEV_MODE
                        _impl = new IosImpl();
#endif

                    }
                    else
                    {
#if UNITY_EDITOR
                        _impl = new EditorImpl();
#endif
                    }
                }
                if (_impl == null)
                {
                    Debug.LogError("TTPRewardedAds::Impl: failed to create native impl");
                }
                return _impl;
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private class AndroidImpl : ITTPRewardedAds
        {
            private const string SERVICE_GET_METHOD = "getRewardedAds";

            private AndroidJavaObject _serivceJavaObject;
            
            private static readonly BackgroundRevenueCallbackProxy BackgroundRevenueCallback = new BackgroundRevenueCallbackProxy();

            private AndroidJavaObject ServiceJavaObject
            {
                get
                {
                    if (_serivceJavaObject == null)
                    {
                        _serivceJavaObject = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetServiceJavaObject(SERVICE_GET_METHOD);
                    }
                    if (_serivceJavaObject == null)
                        Debug.LogError("TTPRewardedAds::AndroidImpl: failed to get native instance.");
                    return _serivceJavaObject;
                }
            }

            public bool Show(string location)
            {
                if (ServiceJavaObject != null)
                {
                    return ServiceJavaObject.Call<bool>("show", new object[] { location });
                }
                return false;
            }

            public bool IsReady(string location)
            {
                if (ServiceJavaObject != null)
                {
                    return ServiceJavaObject.Call<bool>("isReady", new object[] { location });
                }
                return false;
            }

            public void SetBackgroundRevenueCallbackIfNeeded()
            {
                var TtpRewardedAdsAndroidJavaClass =
                    new AndroidJavaClass("com.tabtale.ttplugins.tt_plugins_rewardedads.TTPRewardedAdsServiceImpl");
                TtpRewardedAdsAndroidJavaClass.CallStatic("setBackgroundRevenueCallback", BackgroundRevenueCallback);
            }
        }
#endif
#if UNITY_IOS && !UNITY_EDITOR && !TTP_DEV_MODE
        private class IosImpl : ITTPRewardedAds
        {
            private bool _isSubscribedToRevenueCallback = false;

            [DllImport("__Internal")]
            private static extern bool ttpRewardedAdsShow(string location);

            [DllImport("__Internal")]
            private static extern bool ttpRewardedAdsIsReady(string location);

            [DllImport("__Internal")]
            private static extern bool ttpRewardedAdsInstanceExists();

            public bool Show(string location)
            {
                SetBackgroundRevenueCallbackIfNeeded();
                return ttpRewardedAdsShow(location);
            }
            public bool IsReady(string location)
            {
                return ttpRewardedAdsIsReady(location);
            }

            public void SetBackgroundRevenueCallbackIfNeeded()
            {
                Debug.Log("TTPRewardedAds::IosImpl::SetBackgroundRevenueCallbackIfNeeded:");
                if (ttpRewardedAdsInstanceExists() && _onRevenuePaidEvent != null && !_isSubscribedToRevenueCallback)
                {
                    Debug.Log("TTPRewardedAds::IosImpl::SetBackgroundRevenueCallbackIfNeeded: will set callback");

                    _isSubscribedToRevenueCallback = true;
                    ttpRewardedAdsSetBackgroundRevenueCallback(BackgroundRevenueCallback);
                }
            }
        }
#endif
        //#if UNITY_EDITOR
        private class EditorImpl : ITTPRewardedAds
        {
            private GameObject interCanvas;

            public System.Action _onClosedAction = () => {
                if (ReadyEvent != null)
                {
                    ReadyEvent(true);
                }
            };

            public EditorImpl()
            {
                if (ReadyEvent != null)
                {
                    ReadyEvent(true);
                }
                
                if (ReadyEventLocation != null)
                {
                    ReadyEventLocation(true, DEFAULT_LOCATION);
                }
            }

            public bool Show(string location)
            {
                if (interCanvas == null)
                {
                    interCanvas = Resources.Load<GameObject>("Prefabs/TTPRewardedAdsCanvas");
                    interCanvas = GameObject.Instantiate(interCanvas);
                    interCanvas.name = "TTPRewardedAdsCanvas";
                }
                interCanvas.SetActive(true);
                return true;
            }
            public bool IsReady(string location)
            {
                return true;
            }

        }
        //#endif

        private class EmptyImpl : ITTPRewardedAds
        {
            public bool Show(string location)
            {
                return false;
            }

            public bool IsReady(string location)
            {
                return false;
            }
        }

        /// <summary>
        /// This class provides notifications about changes using events.
        /// Add this class as a unity component for compatibility with SendUnityMessage.
        /// </summary>
        [Preserve]
        public class RewardedAdsDelegate : MonoBehaviour
        {
            [System.Serializable]
            private class OnLoadedMessage
            {
                public bool loaded = false;
                public string error = null;
                public string location = "";
            }

            [System.Serializable]
            private class OnClosedMessage
            {
                public bool shouldReward;
            }

            private class OnAdRevenueMessage
            {
                public double revenue = 0.0;
            }

            public void OnRewardedAdsReady(string message)
            {
                if (message != null)
                {
                    Debug.Log("RewardedAdsDelegate::OnRewardedAdsReady: " + message);
                    OnLoadedMessage onLoadedMessage = JsonUtilityWrapper.FromJson<OnLoadedMessage>(message);
                    if (onLoadedMessage != null)
                    {
                        if (onLoadedMessage.location == DEFAULT_LOCATION)
                        {
                            if (ReadyEvent != null)
                            {
                                ReadyEvent(onLoadedMessage.loaded);
                            }
                        }

                        if (ReadyEventLocation != null)
                        {
                            ReadyEventLocation(onLoadedMessage.loaded, onLoadedMessage.location);
                        }
                    }
                }
            }

            public void OnRewardedAdsRevenueReceived(string message)
            {
                if (message != null)
                {
                    Debug.Log("RewardedAdsDelegate::OnRewardedAdsRevenueReceived: " + message);
                    if (OnAdRevenueReceivedCallback != null)
                    {
                        OnAdRevenueMessage revenueMessage = JsonUtilityWrapper.FromJson<OnAdRevenueMessage>(message);
                        if (revenueMessage != null)
                        {
                            OnAdRevenueReceivedCallback(revenueMessage.revenue);
                        }
                    }
                }
            }

            public void OnRewardedAdsShown(string message)
            {
                ((TTPCore.TTPSoundMgr)TTPCore.SoundMgr).PauseGameMusic(true, TTPCore.TTPSoundMgr.Caller.REWARDED_ADS);
            }

            public void OnRewardedAdsClosed(string message)
            {
                ((TTPCore.TTPSoundMgr)TTPCore.SoundMgr).PauseGameMusic(false, TTPCore.TTPSoundMgr.Caller.REWARDED_ADS);
                if(message != null)
                {
                    Debug.Log("RewardedAdsDelegate::OnRewardedAdsClosed: " + message);
                    var onClosedMessage = JsonUtilityWrapper.FromJson<OnClosedMessage>(message);
                    var ilrdData = JsonUtilityWrapper.FromJson<TTPILRDData>(message);
                    if (onClosedMessage != null)
                    {
                        if (_onResultActionILRD != null)
                        {
                            _onResultActionILRD.Invoke(onClosedMessage.shouldReward, ilrdData);
                        }
                        else if (_onResultAction != null)
                        {
                            _onResultAction.Invoke(onClosedMessage.shouldReward);

                        }
                        _onResultAction = null;
                        _onResultActionILRD = null;

                        if (Impl != null && Impl.GetType() == typeof(EditorImpl) && ((EditorImpl)Impl)._onClosedAction != null)
                        {
                            ((EditorImpl)_impl)._onClosedAction.Invoke();
                        }
                    }
                }

            }
        }
        
#if UNITY_ANDROID && !UNITY_EDITOR
        internal class BackgroundRevenueCallbackProxy : AndroidJavaProxy
        {
            public BackgroundRevenueCallbackProxy() : base("com.tabtale.ttplugins.tt_plugins_rewardedads.TTPRewardedAdsServiceImpl$BackgroundRevenueCallback") { }

            public void onRevenuePaid(string message)
            {
                Debug.Log("TTPRewardedAds:BackgroundRevenueCallbackProxy:: onRevenuePaid: " + message);
                SendRevenueCallback(message);
            }
        }
#elif UNITY_IOS && !UNITY_EDITOR && !TTP_DEV_MODE
        [MonoPInvokeCallback((typeof(UnityBackgroundRevenueCallback)))]
        internal static void BackgroundRevenueCallback(string message)
        {
            Debug.Log("TTPRewardedAds:BackgroundRevenueCallback:: onRevenuePaid: " + message);
            SendRevenueCallback(message);
        }
#endif
    }
}
#endif