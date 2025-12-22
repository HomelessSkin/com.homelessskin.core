using System;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "GamePlay/GameSettings")]
    public class GameSettingsAsset : ScriptableObject
    {
        public Data Settings;

        [Serializable]
        public struct Data : IComponentData
        {
            [Header("Camera")]
            public float3 CameraOffset;
            public float CameraSpeed;
            [Space]
            [Header("Animation")]
            public float AnimationFreequency;
            public float AnimationWeight;
            [Space]
            [Header("Controller")]
            public float MaxRopeVelocity;
            public float OnAir;
            public float ControllerOffset;
            public float Floating;
            [Space]
            [Header("Rope")]
            public float ThrowPower;
            public float HookMagnetism;
            public float RopeIncrement;
            public float RopeLenMul;
            public float KnotOffset;
            public float KnotCast;
            [Space]
            [Header("Generation")]
            public int LevelWidth;
            public int LevelHeight;
            public int LevelCount;
            public float FloorSpawnLimit;
            public float4 LevelNoise;
        }
    }
}