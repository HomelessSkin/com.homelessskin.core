using Unity.Entities;

namespace Core
{
    public abstract partial class GameSystemBase : BehaviourSystem, IStateMachine
    {
        public string _Group => Group;
        public IStateMachine.State _State { get => State; set => State = value; }
        public IStateMachine.State _PrevState { get => PrevState; set => PrevState = value; }

        protected string Group;
        protected IStateMachine.State State;
        protected IStateMachine.State PrevState;

        protected override void OnCreate()
        {
            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
            UpdateUI();
        }

        public virtual void SetState(IStateMachine.State state)
        {
            State = state;
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
}