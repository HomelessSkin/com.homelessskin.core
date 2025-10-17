using Core.Util;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;

using UnityEngine;

using static Unity.Mathematics.math;
using static Unity.Mathematics.quaternion;

namespace Core.Rendering
{
    public class RenderableObjectBaker : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] bool DebugPosition;
        [SerializeField] float DebugRayLen;
        [SerializeField] int AnimationID;
        [Space(20f)]
#endif

        [SerializeField] Renderable.Type Type;
        [SerializeField] bool AddRenderable;
        [SerializeField] bool ColliderOffset;
        [SerializeField, Tooltip("Static Only. Set Property to Renderer and use ID here.")] int PropertyID = -1;
        [SerializeField] RenderableObject RenderableObj;
        [SerializeField] GameObject[] BodyParts;

        public int GetID()
        {
            if (!RenderableObj)
            {
                Debug.Log($"Assign Renderable Object to {gameObject.name} Prefab!");

                return -1;
            }

            return RenderableObj.ID;
        }
        public int GetProperty() => PropertyID;
        public string GetName() => RenderableObj.name;

        class RenderableObjectBakerBaker : Baker<RenderableObjectBaker>
        {
            public override void Bake(RenderableObjectBaker authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var id = authoring.GetID();
                AddComponent(entity, new RenderableObjectID { Value = id });
                SetComponentEnabled<RenderableObjectID>(entity, true);

                switch (authoring.Type)
                {
                    case Renderable.Type.Kinematic:
                    AddComponent(entity, new KinematicObject
                    {
                        ID = id,
                        Action = 0,
                    });
                    break;
                    case Renderable.Type.Static:
                    AddComponent(entity, new Renderable
                    {
                        RenderType = Renderable.Type.Static,
                        Entity = entity,
                        IsVisible = true,
                        Key = id,
                        PropertyIndex = authoring.GetProperty(),
                        Matrix = authoring.transform.localToWorldMatrix,
                        Offset = authoring.ColliderOffset ? GetOffset(authoring) : Matrix4x4.identity,
                    });
                    SetComponentEnabled<Renderable>(entity, true);
                    break;
                    case Renderable.Type.Dynamic:
                    if (authoring.AddRenderable)
                    {
                        AddComponent(entity, new Renderable
                        {
                            RenderType = Renderable.Type.Dynamic,
                            Entity = entity,
                            Collider = 0,
                            IsVisible = true,
                            Key = id,
                            PropertyIndex = authoring.GetProperty(),
                            Matrix = authoring.transform.localToWorldMatrix,
                            Offset = authoring.ColliderOffset ? GetOffset(authoring) : Matrix4x4.identity,
                        });
                        SetComponentEnabled<Renderable>(entity, true);
                    }
                    break;
                }

                if (authoring.BodyParts != null && authoring.BodyParts.Length > 0)
                {
                    AddComponent(entity, new Animator
                    {
                        CurrentAnimation = (int)AnimationType.Idle,
                        Offset = new float4x4(LookRotation(right(), up()), 0f)
                    });

                    AddBuffer<BodyPart>(entity);
                    for (int p = 0; p < authoring.BodyParts.Length; p++)
                        AppendToBuffer(entity, new BodyPart
                        {
                            ID = p,
                            Value = GetEntity(authoring.BodyParts[p], TransformUsageFlags.Dynamic),
                        });
                }
            }

            Matrix4x4 GetOffset(RenderableObjectBaker authoring)
            {
                var offset = Matrix4x4.identity;
                if (authoring.TryGetComponent<PhysicsShapeAuthoring>(out var collider))
                    switch (collider.ShapeType)
                    {
                        case ShapeType.Box:
                        {
                            var prop = collider.GetBoxProperties();
                            offset = Matrix4x4.TRS(prop.Center, prop.Orientation, Vector3.one);
                        }
                        break;
                        case ShapeType.Sphere:
                        {
                            var prop = collider.GetSphereProperties(out var q);
                            offset = Matrix4x4.TRS(prop.Center, q, Vector3.one);
                        }
                        break;
                        case ShapeType.Capsule:
                        {
                            var prop = collider.GetCapsuleProperties();
                            offset = Matrix4x4.TRS(prop.Center, prop.Orientation, Vector3.one);
                        }
                        break;
                    }

                return offset;
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (DebugPosition)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, DebugRayLen * Vector3.up);
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, DebugRayLen * Vector3.forward);
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, DebugRayLen * Vector3.right);
            }
        }
#endif
    }

    public struct RenderableObjectID : IComponentData, IEnableableComponent
    {
        public int Value;
    }
    public struct KinematicObject : IComponentData
    {
        public int ID;
        public float3 Forward;

        // Action
        int Act;
        public int Action
        {
            get => Act;
            set
            {
                if (Act != value)
                {
                    CurrentKey = 0;
                    CurrentTime = 0f;

                    Act = value;
                }
            }
        }

        // Animation
        int Key;
        public int CurrentKey
        {
            get => Key;
            set
            {
                if (Key != value)
                {
                    CurrentTime = 0f;

                    Key = value;
                }
            }
        }
        public float CurrentTime;
    }
    public struct Animator : IComponentData
    {
        public int CurrentAnimation
        {
            get => Animation;
            set
            {
                if (value != Animation)
                {
                    Frame = 0;
                    FrameTime = 0f;

                    Animation = value;
                }
            }
        }
        public int CurrentFrame
        {
            get => Frame;
            set
            {
                if (value != Frame)
                {
                    FrameTime = 0f;

                    Frame = value;
                }
            }
        }
        public float CurrentTime
        {
            get => FrameTime;
            set => FrameTime = value;
        }

        public float4x4 Offset;

        public int Animation;
        int Frame;
        float FrameTime;
    }
    public struct BodyPart : IKeyBuffer
    {
        public int ID;
        public Entity Value;

        public int GetID() => ID;
    }
}