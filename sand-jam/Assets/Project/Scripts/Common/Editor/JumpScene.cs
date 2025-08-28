using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;

[EditorToolbarElement(id, typeof(SceneView))]
class GotoSceneButton : EditorToolbarButton
{
    public const string id = "ExampleToolbar/GotoScene";
    private ObjectField field;

    public GotoSceneButton()
    {
        text = "GotoScene";

        field = new ObjectField();
        field.objectType = typeof(Scene);
        Add(field);

        clicked += () =>
        {
            if (field.value == null)
                return;
            Debug.Log($"open scene: {field.value}");
            EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(field.value));
        };
    }
}

[Overlay(typeof(SceneView), "ExampleToolBar")]
public class EditorToolbarExample : ToolbarOverlay
{
    EditorToolbarExample() : base(
        GotoSceneButton.id
        )
    { }
}