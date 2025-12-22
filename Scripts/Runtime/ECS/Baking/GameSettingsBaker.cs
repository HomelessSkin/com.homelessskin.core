using Unity.Entities;

using UnityEngine;

namespace Core
{
    class GameSettingsBaker : MonoBehaviour
    {
        [SerializeField] GameSettingsAsset Asset;

        class GameSettingsBakerBaker : Baker<GameSettingsBaker>
        {
            public override void Bake(GameSettingsBaker authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, authoring.Asset.Settings);
            }
        }
    }
}