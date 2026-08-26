using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DistanceRequirement))]
public class DistanceRequirementEditor : Editor
{
    private SerializedProperty mode;
    private SerializedProperty minDistance;
    private SerializedProperty maxDistance;

    private void OnEnable()
    {
        mode = serializedObject.FindProperty("mode");
        minDistance = serializedObject.FindProperty("minDistance");
        maxDistance = serializedObject.FindProperty("maxDistance");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(
            serializedObject,
            "mode",
            "minDistance",
            "maxDistance"
        );

        EditorGUILayout.PropertyField(mode);

        DistanceRequirement.Mode currentMode =
            (DistanceRequirement.Mode)mode.enumValueIndex;

        switch (currentMode)
        {
            case DistanceRequirement.Mode.Close:

                EditorGUILayout.PropertyField(
                    maxDistance,
                    new GUIContent("Max Distance")
                );

                break;

            case DistanceRequirement.Mode.Far:

                EditorGUILayout.PropertyField(
                    minDistance,
                    new GUIContent("Min Distance")
                );

                break;

            case DistanceRequirement.Mode.Range:

                EditorGUILayout.PropertyField(
                    minDistance,
                    new GUIContent("Min Distance")
                );

                EditorGUILayout.PropertyField(
                    maxDistance,
                    new GUIContent("Max Distance")
                );

                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}