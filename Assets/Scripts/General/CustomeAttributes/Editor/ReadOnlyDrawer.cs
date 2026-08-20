using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        if (property.isArray && property.propertyType != SerializedPropertyType.String)
        {
            DrawReadOnlyList(position, property, label);
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);

            EditorGUI.PropertyField(
                position,
                property,
                label,
                true
            );

            EditorGUI.EndDisabledGroup();
        }

        EditorGUI.EndProperty();
    }

    private void DrawReadOnlyList(
        Rect position,
        SerializedProperty property,
        GUIContent label)
    {
        Rect foldoutRect = new Rect(
            position.x,
            position.y,
            position.width,
            EditorGUIUtility.singleLineHeight
        );

        property.isExpanded = EditorGUI.Foldout(
            foldoutRect,
            property.isExpanded,
            label
        );

        if (!property.isExpanded)
            return;

        float y = position.y + EditorGUIUtility.singleLineHeight + 2;

        EditorGUI.BeginDisabledGroup(true);

        for (int i = 0; i < property.arraySize; i++)
        {
            SerializedProperty element = property.GetArrayElementAtIndex(i);

            float height = EditorGUI.GetPropertyHeight(
                element,
                true
            );

            Rect elementRect = new Rect(
                position.x + 15,
                y,
                position.width - 15,
                height
            );

            EditorGUI.PropertyField(
                elementRect,
                element,
                new GUIContent($"Element {i}"),
                true
            );

            y += height + 2;
        }

        EditorGUI.EndDisabledGroup();
    }

    public override float GetPropertyHeight(
        SerializedProperty property,
        GUIContent label)
    {
        if (!property.isArray ||
            property.propertyType == SerializedPropertyType.String ||
            !property.isExpanded)
        {
            return EditorGUI.GetPropertyHeight(
                property,
                label,
                true
            );
        }

        float height = EditorGUIUtility.singleLineHeight + 2;

        for (int i = 0; i < property.arraySize; i++)
        {
            SerializedProperty element =
                property.GetArrayElementAtIndex(i);

            height += EditorGUI.GetPropertyHeight(
                element,
                true
            ) + 2;
        }

        return height;
    }
}