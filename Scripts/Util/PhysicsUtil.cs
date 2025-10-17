using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

using static Unity.Mathematics.math;

namespace Core.Util
{
    public static class Phys
    {
        public static float3 GetLocalAngular(Entity entity, float3 from, float3 to, float gain, EntityManager manager)
        {
            var vec = cross(from, to);
            if (length(vec) > 0.01f)
            {
                var angle = clamp(acos(dot(from, to)), -1f, 1f);
                var worldAngular = angle * gain * normalize(vec);

                var transform = manager.GetComponentData<LocalTransform>(entity);
                var mass = manager.GetComponentData<PhysicsMass>(entity);

                return mul(inverse(mul(transform.Rotation, mass.InertiaOrientation)), worldAngular);
            }

            return 0f;
        }
        public static float3 GetLocalAngular(Entity entity,
            float3 from,
            float3 to,
            float gain,
            ComponentLookup<LocalTransform> transformLookup,
            ComponentLookup<PhysicsMass> massLookup)
        {
            var vec = cross(from, to);
            if (length(vec) > 0.01f)
            {
                var angle = clamp(acos(dot(from, to)), -1f, 1f);
                var worldAngular = angle * gain * normalize(vec);

                var transform = transformLookup[entity];
                var mass = massLookup[entity];

                return mul(inverse(mul(transform.Rotation, mass.InertiaOrientation)), worldAngular);
            }

            return 0f;
        }
    }
}