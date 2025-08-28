using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tabtale.TTPlugins
{
    public class TTPSafeArea : MonoBehaviour
    {
        RectTransform Panel;
        Rect LastSafeArea = new Rect (0, 0, 0, 0);

        public enum SimDevice { None, iPhoneX }
        public static SimDevice Sim = SimDevice.None;
        Rect[] NSA_iPhoneX = new Rect[]
        {
            new Rect (0f, 102f / 2436f, 1f, 2202f / 2436f),  // Portrait
            new Rect (132f / 2436f, 63f / 1125f, 2172f / 2436f, 1062f / 1125f)  // Landscape
        };
        
        void Awake ()
        {
            Panel = GetComponent<RectTransform> ();
            Refresh ();
        }

        void Update ()
        {
            Refresh ();
        }

        void Refresh ()
        {
            Rect safeArea = GetSafeArea ();

            if (safeArea != LastSafeArea)
                ApplySafeArea (safeArea);
        }

        Rect GetSafeArea ()
        {
            Rect safeArea = Screen.safeArea;

            if (Application.isEditor && Sim != SimDevice.None)
            {
                Rect nsa = new Rect (0, 0, Screen.width, Screen.height);

                switch (Sim)
                {
                    case SimDevice.iPhoneX:
                        if (Screen.height > Screen.width)  // Portrait
                            nsa = NSA_iPhoneX[0];
                        else  // Landscape
                            nsa = NSA_iPhoneX[1];
                        break;
                    default:
                        break;
                }

                safeArea = new Rect (Screen.width * nsa.x, Screen.height * nsa.y, Screen.width * nsa.width, Screen.height * nsa.height);
            }

            return safeArea;
        }

        void ApplySafeArea (Rect safeArea)
        {
            LastSafeArea = safeArea;
            Debug.Log ("TTPSafeArea:: ApplySafeArea -> Before: Panel.anchorMin = " + Panel.anchorMin + ", Panel.anchorMax = " + Panel.anchorMax);
            Debug.Log ("TTPSafeArea:: ApplySafeArea -> New safe area applied to: name = " + name 
                                                                                                 + ", safeArea.x " + safeArea.x 
                                                                                                 + ", safeArea.y " + safeArea.y 
                                                                                                 + ", safeArea.width " + safeArea.width 
                                                                                                 + ", safeArea.height " + safeArea.height 
                                                                                                 + ", on full extents: " 
                                                                                                 + ", Screen.width: " + Screen.width 
                                                                                                 + ", Screen.height: " + Screen.height);

            // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            Panel.anchorMin = anchorMin;
            Panel.anchorMax = anchorMax;
            Debug.Log ("TTPSafeArea:: ApplySafeArea -> After: Panel.anchorMin = " + Panel.anchorMin + ", Panel.anchorMax = " + Panel.anchorMax);

        }
    }

}