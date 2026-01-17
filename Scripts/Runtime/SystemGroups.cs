using Unity.Entities;

namespace Core
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class SpawnSystemGroup : ComponentSystemGroup
    {

    }

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderLast = true)]
    public partial class FixedSystemGroup : ComponentSystemGroup
    {

    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class RenderSystemGroup : ComponentSystemGroup
    {

    }
}