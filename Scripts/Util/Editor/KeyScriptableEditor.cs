#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

namespace Core.Util
{
    [CustomEditor(typeof(KeyScriptable), true)]
    public class KeyScriptableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var scriptable = (KeyScriptable)target;
            if (scriptable == null)
                return;

            if (GUILayout.Button("Randomize ID"))
                scriptable.RandomizeID();

            base.OnInspectorGUI();
        }
    }
}
#endif