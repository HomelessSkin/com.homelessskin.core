using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;

using static Unity.Entities.SystemAPI;
using static Unity.Mathematics.math;

namespace Core
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(SpawnSystemGroup))]
    partial struct DestroySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Destroyable>();
        }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!TryGetSingletonEntity<PlayerTag>(out var playerE))
                return;

            var ecb = Sys.ECB(state.WorldUpdateAllocator);

            var query = QueryBuilder().WithAll<Spawn, Destroyable, LocalTransform>().Build();
            var entities = query.ToEntityArray(Allocator.TempJob);
            var spawns = query.ToComponentDataArray<Spawn>(Allocator.TempJob);
            var destroyables = query.ToComponentDataArray<Destroyable>(Allocator.TempJob);
            var transforms = query.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

            new DestroyJob
            {
                Delta = SystemAPI.Time.DeltaTime,
                PlayerPos = GetComponent<LocalTransform>(playerE).Position,

                Entities = entities,
                Spawns = spawns,
                Destroyables = destroyables,
                Transforms = transforms,

                ECB = ecb.AsParallelWriter()
            }
            .Schedule(entities.Length, entities.Length / JobsUtility.JobWorkerCount, state.Dependency)
            .Complete();

            ecb.Playback(state.EntityManager);

            entities.Dispose();
            spawns.Dispose();
            destroyables.Dispose();
            transforms.Dispose();
        }

        [BurstCompile]
        struct DestroyJob : IJobParallelFor
        {
            [ReadOnly] public float Delta;
            [ReadOnly] public float3 PlayerPos;

            [ReadOnly] public NativeArray<Entity> Entities;
            [ReadOnly] public NativeArray<Spawn> Spawns;
            [ReadOnly] public NativeArray<Destroyable> Destroyables;
            [ReadOnly] public NativeArray<LocalTransform> Transforms;

            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute(int index)
            {
                var entity = Entities[index];
                var destroyable = Destroyables[index];

                UpdateValue(entity, index, ref destroyable);
                if (destroyable.IsDone())
                    ECB.DestroyEntity(index, entity);
                else
                    ECB.SetComponent(index, entity, destroyable);
            }

            void UpdateValue(Entity entity, int index, ref Destroyable destroyable)
            {
                switch (destroyable.DestroyBy)
                {
                    case Destroyable.Type.Timer:
                    destroyable.Value += Delta;
                    break;

                    case Destroyable.Type.Y:
                    destroyable.Value = Transforms[index].Position.y;
                    break;

                    case Destroyable.Type.PlayerDistance:
                    destroyable.Value = distance(Transforms[index].Position, PlayerPos);
                    break;
                }
            }
        }
    }
}