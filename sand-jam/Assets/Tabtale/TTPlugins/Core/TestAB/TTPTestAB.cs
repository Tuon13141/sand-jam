using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tabtale.TTPlugins
{
    public class TTPTestAB
    {
        private const string TAG = "TTPTestAB::";

        private static TTPTestAB _testAb;
        private static TTPTestABViewController _controller;
        private static GameObject _testAbConsole;
        private static string _loadedExperiment = "";
        public static string LoadedExperiment
        {
            set { _loadedExperiment = value; }
            get { return _loadedExperiment; }
        }
        private static string _loadedVariant = "";
        public static string LoadedVariant
        {
            set { _loadedVariant = value; }
            get { return _loadedVariant; }
        }

        private const string STORED_FILENAME = "testAb.json";
        
        private static string _storedFilePath;
        
        private static Dictionary<string, object> _config;
        public static Dictionary<string,object> Config
        {
            get { return _config; }
        }
        
        private static Dictionary<string, object> _configDictionary = new Dictionary<string, object>();

        public static Action OnHideConsole;
        
        public static void Create()
        {
            _testAb = new TTPTestAB();
        }

        private TTPTestAB()
        {
            _storedFilePath = Path.Combine(Application.persistentDataPath, STORED_FILENAME);
            SetupConsole();
            LoadConfigFromFileAndApply();
            
            TTPTestABConfigLoader.Load(config =>
            {
                Debug.Log(TAG + "config loading completed");
                _configDictionary = config;
                if (_controller)
                {
                    _controller.ReloadData();
                }
            });
        }

        public static void Show()
        {
            if (_controller != null)
            {
                _controller.Show();
            }
        }

        public static void ApplyExperimentWithVariant(string experimentName, string variantName)
        {
            var variantsDict = _configDictionary[experimentName] as Dictionary<string, object>;
            if (variantsDict != null)
            {
                var jsonData = TTPJson.Serialize(variantsDict[variantName]);
                SendOnRemoteConfigUpdate(jsonData);
                SaveConfigToFile(experimentName, variantName);
            }
        }

        public static string[] GetExperiments()
        {
            var result = new string[] {};
            if (_configDictionary != null)
            {
                result = _configDictionary.Keys.ToArray();
            }
            return result;
        }

        public static string[] GetVariantNamesForExperiment(String name)
        {
            var variants = _configDictionary[name] as Dictionary<String, object>;
            if (variants != null)
            {
                return variants.Keys.ToArray();
            }

            return new string[] {};
        }
        
        public static string GetVariantForExperiment(String experimentName, String variantName)
        {
            var variantsDict = _configDictionary[experimentName] as Dictionary<string, object>;
            if (variantsDict != null)
            {
                return TTPJson.Serialize(variantsDict[variantName]);
            }

            return "";
        }
        
        public static bool IsSavedConfigForABTestExist()
        {
            return File.Exists(_storedFilePath);
        }

        /// Private
        
        private static void LoadConfigFromFileAndApply()
        {
            if (File.Exists(_storedFilePath))
            {
                Debug.Log("TTPTestAB::LoadConfigFromFileAndApply: path - " + _storedFilePath);
                var sw = File.OpenText(_storedFilePath);
                var jsonData = sw.ReadToEnd();
                var jsonDic = TTPJson.Deserialize(jsonData) as Dictionary<string, object>;
                if (jsonDic != null)
                {
                    if (jsonDic.ContainsKey("config") && jsonDic["config"] is Dictionary<string, object> &&
                        jsonDic.ContainsKey("experimentName") && jsonDic["experimentName"] is string &&
                        jsonDic.ContainsKey("variantName") && jsonDic["variantName"] is string )
                    {
                        var configDic = jsonDic["config"] as Dictionary<string, object>;
                        _config = configDic;
                        var config = TTPJson.Serialize(configDic);
                        _loadedExperiment = jsonDic["experimentName"] as string;
                        _loadedVariant = jsonDic["variantName"] as string;
                        Debug.Log("TTPTestAB::LoadConfigFromFileAndApply:config=" + config +
                                  " experimentName=" + _loadedExperiment +
                                  " variantName=" + _loadedVariant);
                        SendOnRemoteConfigUpdate(config);
                    }
                    else
                    {
                        Debug.Log("TTPTestAB::LoadConfigFromFileAndApply: configuration is incorrect");
                    }
                }
                else
                {
                    Debug.Log("TTPTestAB::LoadConfigFromFileAndApply: configuration is empty");
                }
            }
            else
            {
                Debug.Log("TTPTestAB::LoadConfigFromFileAndApply: no stored configuration");
            }
        }
        
        private static void SetupConsole()
        {
            if (!_testAbConsole)
            {
                _testAbConsole = Resources.Load<GameObject>("TestABCanvas");
                _testAbConsole = Object.Instantiate(_testAbConsole);
                Object.DontDestroyOnLoad(_testAbConsole);
                
                var rectTransform = _testAbConsole.GetComponent<RectTransform>();
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                
                var loggerParent = _testAbConsole.transform.Find("TestABParent").gameObject;
                _controller = loggerParent.AddComponent<TTPTestABViewController>();
            }
        }

        private static void SaveConfigToFile(string experimentName, string variantName)
        {
            Debug.Log("TTPTestAB::SaveConfigToFile: experimentName - " + experimentName + ", variantName - " + variantName);

            var variantsDict = _configDictionary[experimentName] as Dictionary<string, object>;
            if (variantsDict != null)
            {
                var sw = File.CreateText(_storedFilePath);
                var jsonDic = new Dictionary<string, object>();
                jsonDic["config"] = variantsDict[variantName];
                _config = variantsDict[variantName] as Dictionary<string, object>;
                jsonDic["experimentName"] = experimentName;
                jsonDic["variantName"] = variantName;
                var jsonData = TTPJson.Serialize(jsonDic);
                Debug.Log("TTPTestAB::SaveConfigToFile: jsonData=" + jsonData);
                sw.Write(jsonData);
                sw.Close();
            }
        }

        private static void SendOnRemoteConfigUpdate(string jsonData)
        {
            Debug.Log("TTPTestAB::SendOnRemoteConfigUpdate: jsonData - " + jsonData);
            
            var bindingAttr = BindingFlags.NonPublic | BindingFlags.Static;
            var method = typeof(TTPCore).GetMethod("GetTTPGameObject", bindingAttr);
            if (method != null)
            {
                var gameObject = method.Invoke(null, null) as GameObject;
                if (gameObject != null)
                {
                    var coreDelegate = gameObject.GetComponent<TTPCore.CoreDelegate>(); 
                    coreDelegate.OnRemoteConfigUpdate(jsonData);
                }
            }
        }
    }
}
