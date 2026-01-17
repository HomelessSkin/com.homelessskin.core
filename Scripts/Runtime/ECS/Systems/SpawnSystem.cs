using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;

using static Unity.Entities.SystemAPI;

namespace Core
{
    [UpdateInGroup(typeof(SpawnSystemGroup), OrderFirst = true)]
    public partial struct SpawnSystem : ISystem
    {
        ComponentLookup<SpawnRequest> RequestLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Core.Prefab>();
            state.RequireForUpdate<SpawnRequest>();

            RequestLookup = GetComponentLookup<SpawnRequest>();
        }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            RequestLookup.Update(ref state);

            var ecb = Sys.ECB(state.WorldUpdateAllocator);
            var query = QueryBuilder()
                .WithAll<SpawnRequest>()
                .Build()
                .ToEntityArray(Allocator.TempJob);

            new SpawnJob
            {
                Entities = query,
                Prefabs = GetSingletonBuffer<Core.Prefab>(),

                RequestLookup = RequestLookup,

                ECB = ecb.AsParallelWriter()
            }
            .Schedule(query.Length, query.Length / JobsUtility.JobWorkerCount, state.Dependency)
            .Complete();

            ecb.Playback(state.EntityManager);
            query.Dispose();
        }

        [BurstCompile]
        partial struct SpawnJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Entity> Entities;
            [ReadOnly] public DynamicBuffer<Core.Prefab> Prefabs;

            [ReadOnly] public ComponentLookup<SpawnRequest> RequestLookup;

            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute(int index)
            {
                var entity = Entities[index];
                var request = RequestLookup[entity];

                var instance = Sys.InstantiateAt(Sys.GetBufferElement(request.PrefabID, Prefabs).Value,
                       LocalTransform.FromPositionRotation(request.Position, request.Rotation),
                       ref ECB, index);

                ECB.AddComponent(index, instance, new Spawn { OperationID = request.OperationID });
                ECB.DestroyEntity(index, entity);
            }
        }
    }

    public struct SpawnRequest : IComponentData
    {
        public long OperationID;
        public int PrefabID;
        public float3 Position;
        public quaternion Rotation;

        public SpawnRequest(int prefabID, int uniqueID, float3 position, quaternion rotation)
        {
            OperationID = uniqueID + position.GetHashCode() + rotation.GetHashCode();
            PrefabID = prefabID;

            Position = position;
            Rotation = rotation;
        }
    }

    public struct Spawn : IComponentData
    {
        public long OperationID;
    }
}