using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Networking;

using Tabtale.TTPlugins;
using Tabtale.TTPlugins.UnityIAPWrapper;

namespace Tabtale.TTPlugins
{
    public class TTPPurchaseValidation
    {
        private static float RETRY_INTERVAL = 5;
        private static int MAX_RETRY_ATTEMPTS = 5;
        private static string TAG = "TTPPurchaseValidation::";

        private static Dictionary<string, object> _configuration;
        private static event System.Action<OnValidateResponse> OnValidPurchaseResponseEvent;
        private static event Action<bool> _onValidationFinished;

        public static void SendPurchaseInformation(string purchaseToken,
            float price,
            string currency, 
            string productId,
            string iapType,
            Dictionary<string, object> additionalParams,
            string transactionId,
            string originalTransactionId,
            bool? isFreeTrial,
            System.Action<OnValidateResponse> onValidPurchaseResponseEvent,
            Action<bool> onValidationFinished)
        {
            Debug.Log(TAG + "SendPurchaseInformation:");
            OnValidPurchaseResponseEvent = onValidPurchaseResponseEvent;
            _onValidationFinished = onValidationFinished;

            if (!GetIsActive())
            {
                Debug.LogWarning(TAG + "SendPurchaseInformation: isActive is false. Will not send purchase information.");
                ValidateResponse(new OnValidateResponse
                {
                    price = price.ToString() ?? "",
                    currency = currency ?? "",
                    productId = productId ?? "",
                    valid = false,
                    failureReason = ValidationFailureReason.NOT_ACTIVE,
                    error = "not active"
                });
                return;
		    }

            if (string.IsNullOrEmpty(purchaseToken))
            {
                Debug.LogError(TAG + "SendPurchaseInformation: purchaseToken is null or empty.");
                ValidateResponse(new OnValidateResponse
                {
                    price = null,
                    currency = null,
                    productId = null,
                    valid = false,
                    failureReason = ValidationFailureReason.PURCHASE_TOKEN_MISSING,
                    error = "purchase token missing"
                });
                return;
            }

            if (string.IsNullOrEmpty(productId))
            {
                Debug.LogError(TAG + "SendPurchaseInformation: productId is null or empty.");
                ValidateResponse(new OnValidateResponse
                {
                    price = null,
                    currency = null,
                    productId = null,
                    valid = false,
                    failureReason = ValidationFailureReason.PRODUCT_ID_MISSING,
                    error = "product id missing"
                });
                return;
            }

            var purchaseInfo = new Dictionary<string, object>();
            
            var receipt = GetReceipt(productId, purchaseToken, additionalParams);
            if (!string.IsNullOrEmpty(receipt))
            {
                Debug.Log(TAG + "SendPurchaseInformation: receipt =  " + receipt);
                purchaseInfo["receipt"] = receipt;
            }
            else
            {
                Debug.LogError(TAG + "SendPurchaseInformation: receipt is null or empty.");
                ValidateResponse(new OnValidateResponse
                {
                    price = null,
                    currency = null,
                    productId = null,
                    valid = false,
                    failureReason = ValidationFailureReason.INTERNAL_ERROR,
                    error = "could not get receipt"
                });
                return;
            }

            var account = GetAccount();
            if (string.IsNullOrEmpty(account))
            {
                Debug.LogError(TAG + "SendPurchaseInformation: account key is not found in the billing json");
            }
            
            purchaseInfo["account"] = account ?? "Other";
            purchaseInfo["price"] = price;
            purchaseInfo["iapType"] = iapType ?? "Unknown";
            purchaseInfo["currency"] = currency;
            purchaseInfo["iapBundleId"] = productId;
            purchaseInfo["firebaseInstanceId"] = GetFirebaseInstanceId() ?? "Invalid Firebase Instance Id";
        
            purchaseInfo["appVersion"] = Application.version;
            purchaseInfo["appName"] = Application.productName;
            purchaseInfo["appBundleId"] = Application.identifier;
            purchaseInfo["osVersion"] = SystemInfo.operatingSystem;

            if (Application.platform == RuntimePlatform.Android)
                purchaseInfo["platform"] = "Android";
            else if (Application.platform == RuntimePlatform.IPhonePlayer)                
                purchaseInfo["platform"] = "iOS";

            // MMP:
            if (additionalParams.ContainsKey("mmp"))
                purchaseInfo["mmp"] = additionalParams["mmp"];
            if (additionalParams.ContainsKey("adjustUserID"))
                purchaseInfo["adjustUserID"] = additionalParams["adjustUserID"];
            if (additionalParams.ContainsKey("adjustAppToken"))
                purchaseInfo["adjustAppToken"] = additionalParams["adjustAppToken"];
            if (additionalParams.ContainsKey("adjustEventToken"))
                purchaseInfo["adjustEventToken"] = additionalParams["adjustEventToken"];
            if (additionalParams.ContainsKey("appsflyerUserId"))
                purchaseInfo["appsflyerUserId"] = additionalParams["appsflyerUserId"];
            if (additionalParams.ContainsKey("customerUserId"))
                purchaseInfo["customerUserId"] = additionalParams["customerUserId"];
            
            // Advertyising:
            if (additionalParams.ContainsKey("idfa"))
                purchaseInfo["idfa"] = additionalParams["idfa"];
            if (additionalParams.ContainsKey("idfv"))
                purchaseInfo["idfv"] = additionalParams["idfv"];
            if (additionalParams.ContainsKey("googleAdvertisingId"))
                purchaseInfo["googleAdvertisingId"] = additionalParams["googleAdvertisingId"];
            
            if (additionalParams.ContainsKey("ttid"))
                purchaseInfo["ttId"] = additionalParams["ttid"];

            purchaseInfo["nonce"] = Guid.NewGuid().ToString();
            purchaseInfo["clientEventTime"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            if (!string.IsNullOrEmpty(transactionId))
                purchaseInfo["transactionId"] = transactionId;
            if (!string.IsNullOrEmpty(originalTransactionId))
                purchaseInfo["originalTransactionId"] = originalTransactionId;
            if (isFreeTrial != null)
                purchaseInfo["isFreeTrial"] = isFreeTrial;

#if UNITY_IOS
            var appstoreId = GetAppstoreId();
            if (string.IsNullOrEmpty(appstoreId))
                Debug.LogError(TAG + "SendPurchaseInformation: appstoreId is null or empty");
            else
                purchaseInfo["appstoreId"] = appstoreId;
#endif

            StartSendPurchaseInformationCoro(purchaseInfo);
        }

        private static string GetReceipt(string productId, string purchaseToken, Dictionary<string, object> additionalParams)
        {
#if UNITY_ANDROID
            var jsonFormat = "{\"json\":\"{\\\"orderId\\\": \\\"\\\",\\\"packageName\\\":\\\"" + Application.identifier + "\\\",\\\"productId\\\":\\\"" + productId + "\\\",\\\"purchaseTime\\\":0,\\\"purchaseState\\\":0,\\\"purchaseToken\\\":\\\"" + purchaseToken + "\\\"}\",\"signature\":\"\",\"developerPayload\":\"\"}";
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(jsonFormat);
            return System.Convert.ToBase64String(plainTextBytes);
#elif UNITY_IOS
            if (additionalParams.ContainsKey("receipt") && additionalParams["receipt"] is string)
                return additionalParams["receipt"] as string;
#endif
            return null;
        }

        private static void StartRetryCoro(
            Dictionary<string, object> purchaseInfo, 
            int attempt, 
            long errorCode, 
            string errorMessage)
        {
            if (++attempt >= MAX_RETRY_ATTEMPTS)
            {
                Debug.LogWarning(TAG + "StartRetryCoro: exceeded max attempts");
                LogErrorValidationEvent(errorCode, errorMessage);
                return;
            }

            const BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Static;
            var method = typeof(TTPCore).GetMethod("GetTTPGameObject", bindingAttr);
            if (method != null)
            {
                var gameObject = method.Invoke(null, null) as GameObject;
                if (gameObject != null)
                {
                    var mono = gameObject.GetComponent<MonoBehaviour>();
                    mono.StartCoroutine(RetryCoro(new Dictionary<string, object>(purchaseInfo), attempt));
                }
            }
        }

        private static IEnumerator RetryCoro(Dictionary<string, object> purchaseInfo, int attempt)
        {
            // Copy the purchaseInfo dictionary to avoid destroying by GC the original one
            var purchaseInfoCopy = new Dictionary<string, object>(purchaseInfo);

            yield return new WaitForSeconds(RETRY_INTERVAL);
            StartSendPurchaseInformationCoro(purchaseInfoCopy, attempt);
        }

        private static void StartSendPurchaseInformationCoro(Dictionary<string, object> purchaseInfo, int attempt = 0)
        {
            const BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Static;
            var method = typeof(TTPCore).GetMethod("GetTTPGameObject", bindingAttr);
            if (method != null)
            {
                var gameObject = method.Invoke(null, null) as GameObject;
                if (gameObject != null)
                {
                    var mono = gameObject.GetComponent<MonoBehaviour>();
                    mono.StartCoroutine(SendPurchaseInformationCoro(new Dictionary<string, object>(purchaseInfo), attempt));
                }
            }
        }

        private static IEnumerator SendPurchaseInformationCoro(Dictionary<string, object> purchaseInfo, int attempt)
        {
            string url = GetSendPurchaseInformationUrl();
            string paramsStr = TTPJson.Serialize(purchaseInfo);

            Debug.Log(TAG + "SendPurchaseInformationCoro: server URL: " + url + " attempt: " + attempt + " params: "); 
            foreach (KeyValuePair<string, object> kvp in purchaseInfo)
                Debug.Log(" - " + kvp.Key + ": " + kvp.Value);
                
            var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(paramsStr);
            request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 30;

            yield return request.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError(TAG + "SendPurchaseInformation: request error: " + request.error
                    + ":\n" + request.downloadHandler.text);
                if (attempt == 0)
                {
                    ValidateResponse(new OnValidateResponse
                    {
                        price = purchaseInfo["price"] as string,
                        currency = purchaseInfo["currency"] as string,
                        productId = purchaseInfo["iapBundleId"] as string,
                        valid = false,
                        failureReason = ValidationFailureReason.SERVER_ERROR,
                        error = request.error
                    });
                }

                if (ShouldRetryForResponseCode(request.responseCode))
                {
                    Debug.LogWarning(TAG + "SendPurchaseInformation: Will retry in " + RETRY_INTERVAL + " seconds.");
                    StartRetryCoro(purchaseInfo, attempt, request.responseCode, 
                        request.error + "\n" + request.downloadHandler.text);
                }
            }
            else
            {
                Debug.Log(TAG + "SendPurchaseInformation: response: " + request.downloadHandler.text);
                if (attempt == 0)
                {
                    ValidateResponse(new OnValidateResponse
                    {
                        price = purchaseInfo["price"] as string,
                        currency = purchaseInfo["currency"] as string,
                        productId = purchaseInfo["iapBundleId"] as string,
                        valid = true,
                        failureReason = ValidationFailureReason.NONE,
                        error = ""
                    });
                }
            }
        }

