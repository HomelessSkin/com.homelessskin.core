using Unity.Entities;

namespace Core
{
    class EnvironmentElementBaker : PrefabBaker
    {


        class EnvironmentElementBakerBaker : Baker<EnvironmentElementBaker>
        {
            public override void Bake(EnvironmentElementBaker authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, authoring.ID);
                AddComponent<EnvironmentElementTag>(entity);
            }
        }
    }

    public struct EnvironmentElementTag : IComponentData { }
}