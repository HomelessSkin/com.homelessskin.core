using Unity.Entities;

using UnityEngine;

namespace Core.Rendering
{
    public struct Renderable : IComponentData, IEnableableComponent
    {
        public bool IsVisible;

        public Type RenderType;

        public int Key;
        public int PropertyIndex;

        public Entity Entity;
        public Matrix4x4 Matrix;
        public Matrix4x4 Offset;

        // Dynamic
        public int Collider;

        public enum Type : byte
        {
            Null = 0,
            Static = 1,
            Kinematic = 2,
            Dynamic = 3,

        }
    }
}