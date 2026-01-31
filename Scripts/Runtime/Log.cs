using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    public static class Log
    {
        public static string InfoColor = ColorUtility.ToHtmlStringRGB(Color.white);
        public static string WarningColor = ColorUtility.ToHtmlStringRGB(Color.yellow);
        public static string ErrorColor = ColorUtility.ToHtmlStringRGB(Color.red);

        static UnityEvent OnRead = new UnityEvent();
        static Queue<Message> Q = new Queue<Message>();

        public static void AddReadListener(UnityAction action) => OnRead.AddListener(action);
        public static void Info(object agent, string message, float time = 1f)
        {
            var text = $"[<color=#{InfoColor}>{agent.GetType().FullName}</color>] <color=#{InfoColor}>{message}</color>";

            Q.Enqueue(new Message
            {
                Text = text,
                Time = time,
                CallTime = Time.time
            });

            Debug.Log(text);
        }
        public static void Warning(object agent, string message, float time = 3f)
        {
            var text = $"[<color=#{WarningColor}>{agent.GetType().FullName}</color>] <color=#{InfoColor}>{message}</color>";

            Q.Enqueue(new Message
            {
                Text = text,
                Time = time,
                CallTime = Time.time
            });

            Debug.Log(text);
        }
        public static void Error(object agent, string message, float time = 10f)
        {
            var text = $"[<color=#{ErrorColor}>{agent}</color>] <color=#{InfoColor}>{message}</color>";

            Q.Enqueue(new Message
            {
                Text = $"[<color=#{ErrorColor}>{agent.GetType().FullName}</color>] <color=#{InfoColor}>{message}</color>",
                Time = time,
                CallTime = Time.time
            });

            Debug.Log(text);
        }

        public static bool Read(out Message message)
        {
            if (Q.TryDequeue(out message))
                OnRead.Invoke();

            return message != null;
        }
        public static bool IsEmpty() => Q.Count == 0;

        public class Message
        {
            public string Text;
            public float Time;
            public float CallTime;
        }
    }
}