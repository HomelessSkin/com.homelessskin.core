using System;

using Core.Util;

using UnityEngine;
using UnityEngine.Rendering;

namespace Core.Rendering
{
    [CreateAssetMenu(fileName = "Renderable_", menuName = "Presentation/RenderableObject")]
    public class RenderableObject : KeyScriptable
    {
#if UNITY_EDITOR
        [Space]
        public Material DragMaterial;
#endif
        [Space]
        public bool ReceiveShadows = false;
        public ShadowCastingMode ShadowCasting = ShadowCastingMode.Off;
        public Mesh Mesh;
        public Material Material;
    }

    [Serializable]
    public struct RenderProperty : IEquatable<RenderProperty>
    {
        public Type ValueType;
        public string Reference;

        public int IntValue;
        public float FloatValue;
        public Vector2 Vector2Value;
        public Vector3 Vector3Value;
        public bool BooleanValue;

        public bool Equals(RenderProperty other)
        {
            if (ValueType != other.ValueType || Reference != other.Reference)
                return false;

            switch (ValueType)
            {
                case Type.Int:
                return IntValue == other.IntValue;
                case Type.Float:
                return FloatValue == other.FloatValue;
                case Type.Vector2:
                return Vector2Value == other.Vector2Value;
                case Type.Vector3:
                return Vector3Value == other.Vector3Value;
                default:
                return true;
            }
        }
        public override int GetHashCode()
            => (int)ValueType +
            Reference.GetHashCode() +
            IntValue +
            (int)(1000 * FloatValue) / 100 +
            Vector2Value.GetHashCode() +
            Vector3Value.GetHashCode();

        public enum Type : byte
        {
            Null = 0,
            Int = 1,
            Float = 2,
            Vector3 = 3,
            Vector2 = 4,
            Boolean = 5,

        }
    }
}