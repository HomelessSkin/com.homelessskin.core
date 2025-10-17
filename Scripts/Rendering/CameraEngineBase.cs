using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace Core.Rendering
{
    public abstract class CameraEngineBase : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] bool SwitchOn = true;
#endif

        [Space]
        [SerializeField, Range(1, 30)] int CameraSpeed;

        protected MovementType Movement;
        protected Vector3 LookAt;
        protected Vector3 TargetPosition;

        protected Camera Camera;

        protected virtual void Start()
        {
            Camera = GetComponent<Camera>();
        }

        public virtual void UpdateFrame()
        {
#if UNITY_EDITOR
            if (!SwitchOn)
                return;
#endif

            var dt = Time.deltaTime;
            var toTarget = TargetPosition - transform.position;

            transform.position += dt * CameraSpeed * toTarget;
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.LookRotation((LookAt - transform.position).normalized),
                5f * dt * CameraSpeed);
        }
        public virtual void SetTarget(float3 pos, float3 lookAt, MovementType movement = MovementType.Null)
        {
            TargetPosition = pos;
            LookAt = lookAt;
            Movement = movement;
        }
        public virtual void SetTarget(SetCameraRequest request)
        {
            TargetPosition = request.TargetPosition;
            LookAt = request.LookAt;
            Movement = request.MovementType;
        }

        public Vector3 GetTargetPosition() => TargetPosition;
        public Ray ScreenPointToRay(Vector3 point) => Camera.ScreenPointToRay(point);

        public enum MovementType : byte
        {
            Null = 0,

        }
    }

    [System.Serializable]
    public struct SetCameraRequest : IComponentData
    {
        public float3 TargetPosition;
        public float3 LookAt;
        public CameraEngineBase.MovementType MovementType;
    }
}