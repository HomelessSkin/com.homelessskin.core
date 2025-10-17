#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

namespace Core.Rendering
{
    [CustomPropertyDrawer(typeof(RenderProperty))]
    public class RenderablePropertyDrawer : PropertyDrawer
    {
        float VerticalSpacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.IndentedRect(position);

            // Calculate rects with indentation
            var typeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var referenceRect = new Rect(position.x, typeRect.yMax + VerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
            var valueRect = new Rect(position.x, referenceRect.yMax + VerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);

            var valueTypeProp = property.FindPropertyRelative("ValueType");
            EditorGUI.PropertyField(typeRect, valueTypeProp);

            var currentType = (RenderProperty.Type)valueTypeProp.enumValueIndex;
            if (currentType != RenderProperty.Type.Null)
            {
                var referenceProp = property.FindPropertyRelative("Reference");
                EditorGUI.PropertyField(referenceRect, referenceProp);
                DrawValueField(valueRect, property, currentType);
            }

            EditorGUI.EndProperty();
        }

        void DrawValueField(Rect position, SerializedProperty parentProperty, RenderProperty.Type currentType)
        {
            var label = currentType switch
            {
                RenderProperty.Type.Int => "Int Value",
                RenderProperty.Type.Float => "Float Value",
                RenderProperty.Type.Vector2 => "Vector2 Value",
                RenderProperty.Type.Vector3 => "Vector3 Value",
                RenderProperty.Type.Boolean => "Boolean Value",
                _ => "Value"
            };

            var propName = currentType switch
            {
                RenderProperty.Type.Int => "IntValue",
                RenderProperty.Type.Float => "FloatValue",
                RenderProperty.Type.Vector2 => "Vector2Value",
                RenderProperty.Type.Vector3 => "Vector3Value",
                RenderProperty.Type.Boolean => "BooleanValue",
                _ => null
            };

            if (string.IsNullOrEmpty(propName))
                return;

            var prop = parentProperty.FindPropertyRelative(propName);
            position = EditorGUI.PrefixLabel(position, new GUIContent(label));
            EditorGUI.PropertyField(position, prop, GUIContent.none);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var lineCount = 2;

            var valueTypeProp = property.FindPropertyRelative("ValueType");
            if ((RenderProperty.Type)valueTypeProp.enumValueIndex != RenderProperty.Type.Null)
                lineCount++;

            return (EditorGUIUtility.singleLineHeight * lineCount) +
                   (VerticalSpacing * (lineCount - 1));
        }
    }
}
#endif