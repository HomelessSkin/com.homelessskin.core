using Unity.Entities;

namespace Core
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(SpawnSystemGroup))]
    public partial class InputSystemGroup : ComponentSystemGroup
    {

    }

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