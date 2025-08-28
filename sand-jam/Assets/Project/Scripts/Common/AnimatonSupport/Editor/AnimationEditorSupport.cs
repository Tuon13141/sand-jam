using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

[CustomEditor(typeof(Animation))]
public class AnimationEditorSupport : Editor
{
    private int selectedClipIndex = 0;
    private string selectedClip;
    private float rateFrame = 0;
    private bool preview = false;

    private string[] clipNames
    {
        get
        {
            string s = string.Empty;
            foreach (AnimationState state in (Animation)target)
            {
                if (state.clip != null)
                {
                    if (string.IsNullOrEmpty(s))
                    {
                        s = state.clip.name;
                    }
                    else
                    {
                        s = string.Format("{0},{1}", s, state.clip.name);
                    }
                }
            }

            return s.Split(",");
        }
        set
        {
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();

        Rect rect = EditorGUILayout.GetControlRect(false, 10);
        rect.height = 1;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

        Animation animation = (Animation)target;

        selectedClipIndex = EditorGUILayout.Popup("Select Animation Clip", selectedClipIndex, clipNames);
        selectedClip = clipNames[selectedClipIndex];

        preview = EditorGUILayout.Toggle("Preview", preview);
        EditorGUI.BeginDisabledGroup(!preview);
        {
            EditorGUILayout.Space();
            rateFrame = EditorGUILayout.Slider("Rate Frame", rateFrame, 0f, 1f);
            if (!animation.isPlaying && preview)
            {
                SetupFrameAnimation(clipNames[selectedClipIndex], rateFrame);
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play"))
            {
                animation.Play(selectedClip);
            }
            if (GUILayout.Button("Stop"))
            {
                animation.Stop();
            }
            if (GUILayout.Button("Rewind"))
            {
                animation.Rewind(selectedClip);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndVertical();
    }

    private void SetupFrameAnimation(string animName, float rateFrame)
    {
        var animation = (Animation)target;

        var animationClip = animation.GetClip(animName);
        if (animationClip == null) return;

        animationClip.SampleAnimation(animation.gameObject, rateFrame);
    }
}
