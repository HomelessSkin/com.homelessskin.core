using Unity.Collections;
using Unity.Entities;

namespace Core
{
    [UpdateInGroup(typeof(SpawnSystemGroup))]
    [UpdateAfter(typeof(SpawnSystem))]
    public abstract partial class SpawnResetSystem<T> : SystemBase where T : unmanaged, IComponentData
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            RequireForUpdate<Spawn>();
            RequireForUpdate<ResetRequest>();
        }
        protected override void OnUpdate()
        {
            var ecb = Sys.ECB(Allocator.Temp);

            var query = SystemAPI.QueryBuilder().WithAll<Spawn>().Build();
            var entities = query.ToEntityArray(Allocator.Temp);
            var spawns = query.ToComponentDataArray<Spawn>(Allocator.Temp);

            foreach (var (request, entity) in SystemAPI.Query<ResetRequest>().WithEntityAccess())
            {
                if (request.Component.GetType() != typeof(T))
                    continue;

                for (int e = 0; e < entities.Length; e++)
                    if (spawns[e].OperationID == request.ID)
                    {
                        ecb.DestroyEntity(entity);

                        var component = (T)request.Component;
                        ecb.AddComponent(entities[e], component);

                        break;
                    }
            }

            ecb.Playback(EntityManager);
        }
    }

    public class ResetRequest : IComponentData
    {
        public long ID;
        public IComponentData Component;
    }
}