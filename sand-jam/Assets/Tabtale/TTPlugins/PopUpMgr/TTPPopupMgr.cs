#if TTP_POPUPMGR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Tabtale.TTPlugins
{
    public class TTPPopupMgr
    {

        private interface ITTPPopupMgrInternal
        {
            bool ShouldShow(string source);
            void OnShown(string source);
            void OnClosed(string source);
            void NotifyPurchaseHasStarted();
            void NotifyPurchaseHasFinished();
        }

        private static ITTPPopupMgrInternal _impl;
        private static ITTPPopupMgrInternal Impl
        {
            get
            {
                if (_impl == null)
                {
                    if (TTPCore.DevMode)
                    {
                        _impl = new UnityTTPPopupMgr();
                    }
                    else if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android ||
                        UnityEngine.Application.platform == UnityEngine.RuntimePlatform.IPhonePlayer)
                    {
#if UNITY_ANDROID
                        _impl = new AndroidTTPPopupMgr();
#endif
#if UNITY_IOS && !TTP_DEV_MODE
                        _impl = new IosTTPPopupMgr();
#endif

                    }
                    else
                    {
#if UNITY_EDITOR
                        _impl = new UnityTTPPopupMgr();
#endif
                    }
                }
                if (_impl == null)
                {
                    Debug.LogError("");
                }
                return _impl;
            }
        }

        [Preserve]
        static void ReportPurchaseDate(long date)
        {
            TTPLogger.Log("TTPPopupMgr::ReportPurchaseDate:date=" + date);
#if UNITY_ANDROID
            if (Impl?.GetType() == typeof(AndroidTTPPopupMgr))
            {
                ((AndroidTTPPopupMgr)Impl).ReportPurchaseDate(date);
            }
#elif UNITY_IOS && !TTP_DEV_MODE
            if (Impl?.GetType() == typeof(IosTTPPopupMgr))
            {
                ((IosTTPPopupMgr)Impl).ReportPurchaseDate(date);
            }
#endif
        }
        
        public static bool ShouldShow(string source)
        {
            TTPLogger.Log("TTPPopupMgr::ShouldShow:source=" + source);
            return Impl.ShouldShow(source);
        }

        public static void OnShown(string source)
        {
            TTPLogger.Log("TTPPopupMgr::OnShown:source=" + source);
            Impl.OnShown(source);
        }

        public static void OnClosed(string source)
        {
            TTPLogger.Log("TTPPopupMgr::OnClosed:source=" + source);
            Impl.OnClosed(source);
        }
        
        public static void NotifyPurchaseHasStarted()
        {
            TTPLogger.Log("TTPPopupMgr::NotifyPurchaseHasStarted:");
            Impl.NotifyPurchaseHasStarted();
        }
        
        public static void NotifyPurchaseHasFinished()
        {
            TTPLogger.Log("TTPPopupMgr::NotifyPurchaseHasFinished:");
            Impl.NotifyPurchaseHasFinished();
        }
        //public static bool OnClose(string source)
        //{
        //    return Impl.ShouldShow(source);
        //}

        //================================================================================================================================
        //Android Impl 
        //================================================================================================================================
#if UNITY_ANDROID
    private class AndroidTTPPopupMgr : ITTPPopupMgrInternal {

            private const string SERVICE_GET_METHOD = "getPopUpMgr";

            private AndroidJavaObject _serviceJavaObject;

            private AndroidJavaObject ServiceJavaObject
            {
                get
                {
                    if (_serviceJavaObject == null)
                    {
                        _serviceJavaObject = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetServiceJavaObject(SERVICE_GET_METHOD);
                    }
                    if (_serviceJavaObject == null)
                        Debug.LogError("TTPInterstitials::AndroidImpl: failed to get native instance.");
                    return _serviceJavaObject;
                }
            }
            
        public bool ShouldShow(string source)
        {
            if (ServiceJavaObject != null)
            {
                    return ServiceJavaObject.Call<bool>("shouldShow", new object[] { source });
            }
            else
            {
                Debug.LogError("");
            }
            return false;
        }

        public void OnShown(string source)
        {
            if (ServiceJavaObject != null)
            {
                ServiceJavaObject.Call("onShow", new object[] { source });
            }
            else
            {
                Debug.LogError("");
            }
        }

        public void OnClosed(string source)
        {
            if (ServiceJavaObject != null)
            {
                ServiceJavaObject.Call("onClose", new object[] { source });
            }
            else
            {
                Debug.LogError("");
            }
        }

        public void ReportPurchaseDate(long date)
        {
            if (ServiceJavaObject == null)
            {
                Debug.LogError("reportPurchaseDate not called");
                return;
            }
            ServiceJavaObject.Call("reportPurchaseDate", new object[] { date });
        }

        // Unused in Android
        public void NotifyPurchaseHasStarted()
        {
            
        }

        // Unused in Android
        public void NotifyPurchaseHasFinished()
        {
            
        }
    }
#endif
        //================================================================================================================================
        //END Android Impl 
        //================================================================================================================================


        //================================================================================================================================
        // IOS Impl 
        //================================================================================================================================
#if UNITY_IOS && !TTP_DEV_MODE
        public class IosTTPPopupMgr : ITTPPopupMgrInternal {

        [DllImport("__Internal")]
        private static extern bool ttpPopupMgrShouldShow(string source);
        [DllImport("__Internal")]
        private static extern void ttpPopupMgrReportPurchaseDate(long date);
        [DllImport("__Internal")]
        private static extern void ttpPopupMgrNotifyPurchaseHasStarted();
        [DllImport("__Internal")]
        private static extern void ttpPopupMgrNotifyPurchaseHasFinished();

        //[DllImport("__Internal")]
        //private static extern void ttpPopupMgrOnShown(string source);

        public bool ShouldShow(string source)
        {
             return ttpPopupMgrShouldShow(source);
        }

        //Unused in IOS
        public void OnShown(string source)
        {
        }

        //Unused in IOS
        public void OnClosed(string source)
        {
        }

        public void ReportPurchaseDate(long date)
        {
            ttpPopupMgrReportPurchaseDate(date);
        }
        
        public void NotifyPurchaseHasStarted()
        {
            ttpPopupMgrNotifyPurchaseHasStarted();
        }

        public void NotifyPurchaseHasFinished()
        {
            ttpPopupMgrNotifyPurchaseHasFinished();
        }
        }
#endif
        //================================================================================================================================
        // END IOS Impl 
        //================================================================================================================================


        //================================================================================================================================
        // Editor Impl 
        //================================================================================================================================
        //#if UNITY_EDITOR
        private class UnityTTPPopupMgr : ITTPPopupMgrInternal
        {

            public bool ShouldShow(string source)
            {
                return true;
            }

            public void OnShown(string source)
            {
                Debug.Log("TTPPopupMgr::OnShown: ");

            }
            public void OnClosed(string source)
            {
                Debug.Log("TTPPopupMgr::OnClosed: ");
            }

            public void ReportPurchaseDate(long date)
            {
                Debug.Log("TTPPopupMgr::ReportPurchaseDate: ");
            }
            
            public void NotifyPurchaseHasStarted()
            {
                Debug.Log("TTPPopupMgr::NotifyPurchaseHasStarted: ");
            }

            public void NotifyPurchaseHasFinished()
            {
                Debug.Log("TTPPopupMgr::NotifyPurchaseHasFinished: ");
            }
        }
//#endif
    }
}
#endif