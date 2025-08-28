using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(ColorManager))]
public class ColorManagerDraw : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();

        Rect rect = EditorGUILayout.GetControlRect(false, 10);
        rect.height = 1;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

        string colorPresetPath = "Assets/Editor/ColorManager.colors";
        UnityEngine.Object presetObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(colorPresetPath);
        SerializedObject so = new SerializedObject(presetObject);
        SerializedProperty presets = so.FindProperty("m_Presets");
        int count = presets.arraySize;

        EditorGUILayout.LabelField("Color Presets", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal(); // Start horizontal layout
        for (int i = 0; i < count; i++)
        {
            Color color = presets.GetArrayElementAtIndex(i).FindPropertyRelative("m_Color").colorValue;

            // Display color button
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.background = MakeTex(20, 20, color);

            if (GUILayout.Button("", buttonStyle))
            {
                Debug.Log($"Selected Color: {color}");
            }
        }

        EditorGUILayout.EndHorizontal(); // End horizontal layout

    }

    // Helper function to create a solid color texture
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = col;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
