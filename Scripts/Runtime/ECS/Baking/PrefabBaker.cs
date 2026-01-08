using Unity.Entities;

using UnityEditor;

using UnityEngine;

namespace Core
{
    [DisallowMultipleComponent]
    public class PrefabBaker : MonoBehaviour
    {
        [SerializeField] int ID;

        public int GetID() => ID;

        class PrefabBakerBaker : Baker<PrefabBaker>
        {
            public override void Bake(PrefabBaker authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PrefabID { ID = authoring.GetID() });
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                ID = this.GetPrefabID();

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
#endif
    }

    public struct PrefabID : IComponentData
    {
        public int ID;
    }
}