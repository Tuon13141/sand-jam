using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class CLIKUpdateWarning : MonoBehaviour
{
    private const string ClikDualConfigurationWarning = "CLIK_dual_config_warning";
    
    static CLIKUpdateWarning()
    {
        if(PlayerPrefs.GetInt(ClikDualConfigurationWarning, 0) != 0) return;
        PlayerPrefs.SetInt(ClikDualConfigurationWarning,1);
        EditorUtility.DisplayDialog("CLIK 4.4+ Configuration", "You have updated CLIK to a version of 4.4 or above.\n" +
                                                               "CLIK 4.4+ requires an updated ZIP that support dual configuration.\n" +
                                                               "(This is a one time warning, if you already have this zip you can ignore this)","Got it");
    }
}
