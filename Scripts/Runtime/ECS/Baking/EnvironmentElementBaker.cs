using Unity.Entities;

using UnityEngine;

namespace Core
{
    class EnvironmentElementBaker : MonoBehaviour
    {


        class EnvironmentElementBakerBaker : Baker<EnvironmentElementBaker>
        {
            public override void Bake(EnvironmentElementBaker authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<EnvironmentElementTag>(entity);
            }
        }
    }

    public struct EnvironmentElementTag : IComponentData { }
}