using Core.Rendering;
using Core.UI;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace Core.GamePlay
{
    public abstract partial class GameSystemBase : SystemBase
    {
        protected GameEvent.Type State;
        protected GameEvent.Type PrevState;
        protected GameEvent.Type NextState;

        protected UIManagerBase UIManagerBase;
        protected CameraEngineBase CameraEngineBase;

        protected override void OnCreate()
        {
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            GetRef();
            UpdateState();
            Proceed();
            HandleControl();
            UpdateCamera();
            UpdateUI();
        }

        protected virtual void GetRef()
        {
            if (!UIManagerBase)
                UIManagerBase = GameObject
                    .FindGameObjectWithTag("UIManager")
                    .GetComponent<UIManagerBase>();

            if (!CameraEngineBase)
                CameraEngineBase = GameObject
                    .FindGameObjectWithTag("MainCamera")
                    .GetComponent<CameraEngineBase>();
        }
        protected virtual void UpdateState()
        {
            var query = EntityManager.CreateEntityQuery(typeof(GameEvent));
            if (query.CalculateEntityCount() == 0)
                return;

            PrevState = State;
            var events = query.ToComponentDataArray<GameEvent>(Allocator.Temp);
            for (int e = 0; e < events.Length; e++)
                SetState(events[e].Set);

            EntityManager.DestroyEntity(query);
        }
        protected virtual void SetState(GameEvent.Type state)
        {
            State = state;
        }
        protected virtual void Proceed()
        {

        }
        protected virtual void HandleControl()
        {

        }
        protected virtual void UpdateUI()
        {

        }
        protected virtual void HandleSaving(Entity playerE)
        {

        }
        protected virtual void HandleLoading(Entity playerE)
        {

        }

        void UpdateCamera()
        {
            var query = EntityManager.CreateEntityQuery(typeof(SetCameraRequest));
            var requests = query.ToComponentDataArray<SetCameraRequest>(Allocator.Temp);
            for (int r = 0; r < requests.Length; r++)
                CameraEngineBase.SetTarget(requests[r]);
            EntityManager.DestroyEntity(query);

            CameraEngineBase.UpdateFrame();
        }
    }

    public struct GameEvent : IComponentData
    {
        public Type Set;

        public enum Type : byte
        {
            Idle = 0,
            Start = 1,
            Play = 2,
            Over = 3,
            Cutscene = 4,
            Transition = 5,
            Selection = 6,
            Saving = 7,
            Loading = 8,
            Rotation = 9,
        }
    }
}