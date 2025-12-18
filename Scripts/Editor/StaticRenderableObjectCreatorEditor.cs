#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

namespace Core
{
    [CustomEditor(typeof(StaticRenderableObjectCreator))]
    public class StaticRenderableObjectCreatorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var creator = (StaticRenderableObjectCreator)target;
            if (creator == null)
                return;


            if (GUILayout.Button("Randomize Seed"))
            {
                creator.SetSeed((uint)Random.Range(1, int.MaxValue));
                serializedObject.ApplyModifiedProperties();

                EditorUtility.SetDirty(creator);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Create"))
                creator.Create();

            if (GUILayout.Button("Destroy"))
                creator.DestroyChildren();

            base.OnInspectorGUI();
        }
    }
}
#endif