        private static bool ShouldRetryForResponseCode(long responseCode)
        {
            // Retry mechanism in case we get any response code different from any 200s or 400s
            if ((int)(responseCode / 100) == 2 || (int)(responseCode / 100) == 4)
                return false;
            return true;
        }

#if UNITY_IOS
        private static string GetAppstoreId()
        {
            string configurationJson = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetConfigurationJson("global");
            if (!string.IsNullOrEmpty(configurationJson))
            {
                var configuration = TTPJson.Deserialize(configurationJson) as Dictionary<string, object>;
                if (configuration != null)
                {
                    if (configuration.ContainsKey("appId") && configuration["appId"] is string)
                    {
                        return configuration["appId"] as string;
                    }
                }
            }
            return null;
        }
#endif

        private static bool GetIsActive()
        {
            string configurationJson = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetConfigurationJson("global");
            if (!string.IsNullOrEmpty(configurationJson))
            {
                var configuration = TTPJson.Deserialize(configurationJson) as Dictionary<string, object>;
                if (configuration != null)
                {
                    if (configuration.ContainsKey("active") && configuration["active"] is bool)
                    {
                        return (bool)configuration["active"];
                    }
                }
            }
            return true;
        }

        private static string GetSendPurchaseInformationUrl()
        {
            var result = "https://crazy-rvs.ttpsdk.info/check";
            if (Configuration != null)
            {
                if (Configuration.ContainsKey("purchaseValidation") &&
                    Configuration["purchaseValidation"] is Dictionary<string, object>)
                {
                    var purchaseValidation = Configuration["purchaseValidation"] as Dictionary<string, object>;
                    if (purchaseValidation.ContainsKey("serverDomain") && purchaseValidation["serverDomain"] is string)
                    {
                        var serverDomain = purchaseValidation["serverDomain"] as string;
                        if (!string.IsNullOrEmpty(serverDomain))
                        {
                            Debug.Log(TAG + "GetSendPurchaseInformationUrl: serverDomain: " + serverDomain);
                            result = "https://" + serverDomain + "/check";
                        }
                        else
                        {
                            Debug.LogError(TAG + "GetSendPurchaseInformationUrl: serverDomain is empty");
                        }
                    }
                    else
                    {
                        Debug.LogError(TAG + "GetSendPurchaseInformationUrl: serverDomain is null");
                    }
                }
                else
                {
                    Debug.LogError(TAG + "GetSendPurchaseInformationUrl: purchaseValidation object does not exist...");
                }
            }
            else
            {
                Debug.LogError(TAG + "GetSendPurchaseInformationUrl: billing configration is faulty or does not exist...");
            }

            return result;
        }

