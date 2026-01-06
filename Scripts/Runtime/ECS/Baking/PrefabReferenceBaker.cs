using Unity.Entities;

using UnityEngine;

namespace Core
{
    class PrefabReferenceBaker : MonoBehaviour
    {
        [SerializeField] PrefabBaker[] Prefabs;

        class PrefabReferenceBakerBaker : Baker<PrefabReferenceBaker>
        {
            public override void Bake(PrefabReferenceBaker authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddBuffer<Prefab>(entity);

                if (authoring.Prefabs != null)
                    for (int p = 0; p < authoring.Prefabs.Length; p++)
                        AppendToBuffer(entity, new Prefab
                        {
                            ID = authoring.Prefabs[p].GetID(),
                            Value = GetEntity(authoring.Prefabs[p], TransformUsageFlags.Dynamic),
                        });
            }
        }
    }

    public struct Prefab : IKeyBuffer
    {
        public int ID;
        public Entity Value;

        public int GetID() => ID;
        public Entity GetValue() => Value;
    }
}