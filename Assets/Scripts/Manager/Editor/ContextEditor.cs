using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Context))]
public class ContextEditor : Editor
{
    private SerializedProperty overrideCharactersSettings;
    private SerializedProperty player;
    private SerializedProperty enemy;
    private double nextRepaint;

    private void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
        overrideCharactersSettings =
            serializedObject.FindProperty("_overrideCharactersSettings");

        player =
            serializedObject.FindProperty("_target");

        enemy =
            serializedObject.FindProperty("_self");
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
    }

    private void EditorUpdate()
    {
        if (!Application.isPlaying)
            return;

        if (EditorApplication.timeSinceStartup >= nextRepaint)
        {
            Repaint();

            nextRepaint =
                EditorApplication.timeSinceStartup + 0.05;
        }
    }

    public override void OnInspectorGUI()
    {
        Context context = (Context)target;

        serializedObject.Update();

        DrawPropertiesExcluding(
            serializedObject,
            "_overrideCharactersSettings",
            "_self",
            "_target"
        );
        EditorGUILayout.PropertyField(
            overrideCharactersSettings,
            new GUIContent("Override Characters Settings")
        );

        if (overrideCharactersSettings.boolValue)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(
                enemy,
                new GUIContent("Self: Enemy")
            );

            EditorGUILayout.PropertyField(
                player,
                new GUIContent("Target: Player")
            );

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField(
            "Runtime Context",
            EditorStyles.boldLabel
        );

        EditorGUI.BeginDisabledGroup(true);
        
        EditorGUILayout.FloatField(
            "Distance",
            context.Distance
        );
        EditorGUILayout.FloatField(
            "Direction",
            context.Direction
        );
        EditorGUILayout.FloatField(
            "DirectionSign",
            context.DirectionSign
        );

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField(
            "Health",
            EditorStyles.boldLabel
        );

        EditorGUILayout.FloatField(
            "Self HP",
            context.SelfHp
        );

        EditorGUILayout.FloatField(
            "Target HP",
            context.TargetHp
        );

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField(
            "Conditions",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Toggle(
            "Self Low Health",
            context.IsLowHealth
        );

        EditorGUILayout.Toggle(
            "Target Low Health",
            context.TargetIsLowHealth
        );

        EditorGUILayout.Toggle(
            "In Attack Range",
            context.IsInAttackRange
        );

        EditorGUILayout.Toggle(
            "Target Attacking",
            context.TargetIsAttacking
        );

        EditorGUILayout.Toggle(
            "Target Blocking",
            context.TargetIsBlocking
        );

        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
    }
}