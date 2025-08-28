using UnityEditor;
using UnityEngine;

public class MissingScriptsTool
{
    const string k_missingScriptsMenuFolder = "Tools/Missing Scripts/Find&Romove";

    [MenuItem(k_missingScriptsMenuFolder)]
    static void FindMissingScriptsMenuItem()
    {
        foreach (GameObject gameObject in GameObject.FindObjectsOfType<GameObject>(true))
        {
            foreach (Component component in gameObject.GetComponentsInChildren<Component>())
            {
                if (component == null)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                    break;
                }
            }
        }
    }
}