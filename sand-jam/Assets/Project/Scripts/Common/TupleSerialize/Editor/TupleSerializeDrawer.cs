using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TupleSerialize<,>))]
public class Tuple2SerializeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Start the property
        EditorGUI.BeginProperty(position, label, property);

        // Get the properties
        SerializedProperty valueProperty1 = property.FindPropertyRelative("value1");
        SerializedProperty valueProperty2 = property.FindPropertyRelative("value2");

        // Calculate rects
        Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
        Rect minMaxRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);

        // Draw the main label
        EditorGUI.LabelField(labelRect, label);

        // Set up the indent level
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects for min and max labels and fields
        float offsetWidth = 5f;
        float fieldWidth = minMaxRect.width / 2f - offsetWidth;

        Rect fieldRect1 = new Rect(minMaxRect.x, minMaxRect.y, fieldWidth, minMaxRect.height);
        Rect fieldRect2 = new Rect(minMaxRect.x + fieldWidth + offsetWidth, minMaxRect.y, fieldWidth, minMaxRect.height);


        // Draw labels and fields for min and max values
        EditorGUI.PropertyField(fieldRect1, valueProperty1, GUIContent.none);
        EditorGUI.PropertyField(fieldRect2, valueProperty2, GUIContent.none);

        // Restore the indent level
        EditorGUI.indentLevel = indent;

        // End the property
        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(TupleSerialize<,,>))]
public class Tuple3SerializeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Start the property
        EditorGUI.BeginProperty(position, label, property);

        // Get the properties
        SerializedProperty valueProperty1 = property.FindPropertyRelative("value1");
        SerializedProperty valueProperty2 = property.FindPropertyRelative("value2");
        SerializedProperty valueProperty3 = property.FindPropertyRelative("value3");

        // Calculate rects
        Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
        Rect minMaxRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);

        // Draw the main label
        EditorGUI.LabelField(labelRect, label);

        // Set up the indent level
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects for min and max labels and fields
        float offsetWidth = 5f;
        float fieldWidth = (minMaxRect.width - 2f * offsetWidth) / 3f;

        Rect fieldRect1 = new Rect(minMaxRect.x, minMaxRect.y, fieldWidth, minMaxRect.height);
        Rect fieldRect2 = new Rect(minMaxRect.x + fieldWidth + offsetWidth, minMaxRect.y, fieldWidth, minMaxRect.height);
        Rect fieldRect3 = new Rect(minMaxRect.x + (fieldWidth + offsetWidth) * 2, minMaxRect.y, fieldWidth, minMaxRect.height);


        // Draw labels and fields for min and max values
        EditorGUI.PropertyField(fieldRect1, valueProperty1, GUIContent.none);
        EditorGUI.PropertyField(fieldRect2, valueProperty2, GUIContent.none);
        EditorGUI.PropertyField(fieldRect3, valueProperty3, GUIContent.none);

        // Restore the indent level
        EditorGUI.indentLevel = indent;

        // End the property
        EditorGUI.EndProperty();
    }
}
