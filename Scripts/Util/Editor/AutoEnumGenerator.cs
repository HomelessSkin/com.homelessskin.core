#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

namespace Game.Util
{
    [CustomEditor(typeof(AutoEnumGenerator))]
    public class AutoEnumGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var generator = (AutoEnumGenerator)target;
            if (generator == null)
                return;

            if (GUILayout.Button("Generate"))
                generator.Generate();

            base.OnInspectorGUI();
        }
    }
}
#endif