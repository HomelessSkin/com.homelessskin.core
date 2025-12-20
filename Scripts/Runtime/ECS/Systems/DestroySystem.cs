using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Transforms;

using static Unity.Entities.SystemAPI;

namespace Core
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(SpawnSystemGroup))]
    partial struct DestroySystem : ISystem
    {
        ComponentLookup<Spawn> SpawnLookup;
        ComponentLookup<Destroyable> DestroyableLookup;
        ComponentLookup<LocalTransform> TransformLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Destroyable>();

            SpawnLookup = GetComponentLookup<Spawn>();
            DestroyableLookup = GetComponentLookup<Destroyable>();
            TransformLookup = GetComponentLookup<LocalTransform>();
        }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            SpawnLookup.Update(ref state);
            DestroyableLookup.Update(ref state);
            TransformLookup.Update(ref state);

            var ecb = Sys.ECB(state.WorldUpdateAllocator);

            var query = QueryBuilder()
                .WithAll<Destroyable>()
                .Build()
                .ToEntityArray(Allocator.TempJob);

            new DestroyJob
            {
                Delta = SystemAPI.Time.DeltaTime,
                Entities = query,

                SpawnLookup = SpawnLookup,
                DestroyableLookup = DestroyableLookup,
                TransformLookup = TransformLookup,

                ECB = ecb.AsParallelWriter()
            }
            .Schedule(query.Length, query.Length / JobsUtility.JobWorkerCount, state.Dependency)
            .Complete();

            ecb.Playback(state.EntityManager);
            query.Dispose();
        }

        [BurstCompile]
        struct DestroyJob : IJobParallelFor
        {
            [ReadOnly] public float Delta;
            [ReadOnly] public NativeArray<Entity> Entities;

            [ReadOnly] public ComponentLookup<Spawn> SpawnLookup;
            [ReadOnly] public ComponentLookup<Destroyable> DestroyableLookup;
            [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;

            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute(int index)
            {
                var entity = Entities[index];
                var destroyable = DestroyableLookup[entity];

                UpdateValue(entity, ref destroyable);

                if (destroyable.IsDone())
                {
                    ECB.DestroyEntity(index, entity);

                    if (SpawnLookup.HasComponent(entity))
                        Sys.Add(new DestroyEvent(SpawnLookup[entity].OperationID),
                            new Destroyable(Destroyable.Type.None, Destroyable.CompareType.Null, 0f),
                            ref ECB, index);
                }
                else
                    ECB.SetComponent(index, entity, destroyable);
            }

            void UpdateValue(Entity entity, ref Destroyable destroyable)
            {
                switch (destroyable.DestroyBy)
                {
                    case Destroyable.Type.Timer:
                    destroyable.Value += Delta;
                    break;
                    case Destroyable.Type.Y:
                    destroyable.Value = TransformLookup[entity].Position.y;
                    break;
                }
            }
        }
    }

    public struct DestroyEvent : IComponentData
    {
        public long ID;

        public DestroyEvent(long id)
        {
            ID = id;
        }
    }
}