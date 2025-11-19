using Core.Util;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace Core.GamePlay
{
    public abstract partial class GameSystemBase : BehaviourSystem
    {
        protected GameEvent.Type State;
        protected GameEvent.Type PrevState;

        protected override void OnCreate()
        {
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();

            UpdateState();
            Proceed();
            HandleControl();
            UpdateCamera();
            UpdateUI();
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
            if (Input.GetKeyUp(KeyCode.G))
                HandleG();
            if (Input.GetKeyUp(KeyCode.R))
                HandleR();
            if (Input.GetKeyUp(KeyCode.X))
                HandleX();
            if (Input.GetKeyUp(KeyCode.Y))
                HandleY();
            if (Input.GetKeyUp(KeyCode.Z))
                HandleZ();
            if (Input.GetKeyUp(KeyCode.Escape))
                HandleEsc();

            if (Input.GetKeyDown(KeyCode.Mouse0))
                HandleMouseLeftDown();
            else if (Input.GetKeyUp(KeyCode.Mouse0))
                HandleMouseLeftUp();

            if (Input.GetKeyDown(KeyCode.Mouse1))
                HandleMouseRightDown();
            else if (Input.GetKeyUp(KeyCode.Mouse1))
                HandleMouseRightUp();

            var move = Input.mousePositionDelta;
            if (move.magnitude > 0.001f)
                HandleMouseMove(move);

            var scroll = Input.mouseScrollDelta;
            if (scroll.magnitude > 0.001f)
                HandleMouseScroll(scroll);
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
            //var query = EntityManager.CreateEntityQuery(typeof(SetCameraRequest));
            //var requests = query.ToComponentDataArray<SetCameraRequest>(Allocator.Temp);
            //for (int r = 0; r < requests.Length; r++)
            //    CameraEngineBase.SetTarget(requests[r]);
            //EntityManager.DestroyEntity(query);

            //CameraEngineBase.UpdateFrame();
        }

        #region CONTROLLER
        protected virtual void HandleG()
        {

        }
        protected virtual void HandleR()
        {

        }
        protected virtual void HandleX()
        {

        }
        protected virtual void HandleY()
        {

        }
        protected virtual void HandleZ()
        {

        }
        protected virtual void HandleEsc()
        {

        }

        protected virtual void HandleMouseLeftDown()
        {

        }
        protected virtual void HandleMouseLeftUp()
        {

        }
        protected virtual void HandleMouseRightDown()
        {

        }
        protected virtual void HandleMouseRightUp()
        {

        }
        protected virtual void HandleMouseMove(Vector3 move)
        {

        }
        protected virtual void HandleMouseScroll(Vector3 scroll)
        {

        }
        #endregion
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
            LookAround = 10,

        }
    }
}