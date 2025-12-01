namespace Core.Util
{
    public interface IPrefKey
    {
        public string _Key { get; }
    }

    public interface IStateMachine
    {
        protected State _State { get; set; }
        protected void SetState(State state);

        protected enum State : byte
        {
            Null = 0,
            Default = 1,

        }
    }
}