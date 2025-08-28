using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Tabtale.TTPlugins;
using UnityEngine;

public class TTPFacebookUtils
{
    public static XmlDocument TryOpenXml(string path)
    {
        Debug.Log("TTPFacebookUnity: TTPFacebookUtils:: TryOpenXml: path: " + path);
        if (File.Exists(path))
        {
            Debug.Log("TTPFacebookUnity: TTPFacebookUtils:: TryOpenXml: file exists");
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            Debug.Log("TTPFacebookUnity: TTPFacebookUtils:: TryOpenXml: opened file: " + xmlDoc);
            return xmlDoc;
        }
        Debug.Log("TTPFacebookUnity: TTPFacebookUtils:: TryOpenXml: file doesn't exist");
        return null;
    }

    public static Dictionary<string, object> TryOpenConfig(string path)
    {
        Debug.Log("TTPFacebookUnity: TTPFacebookUtils:: TryOpenConfig: path: " + path);
        if (File.Exists(path))
        {
            Debug.Log("TTPFacebookUnity: TTPFacebookUtils:: TryOpenConfig: config file exists");
            var jsonStr = File.ReadAllText(path);
            if (!string.IsNullOrEmpty(jsonStr) && jsonStr.Trim() != "{}")
            {
                Debug.Log("TTPFacebookUnity: TTPFacebookUtils:: TryOpenConfig: config not empty: " + jsonStr);
                var dict = TTPJson.Deserialize(jsonStr) as Dictionary<string, object>;
                if (dict != null)
                {
                    Debug.Log("TTPFacebookUnity: TTPFacebookUtils:: TryOpenConfig: deserialized config");
                    return dict;
                }
                Debug.Log("TTPFacebookUnity: TTPFacebookUtils:: TryOpenConfig: failed to deserializ config");
            }
            Debug.Log("TTPFacebookUnity: TTPFacebookUtils:: TryOpenConfig: config is empty");
        }
        Debug.Log("TTPFacebookUnity: TTPFacebookUtils:: TryOpenConfig: config file doesn't exist");
        return null;
    }

    public static string TryGetValue(Dictionary<string, object> config, string key)
    {
        Debug.Log("TTPFacebookUnity: TTPFacebookUtils:: TryGetValue: from: " + config + " -----for key: " + key);
        object val = null;
        if (config.ContainsKey(key) && config.TryGetValue(key, out val) &&
            val is string)
        {
            Debug.Log("TTPFacebookUnity: TTPFacebookUtils:: TryGetValue: got value: " + (string)val);
            return (string)val;
        }
        Debug.Log("TTPFacebookUnity: TTPFacebookUtils:: TryGetValue: failed to get value");
        return null;
    }
    
    //1 -> true; 0 -> false; -1 -> not found
    public static int TryGetBooleanValue(Dictionary<string, object> config, string key)
    {
        Debug.Log("TTPFacebookUnity: TTPFacebookUtils:: TryGetValue: from: " + config + " -----for key: " + key);
        object val = null;
        if (config.ContainsKey(key) && config.TryGetValue(key, out val) &&
            val is bool)
        {
            Debug.Log("TTPFacebookUnity: TTPFacebookUtils:: TryGetValue: got value: " + (bool)val);
            return (bool)val ? 1 : 0;
        }
        Debug.Log("TTPFacebookUnity: TTPFacebookUtils:: TryGetValue: failed to get value");
        return -1;
    }
}
