using System.Runtime.CompilerServices;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;

namespace Core
{
    #region SYS
    public static class Sys
    {
        /// <summary>
        /// ето сдвиг нахуй
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add_M<T>(T request, EntityManager manager) where T : IComponentData
             => manager.AddComponentObject(manager.CreateEntity(), request);
        /// <summary>
        /// ето сдвиг нахуй
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add_M<T, U>(T request1, U request2, EntityManager manager)
            where T : IComponentData
            where U : IComponentData
        {
            var entity = manager.CreateEntity();
            manager.AddComponentObject(entity, request1);
            manager.AddComponentObject(entity, request2);
        }

        /// <summary>
        /// ето сдвиг нахуй
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(T request, EntityManager manager) where T : unmanaged, IComponentData
            => manager.AddComponentData(manager.CreateEntity(), request);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(T request, ref EntityCommandBuffer manager) where T : unmanaged, IComponentData
            => manager.AddComponent(manager.CreateEntity(), request);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(T request, ref EntityCommandBuffer.ParallelWriter manager, int index) where T : unmanaged, IComponentData
            => manager.AddComponent(index, manager.CreateEntity(index), request);

        /// <summary>
        /// ето сдвиг нахуй
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add2<T, U>(T request1, U request2, EntityManager manager) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            manager.AddComponentData(manager.CreateEntity(), request1);
            manager.AddComponentData(manager.CreateEntity(), request2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add2<T, U>(T request1, U request2, ref EntityCommandBuffer manager) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            manager.AddComponent(manager.CreateEntity(), request1);
            manager.AddComponent(manager.CreateEntity(), request2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add2<T, U>(T request1, U request2, ref EntityCommandBuffer.ParallelWriter manager, int index) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            manager.AddComponent(index, manager.CreateEntity(index), request1);
            manager.AddComponent(index, manager.CreateEntity(index), request2);
        }

        /// <summary>
        /// ето сдвиг нахуй
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T, U>(T req1, U req2, EntityManager manager) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            var entity = manager.CreateEntity();
            manager.AddComponentData(entity, req1);
            manager.AddComponentData(entity, req2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T, U>(T req1, U req2, ref EntityCommandBuffer manager) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            var entity = manager.CreateEntity();
            manager.AddComponent(entity, req1);
            manager.AddComponent(entity, req2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T, U>(T req1, U req2, ref EntityCommandBuffer.ParallelWriter manager, int index) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            var entity = manager.CreateEntity(index);
            manager.AddComponent(index, entity, req1);
            manager.AddComponent(index, entity, req2);
        }

        /// <summary>
        /// ето сдвиг нахуй
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T, U>(T req1, U req2, EntityManager manager, out Entity entity) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            entity = manager.CreateEntity();
            manager.AddComponentData(entity, req1);
            manager.AddComponentData(entity, req2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T, U>(T req1, U req2, ref EntityCommandBuffer manager, out Entity entity) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            entity = manager.CreateEntity();
            manager.AddComponent(entity, req1);
            manager.AddComponent(entity, req2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T, U>(T req1, U req2, ref EntityCommandBuffer.ParallelWriter manager, int index, out Entity entity) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            entity = manager.CreateEntity(index);
            manager.AddComponent(index, entity, req1);
            manager.AddComponent(index, entity, req2);
        }

        /// <summary>
        /// ето сдвиг нахуй
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(T request, EntityManager manager, out Entity entity) where T : unmanaged, IComponentData
        {
            entity = manager.CreateEntity();
            manager.AddComponentData(entity, request);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(T request, ref EntityCommandBuffer manager, out Entity entity) where T : unmanaged, IComponentData
        {
            entity = manager.CreateEntity();
            manager.AddComponent(entity, request);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(T request, ref EntityCommandBuffer.ParallelWriter manager, int index, out Entity entity) where T : unmanaged, IComponentData
        {
            entity = manager.CreateEntity(index);
            manager.AddComponent(index, entity, request);
        }

        /// <summary>
        /// ето сдвиг нахуй
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity AddEnabled<T>(T request, EntityManager manager, bool enabled = true) where T : unmanaged, IComponentData, IEnableableComponent
        {
            var entity = manager.CreateEntity();
            manager.AddComponentData(entity, request);
            manager.SetComponentEnabled<T>(entity, enabled);

            return entity;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity AddEnabled<T>(T request, ref EntityCommandBuffer manager, bool enabled = true) where T : unmanaged, IComponentData, IEnableableComponent
        {
            var entity = manager.CreateEntity();
            manager.AddComponent(entity, request);
            manager.SetComponentEnabled<T>(entity, enabled);

            return entity;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity AddEnabled<T>(T request, ref EntityCommandBuffer.ParallelWriter manager, int index, bool enabled = true) where T : unmanaged, IComponentData, IEnableableComponent
        {
            var entity = manager.CreateEntity(index);
            manager.AddComponent(index, entity, request);
            manager.SetComponentEnabled<T>(index, entity, enabled);

            return entity;
        }

        /// <summary>
        /// ето сдвиг нахуй
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity CreateAt(LocalTransform transform, EntityManager manager)
        {
            var e = manager.CreateEntity();
            manager.AddComponentData(e, transform);

            return e;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity CreateAt(LocalTransform transform, ref EntityCommandBuffer manager)
        {
            var e = manager.CreateEntity();
            manager.AddComponent(e, transform);

            return e;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity CreateAt(LocalTransform transform, ref EntityCommandBuffer.ParallelWriter manager, int EIIQ)
        {
            var e = manager.CreateEntity(EIIQ);
            manager.AddComponent(EIIQ, e, transform);

            return e;
        }

        /// <summary>
        /// ето сдвиг нахуй
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity InstantiateAt(Entity prefab, LocalTransform transform, EntityManager manager)
        {
            var e = manager.Instantiate(prefab);
            manager.SetComponentData(e, transform);

            return e;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity InstantiateAt(Entity prefab, LocalTransform transform, ref EntityCommandBuffer manager)
        {
            var e = manager.Instantiate(prefab);
            manager.SetComponent(e, transform);

            return e;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity InstantiateAt(Entity prefab, LocalTransform transform, ref EntityCommandBuffer.ParallelWriter manager, int EIIQ)
        {
            var e = manager.Instantiate(EIIQ, prefab);
            manager.SetComponent(EIIQ, e, transform);

            return e;
        }

        /// <summary>
        /// ето сдвиг нахуй
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity InstantiateAt(Entity prefab, float3 pos, EntityManager manager)
        {
            var e = manager.Instantiate(prefab);
            manager.SetComponentData(e, LocalTransform.FromPosition(pos));

            return e;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity InstantiateAt(Entity prefab, float3 pos, ref EntityCommandBuffer manager)
        {
            var e = manager.Instantiate(prefab);
            manager.SetComponent(e, LocalTransform.FromPosition(pos));

            return e;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity InstantiateAt(Entity prefab, float3 pos, ref EntityCommandBuffer.ParallelWriter manager, int EIIQ)
        {
            var e = manager.Instantiate(EIIQ, prefab);
            manager.SetComponent(EIIQ, e, LocalTransform.FromPosition(pos));

            return e;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityCommandBuffer ECB(Allocator allocator) => new EntityCommandBuffer(allocator);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveID<T>(ref this DynamicBuffer<T> buffer, int id) where T : unmanaged, IKeyBuffer
        {
            var index = buffer.GetElementIndex(id);
            if (index >= 0)
                buffer.RemoveAt(index);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(this DynamicBuffer<T> buffer, int id) where T : unmanaged, IKeyBuffer => buffer.GetElementIndex(id) >= 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetElementIndex<T>(this DynamicBuffer<T> buffer, int id) where T : unmanaged, IKeyBuffer
        {
            for (int b = 0; b < buffer.Length; b++)
                if (buffer[b].GetID() == id)
                    return b;

            return -1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetBufferElement<T>(int id, DynamicBuffer<T> buffer) where T : unmanaged, IKeyBuffer
        {
            for (int b = 0; b < buffer.Length; b++)
                if (buffer[b].GetID() == id)
                    return buffer[b];

            return buffer[0];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeList<T> GetConnected<U, T>(DynamicBuffer<U> from, DynamicBuffer<T> with, Allocator allocator = Allocator.Temp)
            where T : unmanaged, IKeyBuffer
            where U : unmanaged, IKeyBuffer
        {
            var list = new NativeList<T>(allocator);

            for (int f = 0; f < from.Length; f++)
            {
                var ff = from[f].GetID();
                for (int w = 0; w < with.Length; w++)
                {
                    var ww = with[w];
                    if (ff == ww.GetID())
                        list.Add(ww);
                }
            }

            return list;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity GetBufferElement_Job<T>(int id, DynamicBuffer<T> buffer) where T : unmanaged, IKeyBuffer
        {
            var result = buffer[0].GetValue();
            var array = new NativeArray<int>(1, Allocator.TempJob);

            new GetBufferElementJob<T>
            {
                ID = id,
                Buffer = buffer,
                Result = array,
            }
            .Schedule(buffer.Length, buffer.Length / JobsUtility.JobWorkerCount)
            .Complete();

            if (array[0] > 0)
                result = buffer[array[0]].GetValue();

            array.Dispose();

            return result;
        }
    }
    #endregion

    [BurstCompile]
    struct GetBufferElementJob<T> : IJobParallelFor where T : unmanaged, IKeyBuffer
    {
        [ReadOnly] public int ID;
        [ReadOnly] public DynamicBuffer<T> Buffer;

        [NativeDisableParallelForRestriction] public NativeArray<int> Result;

        public void Execute(int index)
        {
            if (Buffer[index].GetID() == ID)
                Result[0] = index;
        }
    }

    public abstract partial class BehaviourSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            GetRef();
            Proceed();
        }
        protected abstract void GetRef();
        protected abstract void Proceed();
    }

    public abstract partial class ReloadManagedSingletoneSystem<T> : BehaviourSystem
        where T : IComponentData, new()
    {
        protected T Value;

        protected override void OnUpdate()
        {
            Reload();
            if (Value == null)
                return;

            base.OnUpdate();
        }

        void Reload()
        {
            var query = EntityManager.CreateEntityQuery(typeof(T));
            switch (query.CalculateEntityCount())
            {
                case 0:
                Sys.Add_M<T>(new T(), EntityManager);
                break;
                case 1:
                Value = EntityManager.GetComponentObject<T>(query.GetSingletonEntity());
                break;
                default:
                EntityManager.DestroyEntity(query);
                break;
            }
        }
    }

    public enum LogLevel : byte
    {
        Nominal = 0,
        Error = 1,
        Warning = 2,

    }
}