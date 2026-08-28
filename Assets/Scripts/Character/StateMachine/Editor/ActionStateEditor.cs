using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ActionState), true)]
public class ActionStateEditor : Editor
{
    private SerializedProperty overrideDuration;
    private SerializedProperty desiredDuration;

    private void OnEnable()
    {
        overrideDuration =
            serializedObject.FindProperty("overrideDuration");

        desiredDuration =
            serializedObject.FindProperty("desiredDuration");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw all properties normally.
        DrawPropertiesExcluding(
            serializedObject,
            "overrideDuration",
            "desiredDuration"
        );

        EditorGUILayout.Space();

        // Only customize this specific section.
        EditorGUILayout.LabelField(
            "Animation Duration",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            overrideDuration,
            new GUIContent("Override Duration")
        );

        if (overrideDuration.boolValue)
        {
            EditorGUILayout.PropertyField(
                desiredDuration,
                new GUIContent("Desired Duration")
            );
        }

        serializedObject.ApplyModifiedProperties();
    }
}