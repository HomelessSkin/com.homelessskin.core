using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace Core.Rendering
{
    [UpdateInGroup(typeof(RenderSystemGroup))]
    [UpdateBefore(typeof(SyncMatricesSystem))]
    partial class RenderSystem : SystemBase
    {
        ObjectRenderer Renderer;

        protected override void OnCreate()
        {
            base.OnCreate();

            RequireForUpdate<Renderable>();
        }
        protected override void OnUpdate()
        {
            GetRef();
            DrawMeshes();
        }

        void GetRef()
        {
            if (!Renderer)
                Renderer = GameObject
                    .FindGameObjectWithTag("Renderer")
                    .GetComponent<ObjectRenderer>();
        }
        void DrawMeshes()
        {
            var query = SystemAPI
                .QueryBuilder()
                .WithAll<Renderable>()
                .Build()
                .ToComponentDataArray<Renderable>(Allocator.Temp);

            Renderer.Render(query);
        }
    }
}