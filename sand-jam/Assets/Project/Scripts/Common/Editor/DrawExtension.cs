using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class DrawExtension
{
    public static void DrawHorizontalLine()
    {
        GUILayout.Space(6);
        Rect rect = EditorGUILayout.GetControlRect(false, 10);
        rect.height = 2;
        EditorGUI.DrawRect(rect, new Color32(0, 135, 189, 255));
    }

    public static void ClearConsole()
    {
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor");
        var clearMethod = logEntries?.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod?.Invoke(null, null);
    }
}
