using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace Tabtale.TTPlugins
{
    public class TTPOpenURL : MonoBehaviour
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void ttpOpenUrl(string url);
#endif
    
        public static void Open(string url)
        {
#if UNITY_IOS && !UNITY_EDITOR
        ttpOpenUrl(url);
#else
            Application.OpenURL(url);
#endif
        }
    }
}

