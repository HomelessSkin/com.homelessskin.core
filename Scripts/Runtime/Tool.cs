#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

namespace Core
{
    public static class Tool
    {
        public static void CreateTag(string tagName)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tagsProp = tagManager.FindProperty("tags");

            var found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                var t = tagsProp.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(tagName))
                {
                    found = true;

                    break;
                }
            }

            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                var newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
                newTag.stringValue = tagName;
                tagManager.ApplyModifiedProperties();

                Debug.Log($"Tag '{tagName}' created successfully!");
            }
        }
    }
}
#endif