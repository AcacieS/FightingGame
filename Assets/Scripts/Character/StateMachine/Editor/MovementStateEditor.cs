using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MovementState))]
public class MovementStateEditor : Editor
{
    private SerializedProperty overrideMovementSettings;
    private SerializedProperty movementSpeed;
    private SerializedProperty movementAcceleration;

    private void OnEnable()
    {
        overrideMovementSettings =
            serializedObject.FindProperty("_overrideMovementSettings");

        movementSpeed =
            serializedObject.FindProperty("_movementSpeed");

        movementAcceleration =
            serializedObject.FindProperty("_movementAcceleration");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw everything except the fields we want to control manually
        DrawPropertiesExcluding(
            serializedObject,
            "_overrideMovementSettings",
            "_movementSpeed",
            "_movementAcceleration"
        );

        EditorGUILayout.PropertyField(
            overrideMovementSettings,
            new GUIContent("Override Movement Settings")
        );

        if (overrideMovementSettings.boolValue)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(
                movementSpeed,
                new GUIContent("Movement Speed")
            );

            EditorGUILayout.PropertyField(
                movementAcceleration,
                new GUIContent("Movement Acceleration")
            );

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}