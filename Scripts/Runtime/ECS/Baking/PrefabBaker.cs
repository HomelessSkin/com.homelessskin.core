using System;

using Unity.Entities;

using UnityEngine;

namespace Core
{
    /// <summary>
    /// добавь в бейкер AddComponent(entity, authoring.ID); или не наследуй блять просто так
    /// </summary>
    public class PrefabBaker : MonoBehaviour
    {
        //[SerializeField] int SpawnChance;
        //[SerializeField] float SpawnDelay;
        [SerializeField] protected PrefabID ID;

        public int GetID() => ID.Value;
        //public int GetSpawnChance() => SpawnChance;
        //public float GetSpawnDelay() => SpawnDelay;

        public long Spawn(int unique = 0) => Spawn(unique, Vector3.zero, Quaternion.identity);
        public long Spawn(int unique, Transform transform) => Spawn(unique, transform.position, transform.rotation);
        public virtual long Spawn(int unique, Vector3 position, Quaternion rotation)
        {
            var request = new SpawnRequest(ID.Value, unique, position, rotation);

            Sys.Add(request, World.DefaultGameObjectInjectionWorld.EntityManager);

            return request.OperationID;
        }

        class PrefabBakerBaker : Baker<PrefabBaker>
        {
            public override void Bake(PrefabBaker authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, authoring.ID);
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
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