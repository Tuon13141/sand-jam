using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode.Extensions;

using Tabtale.TTPlugins;

namespace Tabtale.TTPlugins
{
#if UNITY_IOS
    public class TTPBuildIdPostProcess
    {
        [PostProcessBuild(44000)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            var plistPath = Path.Combine(path, "Info.plist");
            Debug.Log("TTPBuildIdPostProcess::OnPostProcessBuild: plistPath - " + plistPath);

            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            PlistElementDict rootDict = plist.root;

            AppendUnityVersion(rootDict);
            AppendBuildHashIndicator(rootDict);

            File.WriteAllText(plistPath, plist.WriteToString());
        }

        private static void AppendBuildHashIndicator(PlistElementDict rootDict)
        {
            string buildHash = System.Guid.NewGuid().ToString("N");
            Debug.Log("TTPBuildIdPostProcess::AppendBuildHashIndicator - " + buildHash);
            rootDict.SetString("TTPBuildId", buildHash);
        }

        public static void AppendUnityVersion(PlistElementDict rootDict)
        {
            Debug.Log("TTPBuildIdPostProcess::AppendUnityVersion - " + Application.unityVersion);
            rootDict.SetString("TTPUnityVersion", Application.unityVersion);
        }
    }
#else
    public class TTPBuildIdPostProcess : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder
        {
            get { return 0; }
        }

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string pathToModuleAndroidManifest = TTPUtils.CombinePaths(new List<string> {path, "src", "main", "AndroidManifest.xml"});
            if (File.Exists(pathToModuleAndroidManifest))
            {
                Debug.Log("TTPBuildIdPostProcess:: pathToAndroidManifest - " + pathToModuleAndroidManifest);

                var xmlDocument = new XmlDocument();
                xmlDocument.Load(pathToModuleAndroidManifest);
        
                RemoveOldValues(xmlDocument);
                
                AppendUnityVersion(xmlDocument);
                AppendBuildHashIndicator(xmlDocument);

                xmlDocument.Save(pathToModuleAndroidManifest);
            }
            else
            {
                Debug.Log("TTPBuildIdPostProcess:: there is no manifest in path - " + pathToModuleAndroidManifest);
            }
        }

        private static void RemoveOldValues(XmlDocument xmlDocument)
        {
            if (xmlDocument.DocumentElement != null) {
                XmlNode applicationNode = xmlDocument.DocumentElement.SelectSingleNode("application");
                List<XmlNode> nodesToRemove = new List<XmlNode>();

                if (applicationNode != null) {
                    foreach (XmlNode xmlNode in applicationNode.ChildNodes) {
                        Debug.Log("TTPBuildIdPostProcess::RemoveOldValues - " + xmlNode);
                        XmlAttribute androidNameAttribute = xmlNode.Attributes?["android:name"];
                        if (androidNameAttribute == null) continue;

                        if ((xmlNode.Name == "meta-data" && (androidNameAttribute.Value == "ttp.build-id" || androidNameAttribute.Value == "ttp.unity-version")) ||
                            (xmlNode.Name == "provider" && androidNameAttribute.Value == "com.tabtale.ttplugins.ttpcore.TTPFakeContentProvider")) {
                            nodesToRemove.Add(xmlNode);
                        }
                    }

                    foreach (XmlNode xmlNode in nodesToRemove) {
                        applicationNode.RemoveChild(xmlNode);
                    }
                }
            }
        }

        private static void AppendBuildHashIndicator(XmlDocument xmlDocument)
        {
            string buildHash = System.Guid.NewGuid().ToString("N");
            Debug.Log("TTPBuildIdPostProcess::AppendBuildHashIndicator - " + buildHash);

            XmlElement metaDataElement = CreateAndroidMetadata(xmlDocument, "ttp.build-id", buildHash);
            if (xmlDocument.DocumentElement != null) {
                XmlNode applicationNode = xmlDocument.DocumentElement.SelectSingleNode("application");
                if (applicationNode != null) {
                    applicationNode.AppendChild(metaDataElement);

                    // Append build-id to content provider attribute
                    XmlElement providerElement = xmlDocument.CreateElement("provider", xmlDocument.DocumentElement.NamespaceURI);
                    providerElement.Attributes.Append(CreateAndroidAttribute(xmlDocument, "authorities",
                        Application.identifier + ".unity.build-id=" + buildHash));
                    providerElement.Attributes.Append(CreateAndroidAttribute(xmlDocument, "name", "com.tabtale.ttplugins.ttpcore.TTPFakeContentProvider"));
                    providerElement.Attributes.Append(CreateAndroidAttribute(xmlDocument, "exported", "false"));
                    applicationNode.AppendChild(providerElement);
                }
            }
        }

        private static void AppendUnityVersion(XmlDocument xmlDocument)
        {
            Debug.Log("TTPBuildIdPostProcess::AppendUnityVersion - " + Application.unityVersion);

            XmlElement metaDataElement = CreateAndroidMetadata(xmlDocument, "ttp.unity-version", Application.unityVersion);
            if (xmlDocument.DocumentElement != null) {
                XmlNode applicationNode = xmlDocument.DocumentElement.SelectSingleNode("application");
                if (applicationNode != null) {
                    applicationNode.AppendChild(metaDataElement);

                    // Append unity-version to content provider attribute
                    XmlElement providerElement = xmlDocument.CreateElement("provider", xmlDocument.DocumentElement.NamespaceURI);
                    providerElement.Attributes.Append(CreateAndroidAttribute(xmlDocument, "authorities",
                        Application.identifier + ".ttp.unity-version=" + Application.unityVersion));
                    providerElement.Attributes.Append(CreateAndroidAttribute(xmlDocument, "name", "com.tabtale.ttplugins.ttpcore.TTPFakeContentProvider"));
                    providerElement.Attributes.Append(CreateAndroidAttribute(xmlDocument, "exported", "false"));
                    applicationNode.AppendChild(providerElement);
                }
            }
        }

        private static XmlElement CreateAndroidMetadata(XmlDocument xmlDocument, string name, string value)
        {
            XmlElement metaDataElement = xmlDocument.CreateElement("meta-data");
            metaDataElement.Attributes.Append(CreateAndroidAttribute(xmlDocument, "name", name));
            metaDataElement.Attributes.Append(CreateAndroidAttribute(xmlDocument, "value", value));
            return metaDataElement;
        }

        private static XmlAttribute CreateAndroidAttribute(XmlDocument xmlDocument, string key, string value) 
        {
            XmlAttribute attribute = xmlDocument.CreateAttribute("android", key, "http://schemas.android.com/apk/res/android");
            attribute.Value = value;   
            return attribute;
        }
    }   
#endif
}
