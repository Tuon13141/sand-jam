using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Matrix<>), true)]
public class MatrixDraw : PropertyDrawer
{
    private static float LineHeight => EditorGUIUtility.singleLineHeight;
    private static readonly Vector2 cellSpacing = new Vector2(5f, 5f);

    private static bool showGridSize = false;
    private const float firstLineMargin = 5f;
    private const float lastLineMargin = 2f;

    private SerializedProperty thisProperty;
    private SerializedProperty cellSizeProperty;
    private SerializedProperty cellsProperty;

    private Vector2Int gridSizeProperty => new Vector2Int(cellsProperty.arraySize, GetRowAt(0).arraySize);

    static class Texts
    {
        public static readonly GUIContent reset = new GUIContent("Reset Value");
        public static readonly GUIContent changeGridSize = new GUIContent("Change Grid Size");
        public static readonly GUIContent changeCellSize = new GUIContent("Change Cell Size");

        public const string gridSizeLabel = "Grid Size";
        public const string cellSizeLabel = "Cell Size";
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        thisProperty = property;
        TryFindPropertyRelative(property, "cellSize", out cellSizeProperty);
        TryFindPropertyRelative(property, "cells", out cellsProperty);

        position = EditorGUI.IndentedRect(position);

        // Begin property drawing
        EditorGUI.BeginProperty(position, label, property);

        // Display foldout
        var foldoutRect = new Rect(position)
        {
            height = LineHeight
        };

        // We're using EditorGUI.IndentedRect to draw our Rects, and it already takes the indentLevel into account, so we must set it to 0.
        // This allows the PropertyDrawer to handle nested variables correctly.
        // More info: https://answers.unity.com/questions/1268850/how-to-properly-deal-with-editorguiindentlevel-in.html
        EditorGUI.indentLevel = 0;

        property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(foldoutRect, property.isExpanded, label,
            menuAction: ShowHeaderContextMenu);
        EditorGUI.EndFoldoutHeaderGroup();

        // Go to next line
        position.y += LineHeight;

        if (showGridSize)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.Vector2IntField(position, "Size", gridSizeProperty);
            EditorGUI.EndDisabledGroup();
            position.y += LineHeight;
        }

        if (property.isExpanded)
        {
            position.y += firstLineMargin;


            var cellRect = new Rect(position.x, position.y, cellSizeProperty.vector2IntValue.x, cellSizeProperty.vector2IntValue.y);

            for (var y = 0; y < cellsProperty.arraySize; y++)
            {
                var rowProperty = GetRowAt(y);

                for (var x = 0; x < rowProperty.arraySize; x++)
                {
                    var pos = new Rect(cellRect)
                    {
                        x = cellRect.x + (cellRect.width + cellSpacing.x) * y,
                        y = cellRect.y + (cellRect.height + cellSpacing.y) * (rowProperty.arraySize - x - 1)
                    };

                    var propertyElement = rowProperty.GetArrayElementAtIndex(x);

                    if (propertyElement.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        var match = Regex.Match(propertyElement.type, @"PPtr<\$(.+)>");
                        if (match.Success)
                        {
                            var objectType = match.Groups[1].ToString();
                            var assemblyName = "UnityEngine";
                            EditorGUI.ObjectField(pos, propertyElement, System.Type.GetType($"{assemblyName}.{objectType}, {assemblyName}"), GUIContent.none);
                        }
                    }
                    else
                    {
                        EditorGUI.PropertyField(pos, propertyElement, GUIContent.none);
                    }
                }
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var height = base.GetPropertyHeight(property, label);
        if (showGridSize) height += LineHeight;

        TryFindPropertyRelative(property, "cellSize", out cellSizeProperty);
        TryFindPropertyRelative(property, "cells", out cellsProperty);
        var rowSize = 0;

        if (cellsProperty.arraySize > 0)
        {
            var rowProperty = GetRowAt(0);
            rowSize = rowProperty.arraySize;
        }

        if (property.isExpanded)
        {
            height += firstLineMargin;

            height += rowSize * (cellSizeProperty.vector2IntValue.y + cellSpacing.y) - cellSpacing.y; // Cells lines

            height += lastLineMargin;
        }

        return height;
    }
    private SerializedProperty GetRowAt(int idx)
    {
        if (cellsProperty.arraySize == 0)
        {
            InitNewGrid(MatrixConst.DEFAULT_GRID_SIZE);
        }

        return cellsProperty.GetArrayElementAtIndex(idx).FindPropertyRelative("row");
    }

    private void TryFindPropertyRelative(SerializedProperty parent, string relativePropertyPath, out SerializedProperty prop)
    {
        prop = parent.FindPropertyRelative(relativePropertyPath);

        if (prop == null)
        {
            Debug.LogError($"Couldn't find variable \"{relativePropertyPath}\" in {parent.name}");
        }
    }

    private void ShowHeaderContextMenu(Rect position)
    {
        var menu = new GenericMenu();
        menu.AddItem(Texts.reset, false, () => InitNewGrid(gridSizeProperty));
        menu.AddSeparator(""); // An empty string will create a separator at the top level
        menu.AddItem(Texts.changeGridSize, false, () => EditorWindowVector2IntField.ShowWindow("Change Grid Size", gridSizeProperty, InitNewGrid, Texts.gridSizeLabel));
        menu.AddItem(Texts.changeCellSize, false, () => EditorWindowVector2IntField.ShowWindow("Change Cell Size", cellSizeProperty.vector2IntValue, SetNewCellSize, Texts.cellSizeLabel));

        menu.DropDown(position);
    }

    private void InitNewGrid(Vector2Int newSize)
    {
        cellsProperty.ClearArray();
        for (var x = 0; x < newSize.x; x++)
        {
            cellsProperty.InsertArrayElementAtIndex(x); // Insert a new row
            var row = GetRowAt(x); // Get the new row
            row.ClearArray(); // Clear it
            for (var y = 0; y < newSize.y; y++)
            {
                row.InsertArrayElementAtIndex(y);

                var cell = row.GetArrayElementAtIndex(y);

                cell.Reset();
            }
        }

        thisProperty.serializedObject.ApplyModifiedProperties();
    }

    private void SetNewCellSize(Vector2Int newSize)
    {
        cellSizeProperty.vector2IntValue = newSize;
        thisProperty.serializedObject.ApplyModifiedProperties();
    }
}