using UnityEngine;

namespace Tabtale.TTPlugins
{
    public class TTPLocalConfigurationScriptableObject : ScriptableObject
    {
        public TTPLocalConfigData[] configData;
    }

    [System.Serializable]
    public class TTPLocalConfigData
    {
        public string name;
        public string value;
    }
}