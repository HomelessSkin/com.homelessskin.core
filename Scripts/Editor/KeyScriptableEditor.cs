#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

namespace Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(KeyScriptable), true)]
    public class KeyScriptableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var scriptable = (KeyScriptable)target;
            if (scriptable == null)
                return;

            if (GUILayout.Button("Randomize ID"))
            {
                var len = targets.Length;
                for (int t = 0; t < len; t++)
                {
                    EditorUtility.DisplayProgressBar($"Randomizing ID's", $"{t} out of {len}", Mathf.Floor((float)t / len));
                    ((KeyScriptable)targets[t]).RandomizeID(true);
                    EditorUtility.SetDirty(targets[t]);
                }

                EditorUtility.ClearProgressBar();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            base.OnInspectorGUI();
        }
    }
}
#endif