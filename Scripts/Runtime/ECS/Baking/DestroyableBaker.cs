using System;

using Unity.Entities;

using UnityEngine;

namespace Core
{
    class DestroyableBaker : MonoBehaviour
    {
        [SerializeField] Destroyable Destroyable;

        class DestroyableBakerBaker : Baker<DestroyableBaker>
        {
            public override void Bake(DestroyableBaker authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, authoring.Destroyable);
            }
        }
    }

    [Serializable]
    public struct Destroyable : IComponentData
    {
        public Type DestroyBy;
        public CompareType IF;
        public float Target;

        [HideInInspector] public float Value;

        public Destroyable(Type type, CompareType compare, float target, float start = 0f)
        {
            DestroyBy = type;
            IF = compare;
            Target = target;
            Value = start;
        }

        public bool IsDone()
        {
            switch (IF)
            {
                case CompareType.Equal:
                return Value == Target;
                case CompareType.Smaller:
                return Value < Target;
                case CompareType.Greater:
                return Value > Target;
                case CompareType.SmallerOrEqual:
                return Value <= Target;
                case CompareType.GreaterOrEqual:
                return Value >= Target;
            }

            return true;
        }

        public enum Type : byte
        {
            None = 0,
            Y = 1,
            Timer = 2,


        }

        public enum CompareType : byte
        {
            Null = 0,
            Equal = 1,
            Smaller = 2,
            Greater = 3,
            SmallerOrEqual = 4,
            GreaterOrEqual = 5,
        }
    }
}