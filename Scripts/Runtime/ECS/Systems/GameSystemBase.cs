using Unity.Collections;
using Unity.Entities;

namespace Core
{
    public abstract partial class GameSystemBase : BehaviourSystem, IStateMachine
    {
        public IStateMachine.State _State => State;
        protected IStateMachine.State State;
        public IStateMachine.State _PrevState => PrevState;
        protected IStateMachine.State PrevState;

        protected override void OnCreate()
        {
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();

            UpdateState();
            Proceed();
            UpdateUI();
        }

        public virtual void SetState(IStateMachine.State state)
        {
            State = state;
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
        protected virtual void Proceed()
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
    }

    public struct GameEvent : IComponentData
    {
        public IStateMachine.State Set;
    }
}