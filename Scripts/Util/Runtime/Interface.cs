namespace Core.Util
{
    public interface IPrefKey
    {
        public string _Key { get; }
    }

    public interface IStateMachine
    {
        public State _State { get; }
        public void SetState(State state);

        public enum State : byte
        {
            Null = 0,
            Default = 1,
            Exit = 2,
        }
    }
}