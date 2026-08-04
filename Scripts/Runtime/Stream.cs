using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using Unity.Scenes;
using Unity.Transforms;

using static Unity.Entities.SystemAPI;
using static Unity.Mathematics.math;

namespace Core
{
    public static class Stream
    {
        public static Entity Scene(
            EntitySceneReference scene,
            WorldUnmanaged world,
            SceneTransform offset,
            EntityManager manager,
            bool autoLoad = false,
            int priority = 0,
            SceneLoadFlags flags = SceneLoadFlags.NewInstance)
        {
            var sceneEntity = SceneSystem.LoadSceneAsync(world, scene,
                new SceneSystem.LoadParameters
                {
                    AutoLoad = autoLoad,
                    Priority = priority,
                    Flags = flags,
                });

            var buf = new PostLoadCommandBuffer();
            buf.CommandBuffer = new EntityCommandBuffer(Allocator.Persistent);

            var postLoadEntity = buf.CommandBuffer.CreateEntity();
            buf.CommandBuffer.AddComponent(postLoadEntity, offset);

            manager.AddComponentData(sceneEntity, buf);

            return sceneEntity;
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ProcessAfterLoad)]
    public partial struct SceneOffsetSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SceneTransform>();
        }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var offsetQuery = QueryBuilder().WithAll<SceneTransform>().Build();
            var offsets = offsetQuery.ToComponentDataArray<SceneTransform>(Allocator.Temp);
            state.EntityManager.DestroyEntity(offsetQuery);

            foreach (var offset in offsets)
                foreach (var transform in Query<RefRW<LocalTransform>>())
                {
                    transform.ValueRW.Position = offset.Translation + mul(offset.Rotation, transform.ValueRO.Position);
                    transform.ValueRW.Rotation = offset.Rotation;
                }
        }
    }

    public struct SceneTransform : IComponentData
    {
        public float3 Translation;
        public quaternion Rotation;
    }
}