using System;

using Unity.Entities;

using UnityEngine;

namespace Core
{
    [DisallowMultipleComponent]
    public class PrefabBaker : MonoBehaviour
    {
        [SerializeField] PrefabID ID;

        public int GetID() => ID.Value;

        class PrefabBakerBaker : Baker<PrefabBaker>
        {
            public override void Bake(PrefabBaker authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PrefabID { Value = authoring.GetID() });
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            //if (gameObject.activeInHierarchy)
            ID.Value = gameObject.GetPrefabID();
        }
#endif
    }

    [Serializable]
    public struct PrefabID : IComponentData
    {
        public int Value;
    }
}