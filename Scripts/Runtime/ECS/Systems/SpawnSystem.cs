using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

using static Unity.Entities.SystemAPI;

namespace Core
{
    [UpdateInGroup(typeof(SpawnSystemGroup), OrderFirst = true)]
    public partial struct SpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Source>();
            state.RequireForUpdate<SpawnRequest>();
        }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = Sys.ECB(state.WorldUpdateAllocator);

            var query = QueryBuilder().WithAll<SpawnRequest>().Build();
            var entities = query.ToEntityArray(Allocator.TempJob);
            var requests = query.ToComponentDataArray<SpawnRequest>(Allocator.TempJob);

            new SpawnJob
            {
                Entities = entities,
                Requests = requests,

                Prefabs = GetSingletonBuffer<Core.Source>(),

                ECB = ecb.AsParallelWriter()
            }
            .Schedule(entities.Length, entities.Length / JobsUtility.JobWorkerCount, state.Dependency)
            .Complete();

            ecb.Playback(state.EntityManager);

            entities.Dispose();
            requests.Dispose();
        }

        [BurstCompile]
        partial struct SpawnJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Entity> Entities;
            [ReadOnly] public DynamicBuffer<Source> Prefabs;

            [ReadOnly] public NativeArray<SpawnRequest> Requests;

            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute(int index)
            {
                var entity = Entities[index];
                var request = Requests[index];

                var instance = Sys.InstantiateAt(Sys.GetBufferElement(request.PrefabID, Prefabs).Value,
                       LocalTransform.FromPositionRotation(request.Position, request.Rotation),
                       ref ECB, index);

                ECB.AddComponent(index, instance, new Spawn { OperationID = request.OperationID });
                ECB.DestroyEntity(index, entity);
            }
        }
    }

    [Serializable]
    public struct SpawnRequest : IComponentData
    {
        public long OperationID;
        public int PrefabID;
        public float3 Position;
        public quaternion Rotation;

        public SpawnRequest(int prefabID, int uniqueID)
        {
            OperationID = uniqueID;
            PrefabID = prefabID;

            Position = 0f;
            Rotation = quaternion.identity;
        }
        public SpawnRequest(int prefabID, int uniqueID, float3 position, quaternion rotation)
        {
            OperationID = uniqueID + position.GetHashCode() + rotation.GetHashCode();
            PrefabID = prefabID;

            Position = position;
            Rotation = rotation;
        }
        public SpawnRequest(int prefabID, int uniqueID, Transform transform)
        {
            OperationID = uniqueID + transform.position.GetHashCode() + transform.rotation.GetHashCode();
            PrefabID = prefabID;

            Position = transform.position;
            Rotation = transform.rotation;
        }
        public SpawnRequest(int prefabID, int uniqueID, Transform transform, Vector3 offset)
        {
            OperationID = uniqueID + (transform.position + offset).GetHashCode() + transform.rotation.GetHashCode();
            PrefabID = prefabID;

            Position = transform.position + offset;
            Rotation = transform.rotation;
        }
    }

    public struct Spawn : IComponentData
    {
        public long OperationID;
    }
}