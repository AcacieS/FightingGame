using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DistanceConsideration))]
public class DistanceConsiderationEditor : Editor
{
    private SerializedProperty weight;
    private SerializedProperty mode;
    private SerializedProperty minDistance;
    private SerializedProperty maxDistance;
    private SerializedProperty curve;

    private void OnEnable()
    {
        weight = serializedObject.FindProperty("weight");
        mode = serializedObject.FindProperty("mode");
        minDistance = serializedObject.FindProperty("minDistance");
        maxDistance = serializedObject.FindProperty("maxDistance");
        curve = serializedObject.FindProperty("curve");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField(
            "Script",
            MonoScript.FromMonoBehaviour((DistanceConsideration)target),
            typeof(MonoScript),
            false
        );
        EditorGUI.EndDisabledGroup();
        // Draw inherited Consideration field
        EditorGUILayout.PropertyField(weight);

        // Draw DistanceConsideration fields
        EditorGUILayout.PropertyField(mode);

        DistanceConsideration.Mode currentMode =
            (DistanceConsideration.Mode)mode.enumValueIndex;

        switch (currentMode)
        {
            case DistanceConsideration.Mode.Close:

                EditorGUILayout.PropertyField(
                    maxDistance,
                    new GUIContent("Max Distance")
                );

                break;

            case DistanceConsideration.Mode.Far:

                EditorGUILayout.PropertyField(
                    minDistance,
                    new GUIContent("Min Distance")
                );

                break;

            case DistanceConsideration.Mode.Curve:
                EditorGUILayout.PropertyField(minDistance);
                EditorGUILayout.PropertyField(maxDistance);

                // This displays Unity's built-in curve editor
                EditorGUILayout.PropertyField(curve);
                break;
            case DistanceConsideration.Mode.Range:

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