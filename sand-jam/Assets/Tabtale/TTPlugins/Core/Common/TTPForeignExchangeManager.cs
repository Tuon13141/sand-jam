using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

namespace Tabtale.TTPlugins
{
    public class TTPForeignExchangeManager
    {
        public const float FAILED = float.MinValue;

        private static TTPForeignExchangeManager _instance;
        private const string TAG = "TTPForeignExchangeManager::";
        private Dictionary<string, float> _exchangeRates = new Dictionary<string, float>();

        public static TTPForeignExchangeManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new TTPForeignExchangeManager();
            }
            return _instance;
        }

        public void Exchange(string fromCurrency, float amount, Action<float, string> completionHandler)
        {
            if (amount < 0)
            {
                Debug.LogError(TAG + "Exchange: amount must be positive");
                completionHandler(FAILED, "Error: amount must be positive");
                return;
            }

            Debug.Log(TAG + "Exchange: amount = " + amount + ", fromCurrency = " + fromCurrency);

            string fromCurrencyNormalized = fromCurrency.ToUpper().Trim();
            if (fromCurrencyNormalized == "USD")
            {
                completionHandler(amount, null);
                return;
            };

            LoadExchangedAmount(fromCurrencyNormalized, amount, (result) => {
                if (result != FAILED)
                {
                    completionHandler(result, null);
                }
                else
                {
                    result = ConvertLocally(fromCurrencyNormalized, amount);
                    if (result != FAILED)
                    {
                        completionHandler(result, null);
                    }
                    else
                    {
                        completionHandler(FAILED, "Error: failed to convert amount with currency - " + fromCurrencyNormalized);
                    }
                }
            });
        }

        private TTPForeignExchangeManager() 
        { 
            ReadDefaultConfiguration();
        }

        private void ReadDefaultConfiguration()
        {
            var json = "";
            if (Application.platform == RuntimePlatform.Android)
            {
                var path = TTPUtils.CombinePaths(new List<string>(){ "exchange", "exchangeRates.json" });
                json = TTPUtils.ReadStreamingAssetsFile(path);
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                var path = Application.streamingAssetsPath + "/exchange/exchangeRates.json";
                json = File.ReadAllText(path);
            }

            if (!string.IsNullOrEmpty(json))
            {
                var dict = TTPJson.Deserialize(json) as Dictionary<string, object>;
                if (dict != null)
                {
                    foreach (var entry in dict)
                    {
                        string currency = entry.Key;
                        float rate = float.Parse(entry.Value.ToString());
                        _exchangeRates.Add(currency, rate);
                    }
                }
                else
                {
                    Debug.LogError(TAG + "ReadDefaultConfiguration: failed to deserialize - " + json);
                }
            }
            else
            {
                Debug.LogError(TAG + "ReadDefaultConfiguration: failed to read file");
            }
        }

        private void LoadExchangedAmount(string fromCurrency, float amount, Action<float> completionHandler)
        {
            var urlStr = CreateConversionUrl(fromCurrency, amount);
            Debug.Log(TAG + "LoadExchangedAmountCoro: url = " + urlStr);

            DownloadString(urlStr, (result) => {
                if (result != null) 
                {
                    var dict = TTPJson.Deserialize(result) as Dictionary<string, object>;
                    if (dict != null)
                    {
                        if (dict.ContainsKey("result"))
                        {
                            float converted = float.Parse(dict["result"].ToString());
                            Debug.Log(TAG + "LoadExchangedAmountCoro: exchangeRate = " + converted);
                            completionHandler(converted);
                        }
                        else
                        {
                            Debug.LogError(TAG + "LoadExchangedAmountCoro: failed to get result from json - " + result);
                            completionHandler(FAILED);
                        }
                    }
                    else
                    {
                        Debug.LogError(TAG + "LoadExchangedAmountCoro: failed to deserialize json - " + result);
                        completionHandler(FAILED);
                    }
                }
                else
                {
                    Debug.LogError(TAG + "LoadExchangedAmountCoro: failed to get responce");
                    completionHandler(FAILED);
                }
            });
        }

        private string CreateConversionUrl(string fromCurrency, float amount)
        {
            const string TO_CURRENCY = "USD";
            const string API_KEY = "33a02bb3339e639eaf9979101a68f2d3";

            var amountStr = amount.ToString(CultureInfo.InvariantCulture);
            return "https://api.exchangeratesapi.io/v1/convert?access_key=" 
                + API_KEY + "&from=" + fromCurrency + "&to=" + TO_CURRENCY + "&amount=" + amountStr;
        }

        private float ConvertLocally(string fromCurrency, float amount)
        {
            if (_exchangeRates.ContainsKey(fromCurrency))
            {
                float rate = _exchangeRates[fromCurrency];
                return amount * rate;
            }
            return FAILED;
        }

        private void DownloadString(string uri, Action<string> completionHandler)
        {
            const BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Static;
            var method = typeof(TTPCore).GetMethod("GetTTPGameObject", bindingAttr);
            if (method != null)
            {
                var gameObject = method.Invoke(null, null) as GameObject;
                if (gameObject != null)
                {
                    var mono = gameObject.GetComponent<MonoBehaviour>();
                    mono.StartCoroutine(DownloadStringCoro(uri, completionHandler));
                }
            }
        }

        private IEnumerator DownloadStringCoro(string uri, Action<string> completionHandler)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();
                if (webRequest.isNetworkError)
                {
                    Debug.LogError(TAG + "DownloadString: Error: " + webRequest.error);
                    completionHandler(null);
                }
                else
                {
                    var result = webRequest.downloadHandler.text;
                    Debug.Log(TAG + "DownloadString: Received: " + result);
                    completionHandler(result);
                }   
            }
        }
    }
}