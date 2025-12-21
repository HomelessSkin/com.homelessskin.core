using System;
using System.IO;
using System.Threading.Tasks;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace Core
{
    public interface IKeyBuffer : IBufferElementData
    {
        public int GetID();
        public Entity GetValue() => Entity.Null;
    }

    public interface IStorage
    {
        public string _DataFile { get; }
        public string _ResourcesPath { get; }
        public string _PersistentPath { get; }
        public string _Dir { get; }

        public string Collect(string name, string type = null)
        {
            var path = $"{_Dir}";
            if (!string.IsNullOrEmpty(type))
                path += $"{type}/";
            path += $"{name}{_DataFile.Replace("*", "")}";

            if (File.Exists(path))
                return File.ReadAllText(path);

            return "";
        }
        public async Task<string> CollectAsync(string name, string type = null)
        {
            var path = $"{_Dir}";
            if (!string.IsNullOrEmpty(type))
                path += $"{type}/";
            path += $"{name}{_DataFile.Replace("*", "")}";

            if (File.Exists(path))
                return await File.ReadAllTextAsync(path);

            return "";
        }
        public virtual void Store(Data data)
        {
            if (!Directory.Exists($"{_Dir}/{data.Type}"))
                Directory.CreateDirectory($"{_Dir}/{data.Type}");

            File.WriteAllText($"{_Dir}/{data.Type}/{data.Name}{_DataFile.Replace("*", "")}", data.Serialize());
        }
        public async virtual Task StoreAsync(Data data)
        {
            if (!Directory.Exists($"{_Dir}/{data.Type}"))
                Directory.CreateDirectory($"{_Dir}/{data.Type}");

            await File.WriteAllTextAsync($"{_Dir}/{data.Type}/{data.Name}{_DataFile.Replace("*", "")}", data.Serialize());
        }

        [Serializable]
        public class Data
        {
            public string Name;
            public string Type;

            public virtual string Serialize() => JsonUtility.ToJson(this, true);
        }
    }

    public interface IPrefabID
    {
        public int GetID();
    }

    public interface IPrefKey
    {
        public string _Key { get; }
    }

    public interface IStateMachine
    {
        public string _Group { get; }
        public State _State { get; set; }
        public State _PrevState { get; set; }

        public void SetState(IStateMachine.State state)
        {
            _State = state;
        }
        public void UpdateState(EntityManager manager)
        {
            var query = manager.CreateEntityQuery(typeof(GameEvent));
            if (query.IsEmpty)
                return;

            _PrevState = _State;
            var entities = query.ToEntityArray(Allocator.Temp);
            for (int e = 0; e < entities.Length; e++)
            {
                var entity = entities[e];
                var @event = manager.GetComponentObject<GameEvent>(entity);
                if (@event.Group == "All" ||
                     @event.Group == _Group ||
                    (@event.Group == "WithState" && @event.Over == _State))
                    SetState(@event.Set);
            }
        }

        public class GameEvent : IComponentData
        {
            public string Group;
            public IStateMachine.State Over;
            public IStateMachine.State Set;
        }

        public enum State : byte
        {
            Null = 0,
            Default = 1,
            Exit = 2,
            Waiting = 3,

            Idle = 10,
            Start = 11,
            Play = 12,
            Over = 13,
            Cutscene = 14,
            Transition = 15,
            Selection = 16,
            Saving = 17,
            Loading = 18,
            Rotation = 19,
            LookAround = 20,

        }
    }
}