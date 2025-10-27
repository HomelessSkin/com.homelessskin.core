using System.Runtime.CompilerServices;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

namespace Core.Util
{
    public static class Sys
    {
        public static void Deb<T>(T value) => Debug.Log($"{value}");
        public static void Add_M<T>(T request, EntityManager manager) where T : IComponentData
             => manager.AddComponentObject(manager.CreateEntity(), request);
        public static void Add<T>(T request, EntityManager manager) where T : unmanaged, IComponentData
            => manager.AddComponentData(manager.CreateEntity(), request);
        public static void Add<T>(T request, ref EntityCommandBuffer manager) where T : unmanaged, IComponentData
            => manager.AddComponent(manager.CreateEntity(), request);
        public static void Add<T>(T request, ref EntityCommandBuffer.ParallelWriter manager, int index) where T : unmanaged, IComponentData
            => manager.AddComponent(index, manager.CreateEntity(index), request);

        public static void Add2<T, U>(T request1, U request2, EntityManager manager) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            manager.AddComponentData(manager.CreateEntity(), request1);
            manager.AddComponentData(manager.CreateEntity(), request2);
        }
        public static void Add2<T, U>(T request1, U request2, ref EntityCommandBuffer manager) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            manager.AddComponent(manager.CreateEntity(), request1);
            manager.AddComponent(manager.CreateEntity(), request2);
        }
        public static void Add2<T, U>(T request1, U request2, ref EntityCommandBuffer.ParallelWriter manager, int index) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            manager.AddComponent(index, manager.CreateEntity(index), request1);
            manager.AddComponent(index, manager.CreateEntity(index), request2);
        }

        public static void Add<T, U>(T req1, U req2, EntityManager manager) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            var entity = manager.CreateEntity();
            manager.AddComponentData(entity, req1);
            manager.AddComponentData(entity, req2);
        }
        public static void Add<T, U>(T req1, U req2, ref EntityCommandBuffer manager) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            var entity = manager.CreateEntity();
            manager.AddComponent(entity, req1);
            manager.AddComponent(entity, req2);
        }
        public static void Add<T, U>(T req1, U req2, ref EntityCommandBuffer.ParallelWriter manager, int index) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            var entity = manager.CreateEntity(index);
            manager.AddComponent(index, entity, req1);
            manager.AddComponent(index, entity, req2);
        }

        public static void Add<T, U>(T req1, U req2, EntityManager manager, out Entity entity) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            entity = manager.CreateEntity();
            manager.AddComponentData(entity, req1);
            manager.AddComponentData(entity, req2);
        }
        public static void Add<T, U>(T req1, U req2, ref EntityCommandBuffer manager, out Entity entity) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            entity = manager.CreateEntity();
            manager.AddComponent(entity, req1);
            manager.AddComponent(entity, req2);
        }
        public static void Add<T, U>(T req1, U req2, ref EntityCommandBuffer.ParallelWriter manager, int index, out Entity entity) where T : unmanaged, IComponentData where U : unmanaged, IComponentData
        {
            entity = manager.CreateEntity(index);
            manager.AddComponent(index, entity, req1);
            manager.AddComponent(index, entity, req2);
        }

        public static void Add<T>(T request, EntityManager manager, out Entity entity) where T : unmanaged, IComponentData
        {
            entity = manager.CreateEntity();
            manager.AddComponentData(entity, request);
        }
        public static void Add<T>(T request, ref EntityCommandBuffer manager, out Entity entity) where T : unmanaged, IComponentData
        {
            entity = manager.CreateEntity();
            manager.AddComponent(entity, request);
        }
        public static void Add<T>(T request, ref EntityCommandBuffer.ParallelWriter manager, int index, out Entity entity) where T : unmanaged, IComponentData
        {
            entity = manager.CreateEntity(index);
            manager.AddComponent(index, entity, request);
        }

        public static Entity AddEnabled<T>(T request, EntityManager manager, bool enabled = true) where T : unmanaged, IComponentData, IEnableableComponent
        {
            var entity = manager.CreateEntity();
            manager.AddComponentData(entity, request);
            manager.SetComponentEnabled<T>(entity, enabled);

            return entity;
        }
        public static Entity AddEnabled<T>(T request, ref EntityCommandBuffer manager, bool enabled = true) where T : unmanaged, IComponentData, IEnableableComponent
        {
            var entity = manager.CreateEntity();
            manager.AddComponent(entity, request);
            manager.SetComponentEnabled<T>(entity, enabled);

            return entity;
        }
        public static Entity AddEnabled<T>(T request, ref EntityCommandBuffer.ParallelWriter manager, int index, bool enabled = true) where T : unmanaged, IComponentData, IEnableableComponent
        {
            var entity = manager.CreateEntity(index);
            manager.AddComponent(index, entity, request);
            manager.SetComponentEnabled<T>(index, entity, enabled);

            return entity;
        }

        public static Entity CreateAt(LocalTransform transform, EntityManager manager)
        {
            var e = manager.CreateEntity();
            manager.AddComponentData(e, transform);

            return e;
        }
        public static Entity CreateAt(LocalTransform transform, ref EntityCommandBuffer manager)
        {
            var e = manager.CreateEntity();
            manager.AddComponent(e, transform);

            return e;
        }
        public static Entity CreateAt(LocalTransform transform, ref EntityCommandBuffer.ParallelWriter manager, int EIIQ)
        {
            var e = manager.CreateEntity(EIIQ);
            manager.AddComponent(EIIQ, e, transform);

            return e;
        }

        public static Entity InstantiateAt(Entity prefab, LocalTransform transform, EntityManager manager)
        {
            var e = manager.Instantiate(prefab);
            manager.SetComponentData(e, transform);

            return e;
        }
        public static Entity InstantiateAt(Entity prefab, LocalTransform transform, ref EntityCommandBuffer manager)
        {
            var e = manager.Instantiate(prefab);
            manager.SetComponent(e, transform);

            return e;
        }
        public static Entity InstantiateAt(Entity prefab, LocalTransform transform, ref EntityCommandBuffer.ParallelWriter manager, int EIIQ)
        {
            var e = manager.Instantiate(EIIQ, prefab);
            manager.SetComponent(EIIQ, e, transform);

            return e;
        }

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
        public static Entity InstantiateAt(Entity prefab, float3 pos, ref EntityCommandBuffer.ParallelWriter manager, int EIIQ)
        {
            var e = manager.Instantiate(EIIQ, prefab);
            manager.SetComponent(EIIQ, e, LocalTransform.FromPosition(pos));

            return e;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityCommandBuffer ECB(Allocator allocator) => new EntityCommandBuffer(allocator);

        public static void RemoveID<T>(ref this DynamicBuffer<T> buffer, int id) where T : unmanaged, IKeyBuffer
        {
            var index = buffer.GetElementIndex(id);
            if (index >= 0)
                buffer.RemoveAt(index);
        }
        public static bool Contains<T>(this DynamicBuffer<T> buffer, int id) where T : unmanaged, IKeyBuffer => buffer.GetElementIndex(id) >= 0;
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

    public interface IKeyBuffer : IBufferElementData
    {
        public int GetID();
        public Entity GetValue() => Entity.Null;
    }
}