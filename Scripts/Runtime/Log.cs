using System.Collections.Generic;

using UnityEngine;

namespace Core
{
    public static class Log
    {
        public static string InfoColor = ColorUtility.ToHtmlStringRGB(Color.white);
        public static string WarningColor = ColorUtility.ToHtmlStringRGB(Color.yellow);
        public static string ErrorColor = ColorUtility.ToHtmlStringRGB(Color.red);

        static Queue<Message> Q = new Queue<Message>();

        public static bool Read(out Message message) => Q.TryDequeue(out message);

        public static void Info(string agent, string message, float time = 1f)
        {
            var text = $"[<color=#{InfoColor}>{agent}</color>] <color=#{InfoColor}>{message}</color>";

            Q.Enqueue(new Message
            {
                Text = text,
                Time = time,
                CallTime = Time.time
            });

            Debug.Log(text);
        }
        public static void Warning(string agent, string message, float time = 3f)
        {
            var text = $"[<color=#{WarningColor}>{agent}</color>] <color=#{InfoColor}>{message}</color>";

            Q.Enqueue(new Message
            {
                Text = text,
                Time = time,
                CallTime = Time.time
            });

            Debug.Log(text);
        }
        public static void Error(string agent, string message, float time = 10f)
        {
            var text = $"[<color=#{ErrorColor}>{agent}</color>] <color=#{InfoColor}>{message}</color>";

            Q.Enqueue(new Message
            {
                Text = $"[<color=#{ErrorColor}>{agent}</color>] <color=#{InfoColor}>{message}</color>",
                Time = time,
                CallTime = Time.time
            });

            Debug.Log(text);
        }

        public class Message
        {
            public string Text;
            public float Time;
            public float CallTime;
        }
    }
}