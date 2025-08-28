#if TTP_ADJUST
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
	/// <summary>
	/// This enum defines the mediation types
	/// </summary>
	public enum TTPMediationType
	{
		admob,
		max
	}

	/// <summary>
    /// This class provides logging to Adjust service
    /// </summary>
	public class TTPAdjust
	{
		
		public static event System.Action<string> OnAdjustIdReceivedEvent;
		public static event System.Action<string> OnAdjustDeeplinkReceivedEvent;
		
		/// <summary>
		/// This class hold the data needed for reporting the ad view in Adjust
		/// </summary>
		public class AdViewParams
		{
			public readonly float Revenue;
			public readonly string Currency;
			public readonly string Type;
			public readonly string Network;
			public readonly string NetworkPlacement;
			public readonly string Placement;
			public readonly string CreativeIdentifier;
			public readonly TTPMediationType MediationType; 
		
			public AdViewParams(
				float revenue,
				string currency,
				string type,
				string network,
				string networkPlacement,
				string placement,
				string creativeIdentifier,
				TTPMediationType mediationType
			)
			{
				this.Revenue = revenue;
				this.Currency = currency;
				this.Type = type;
				this.Network = network;
				this.NetworkPlacement = networkPlacement;
				this.Placement = placement;
				this.CreativeIdentifier = creativeIdentifier;
				this.MediationType = mediationType;
			}
		}
		
		private static IAdjust _impl;
		private static IAdjust Impl
		{
			get
			{
				if (_impl == null)
				{
					if (TTPCore.IncludedServices != null && !TTPCore.IncludedServices.adjust)
					{
						_impl = new EditorImpl();
					}
					else if (TTPCore.DevMode)
                    {
                        _impl = new EditorImpl();
                    }
                    else if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android ||
						UnityEngine.Application.platform == UnityEngine.RuntimePlatform.IPhonePlayer)
					{
#if UNITY_ANDROID
						_impl = new AndroidImpl ();
#endif
#if UNITY_IOS && !TTP_DEV_MODE
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
					Debug.LogError("TTPAdjust::Impl: failed to create native impl");
				}
				return _impl;
			}
		}

		/// <summary>
        /// Log events to Adjust
        /// </summary>
        /// <param name="eventToken">Token of the event</param>
        /// <param name="eventParams">Parameters dictionary logging with event</param>
		/// <param name="revenue">The revenue that is being tracked</param>
		/// <param name="currency">The currency of the revenue</param>
		public static void LogEvent(string eventToken, IDictionary<string, object> eventParams, float revenue, string currency = "USD")
		{
			TTPLogger.Log("TTPAdjust::LogEvent:eventToken=" + eventToken);
			if (revenue == 0f)
			{
				Debug.LogWarning("TTPAdjust::ATTENTION: revenue is set to 0 for this event (" + eventToken +
				                 "). It will not be included but the event will be sent. " +
				                 "0 revenue causes issues with UA campaigns and is not allowed to be sent as 0.");
			}
			if (Impl != null)
				Impl.LogEvent(eventToken, eventParams, revenue, currency);
		}

		public static void LogEvent(string eventToken, IDictionary<string, object> eventParams)
		{
			TTPLogger.Log("TTPAdjust::LogEvent:eventToken=" + eventToken);
			if (Impl != null)
				Impl.LogNonRevenueEvent(eventToken, eventParams);
		}
		
		/// <summary>
		/// Log events to Adjust
		/// </summary>
		/// <param name="adViewParams">Object that holds the params that needs to be reported</param>
		public static void ReportAdView(AdViewParams adViewParams)
		{
			TTPLogger.Log("TTPAdjust::ReportAdView:");
			if (Impl != null)
				Impl.ReportAdView(adViewParams);
		}

		public static string GetAdjustId()
		{
			TTPLogger.Log("TTPAdjust::GetAdjustId:");
			if (Impl != null)
			{
				return Impl.GetAdjustId();
			}

			return "NA";
		}
		
		/// <summary>
		/// Private interface for methods
		/// </summary>
		private interface IAdjust
		{
			void LogEvent (string eventToken, IDictionary<string, object> eventParams, float revenue, string currency = "USD");
			void LogNonRevenueEvent(string eventToken, IDictionary<string, object> eventParams);
			void ReportAdView(AdViewParams adViewParams);
			string GetAdjustId();
		}



#if UNITY_IOS && !TTP_DEV_MODE

		private class IosImpl : IAdjust
		{
			[DllImport("__Internal")]
			private static extern void ttpAdjustLogEvent(string eventToken, string eventParamsJson, float revenue, string currency = "USD");
			
			[DllImport("__Internal")]
			private static extern void ttpAdjustNonRevenueLogEvent(string eventToken, string eventParamsJson);

			[DllImport("__Internal")]
			private static extern void ttpAdjustReportAdView(int mediationType, float revenue, string currency, string type, string network,  string networkPlacement, string placement, string creativeIdentifier);
			
			[DllImport("__Internal")]
			private static extern string ttpGetAdjustId();
			
			public void LogEvent (string eventToken, IDictionary<string, object> eventParams, float revenue, string currency = "USD")
			{
				ttpAdjustLogEvent(eventToken, eventParams != null ? TTPJson.Serialize(eventParams) : "{}", revenue, currency);
			}

			public void LogNonRevenueEvent(string eventToken, IDictionary<string, object> eventParams)
			{
				ttpAdjustNonRevenueLogEvent(eventToken, eventParams != null ? TTPJson.Serialize(eventParams) : "{}");
			}

			public void ReportAdView(AdViewParams adViewParams)
			{
				var mediationType = (adViewParams.MediationType == TTPMediationType.admob) ? 0 : 1;
				ttpAdjustReportAdView(
					mediationType,
					adViewParams.Revenue,
					adViewParams.Currency,
					adViewParams.Type,
					adViewParams.Network,
					adViewParams.NetworkPlacement,
					adViewParams.Placement,
					adViewParams.CreativeIdentifier);
			}

			public string GetAdjustId()
			{
				return ttpGetAdjustId();
			}
		}

#endif



#if UNITY_ANDROID
		private class AndroidImpl : IAdjust
		{
			private const string SERVICE_GET_METHOD = "getAdjust";

			private AndroidJavaObject _serivceJavaObject;

			private AndroidJavaObject ServiceJavaObject
			{
				get
				{
					if (_serivceJavaObject == null)
					{
						_serivceJavaObject = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetServiceJavaObject(SERVICE_GET_METHOD);
					}
					if (_serivceJavaObject == null) {
						Debug.LogError ("TTPAdjust::AndroidImpl: failed to get Adjust Tool native instance.");
					}
					return _serivceJavaObject;
				}
			}


			// methods for the native 

			public void LogEvent(string eventToken, IDictionary<string, object> eventParams, float revenue, string currency = "USD")
			{
				if (ServiceJavaObject != null)
				{
					var eventParamsJson = eventParams != null ? TTPJson.Serialize(eventParams) : "{}";
					ServiceJavaObject.Call("logEventUnity", new object[] {eventToken, eventParamsJson, revenue, currency});
				}
			}
			
			public void LogNonRevenueEvent(string eventToken, IDictionary<string, object> eventParams)
			{
				if (ServiceJavaObject != null)
				{
					var eventParamsJson = eventParams != null ? TTPJson.Serialize(eventParams) : "{}";
					ServiceJavaObject.Call("logNonRevenueEventUnity", new object[] {eventToken, eventParamsJson});
				}
			}

			public void ReportAdView(AdViewParams adViewParams)
			{
				if (ServiceJavaObject != null)
				{
					var mediationType = (adViewParams.MediationType == TTPMediationType.admob) ? 0 : 1;
					ServiceJavaObject.Call("reportAdViewUnity", new object []{
						mediationType,
						adViewParams.Revenue,
						adViewParams.Currency,
						adViewParams.Type,
						adViewParams.Network,
						adViewParams.NetworkPlacement,
						adViewParams.Placement,
						adViewParams.CreativeIdentifier});
				}
			}

			public string GetAdjustId()
			{
				if (ServiceJavaObject != null)
				{
					return ServiceJavaObject.Call<string>("getAdjustId");
				}

				return "NA";
			}
		}

#endif


        // #if UNITY_EDITOR

        private class EditorImpl : IAdjust
		{
			public void LogEvent (string eventToken, IDictionary<string, object> eventParams, float revenue, string currency = "USD")
			{
				string paramsStr = "";
				if(eventParams != null && eventParams.Count > 0)
				{
					paramsStr = TTPJson.Serialize(eventParams);
				}
				Debug.Log("TTPAdjust::EditorImpl::LogEvent: eventToken: " + eventToken + "\neventParams: " + paramsStr + "\nrevenue: " + revenue + "\ncurrency: " + currency);
			}

			public void LogNonRevenueEvent(string eventToken, IDictionary<string, object> eventParams)
			{
				string paramsStr = "";
				if(eventParams != null && eventParams.Count > 0)
				{
					paramsStr = TTPJson.Serialize(eventParams);
				}
				Debug.Log("TTPAdjust::EditorImpl::LogNonRevenueEvent: eventToken: " + eventToken + "\neventParams: " + paramsStr);

			}

			public void ReportAdView(AdViewParams adViewParams)
			{
				Debug.Log("TTPAdjust::EditorImpl::ReportAdView: mediation: " + adViewParams.MediationType + 
				          "\nrevenue: " + adViewParams.Revenue + 
				          "\ncurrency: " + adViewParams.Currency + 
				          "\ntype: " + adViewParams.Type + 
				          "\nnetwork: " + adViewParams.Network + 
				          "\nnetworkPlacement: " + adViewParams.NetworkPlacement + 
				          "\nplacement: " + adViewParams.Placement + 
				          "\ncreativeIdentifier: " + adViewParams.CreativeIdentifier);
			}

			public string GetAdjustId()
			{
				return "EditorImplAdjustId";
			}
		}
        // #endif

        [Preserve]
        public class AdjustDelegate : MonoBehaviour
        {
	        public void OnAdjustIdReceived(string adjustId)
	        {
		        Debug.Log("AdjustDelegate::OnAdjustIdReceived:" + adjustId);
		        if (OnAdjustIdReceivedEvent != null)
		        {
			        OnAdjustIdReceivedEvent(adjustId);
		        }
		        else
		        {
			        Debug.Log("AdjustDelegate::OnAdjustIdReceived fired but no one is registered to it.");
		        }
	        }
	        
	        public void OnAdjustDeeplinkReceived(string url)
	        {
		        Debug.Log("AdjustDelegate::OnAdjustDeeplinkReceived:" + url);
		        if (OnAdjustDeeplinkReceivedEvent == null)
		        {
			        Debug.Log("AdjustDelegate::OnAdjustDeeplinkReceived fired but no one is registered to it.");
			        return;
		        }
						
			      OnAdjustDeeplinkReceivedEvent(url);
	        }
        }
	}
}
#endif