using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using static Unity.Entities.SystemAPI;

namespace Core
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    partial struct SpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Core.Prefab>();
            state.RequireForUpdate<SpawnRequest>();
        }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = Sys.ECB(state.WorldUpdateAllocator);

            new SpawnJob
            {
                Prefabs = GetSingletonBuffer<Core.Prefab>(),
                ECB = ecb.AsParallelWriter()
            }
            .ScheduleParallel(state.Dependency)
            .Complete();

            ecb.Playback(state.EntityManager);
        }

        [BurstCompile]
        partial struct SpawnJob : IJobEntity
        {
            [ReadOnly] public DynamicBuffer<Core.Prefab> Prefabs;

            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute([EntityIndexInQuery] in int EIIQ, in Entity entity, in SpawnRequest request)
            {
                Sys.InstantiateAt(Sys.GetBufferElement(request.ID, Prefabs).Value,
                    LocalTransform.FromPositionRotation(request.Position, request.Rotation),
                    ref ECB, EIIQ);
                ECB.DestroyEntity(EIIQ, entity);
            }
        }
    }

    public struct SpawnRequest : IComponentData
    {
        public int ID;
        public float3 Position;
        public quaternion Rotation;
    }
}