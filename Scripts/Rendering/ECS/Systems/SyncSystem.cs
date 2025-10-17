using Core.Util;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

using static Unity.Entities.SystemAPI;
using static Unity.Mathematics.math;

namespace Core.Rendering
{
    [UpdateInGroup(typeof(RenderSystemGroup))]
    public partial struct SyncMatricesSystem : ISystem
    {
        ComponentLookup<LocalToWorld> LTWLookup;
        ComponentLookup<PhysicsCollider> ColliderLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Renderable>();

            LTWLookup = GetComponentLookup<LocalToWorld>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            LTWLookup.Update(ref state);

            var ecb = Sys.ECB(state.WorldUpdateAllocator);

            var objects = QueryBuilder()
                .WithAll<RenderableObjectID>()
                .Build()
                .ToEntityArray(Allocator.TempJob);
            var renderables = QueryBuilder()
                .WithAll<Renderable>()
                .Build();

            new MatrixJob
            {
                LTWLookup = LTWLookup,
                Objects = objects,
                ECB = ecb.AsParallelWriter()
            }
            .ScheduleParallel(renderables, state.Dependency)
            .Complete();
            ecb.Playback(state.EntityManager);

            objects.Dispose();
        }

        [BurstCompile]
        partial struct MatrixJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<LocalToWorld> LTWLookup;
            [ReadOnly] public NativeArray<Entity> Objects;

            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute([EntityIndexInQuery] in int EIIQ, in Entity entity, ref Renderable renderable)
            {
                var found = false;
                for (int e = 0; e < Objects.Length; e++)
                {
                    var objE = Objects[e];
                    if (renderable.Entity.Equals(objE))
                    {
                        found = true;

                        var ltw = LTWLookup[objE].Value;
                        renderable.Matrix = mul(ltw, renderable.Offset);

                        if (renderable.RenderType == Renderable.Type.Static)
                        {
                            ECB.SetComponentEnabled<RenderableObjectID>(EIIQ, objE, false);
                            ECB.SetComponentEnabled<Renderable>(EIIQ, entity, false);
                        }

                        break;
                    }
                }

                if (!found)
                    ECB.DestroyEntity(EIIQ, entity);
            }
        }
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class RenderSystemGroup : ComponentSystemGroup
    {

    }
}