        private static string GetAccount()
        {
            if (Configuration != null)
            {
                if (Configuration.ContainsKey("purchaseValidation") && Configuration["purchaseValidation"] is Dictionary<string, object>)
                {
                    var purchaseValidation = Configuration["purchaseValidation"] as Dictionary<string, object>;
                    if (purchaseValidation.ContainsKey("account") && purchaseValidation["account"] is string)
                    {
                        return purchaseValidation["account"] as string;
                    }
                }
            }
            return null;
        }

        private static Dictionary<string, object> Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    string configurationJson = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetConfigurationJson("billing");
                    if (!string.IsNullOrEmpty(configurationJson))
                    {
                        _configuration = TTPJson.Deserialize(configurationJson) as Dictionary<string,object>;
                    }
                }
                return _configuration;
            }
        }

        private static string GetFirebaseInstanceId() 
        {
            var analyticsClsType = System.Type.GetType("Tabtale.TTPlugins.TTPAnalytics");
            if (analyticsClsType != null)
            {
                var method = analyticsClsType.GetMethod("GetInstanceIdFirebase", BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null)
                {
                    var id = (string)method.Invoke(null, null);
                    if (!string.IsNullOrEmpty(id))
                    {
                        return id;
                    }
                }
            }
            return null;
        }

        public static void ValidateResponse(OnValidateResponse response)
        {
            if (response.valid)
            {
                Dictionary<string, object> paramsDict = new Dictionary<string, object>();
                paramsDict["price"] = response.price ?? "0";
                paramsDict["currency"] = response.currency ?? "USD";
                paramsDict["productId"] = response.productId ?? "";
                paramsDict["valid"] = true;

                System.Type analyticsHelperClsType = System.Type.GetType("Tabtale.TTPlugins.TTPAnalyticsHelper");
                System.Type analyticsClsType = System.Type.GetType("Tabtale.TTPlugins.TTPAnalytics");
                if (analyticsHelperClsType != null && analyticsClsType != null) 
                {
                    MethodInfo logTransactionMethod = analyticsClsType.GetMethod("LogTransaction", BindingFlags.Public | BindingFlags.Static);
                    MethodInfo generateItemMethod = analyticsHelperClsType.GetMethod("GenerateItem", BindingFlags.Public | BindingFlags.Static);
                    MethodInfo generateRealCurrencyMethod = analyticsHelperClsType.GetMethod("GenerateRealCurrency", BindingFlags.Public | BindingFlags.Static);
                    MethodInfo generateProductsSpentMethod = analyticsHelperClsType.GetMethod("GenerateProductsSpent", BindingFlags.Public | BindingFlags.Static);
                    if (logTransactionMethod != null && generateItemMethod != null && generateProductsSpentMethod != null && generateRealCurrencyMethod != null)
                    {
                        IDictionary<string, object> item = generateItemMethod.Invoke(null, new object[] { 1, response.productId, response.productId }) as IDictionary<string, object>;
                        IDictionary<string, object>[] items = new IDictionary<string, object>[] { item };
                        IDictionary<string, object> rCurrency = generateRealCurrencyMethod.Invoke(null, new object[] { response.price, response.currency }) as IDictionary<string, object>;
                        IDictionary<string, object> pSpent = generateProductsSpentMethod.Invoke(null, new object[] { items, rCurrency, null }) as IDictionary<string, object>;
                        logTransactionMethod.Invoke(null, new object[] { "Store Purchase", null, pSpent, null });
                    }
                    else
                    {
                        Debug.LogWarning(TAG + "ValidateResponse: Could not find Anlytics or AnalyticsHelper methods");
                    }
                }
                else
                {
                    Debug.LogWarning(TAG + "ValidateResponse: Could not find Anlytics or AnalyticsHelper class");
                }
            }
            else
            {
                Debug.LogWarning(TAG + "ValidateResponse: response.valid is false. will not send Store Purchase event.");
            }
            
            if (OnValidPurchaseResponseEvent != null)
            {
                OnValidPurchaseResponseEvent(response);
            }
            Debug.Log(TAG + "onValidationFinished");
            _onValidationFinished.Invoke(response.failureReason == ValidationFailureReason.NONE);
        }

        private static void LogErrorValidationEvent(long code, string message)
        {
            Debug.Log(TAG + "LogErrorValidationEvent, code: " + code + ", message: " + message);
            IDictionary<string, object> logEventParams = new Dictionary<string, object>()
            {
                { "code", code },
                { "message", message }
            };

            System.Type analyticsClsType = System.Type.GetType("Tabtale.TTPlugins.TTPAnalytics");
            if (analyticsClsType != null)
            {
                System.Reflection.MethodInfo method = analyticsClsType.GetMethod("LogEvent", new Type[] {    
                    typeof(long), typeof(string), typeof(IDictionary<string, object>), typeof(bool), typeof(bool) });
                if (method != null)
                {
                    long firebaseAnalyticsConst = 1 << 2;
                    method.Invoke(null, new object[] { 
                        firebaseAnalyticsConst, "ErrorValidationEvent", logEventParams, false, true });
                }
                else
                {
                    Debug.LogError(TAG + "LogErrorValidationEvent: could not find method - LogEvent");
                }
            }
            else
            {
                Debug.LogError(TAG + "LogErrorValidationEvent: could not find TTPAnalytics class");
            }
        }
    }
}