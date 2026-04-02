using Unity.Entities;

using UnityEngine;

namespace Core
{
    class PrefabReferenceBaker : ResourceLoader
    {
        [SerializeField] PrefabBaker[] Prefabs;

        class PrefabReferenceBakerBaker : Baker<PrefabReferenceBaker>
        {
            public override void Bake(PrefabReferenceBaker authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddBuffer<Source>(entity);

                if (authoring.Prefabs != null)
                    for (int p = 0; p < authoring.Prefabs.Length; p++)
                        AppendToBuffer(entity, new Source
                        {
                            ID = authoring.Prefabs[p].gameObject.GetPrefabID(),
                            Value = GetEntity(authoring.Prefabs[p], TransformUsageFlags.Dynamic),
                        });
            }
        }

#if UNITY_EDITOR
        protected override void LoadResources() => Load<PrefabBaker>(ref Prefabs);
#endif
    }

    public struct Source : IKeyBuffer
    {
        public int ID;
        public Entity Value;

        public int GetID() => ID;
        public Entity GetValue() => Value;
    }
}