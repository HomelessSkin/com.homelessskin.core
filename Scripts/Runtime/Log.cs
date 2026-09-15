using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        public static void Object<T>(object agent, T target) where T : ILogTarget
        {
            Info(agent, $"-----------------------------");
            if (target == null)
            {
                Error(agent, $"Target is Null!");
                Info(agent, $"-----------------------------");

                return;
            }

            var type = target.GetType();
            var bindingFlags =
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance;

            var fields = type
                .GetFields(bindingFlags)
                .Where(f => f.GetCustomAttribute<LogInfo>() != null);

            var properties = type
                .GetProperties(bindingFlags)
                .Where(p => p.GetCustomAttribute<LogInfo>() != null);

            if (fields.Count() == 0 && properties.Count() == 0)
            {
                Warning(agent, $"No marked Fields or Properties!");
                Info(agent, $"-----------------------------");

                return;
            }

            foreach (var field in fields)
            {
                var message = "";

                FormatValue(field.GetValue(target), ref message, bindingFlags);
                Info(agent, $"{field.Name}: {message}");
            }

            foreach (var property in properties)
            {
                var message = "";

                FormatValue(property.GetValue(target), ref message, bindingFlags);
                Info(agent, $"{property.Name}: {message}");
            }

            Info(agent, $"-----------------------------");
        }

        public static bool Read(out Message message)
        {
            if (Q.TryDequeue(out message))
                OnRead?.Invoke();

            return message != null;
        }
        public static bool IsEmpty() => Q.Count == 0;

        static void FormatValue(object value, ref string message, BindingFlags bindingFlags)
        {
            if (value == null)
            {
                message += "null";

                return;
            }

            if (value is string str)
            {
                message += $"\"{str}\"";

                return;
            }

            if (value is ILogTarget target)
            {
                message += "{ ";

                var type = target.GetType();
                var fields = type
                    .GetFields(bindingFlags)
                    .Where(f => f.GetCustomAttribute<LogInfo>() != null);

                var properties = type
                    .GetProperties(bindingFlags)
                    .Where(p => p.GetCustomAttribute<LogInfo>() != null);

                var first = true;
                foreach (var field in fields)
                {
                    if (!first)
                        message += ", ";
                    first = false;

                    message += field.Name + ": ";

                    FormatValue(field.GetValue(target), ref message, bindingFlags);
                }

                foreach (var property in properties)
                {
                    if (!first)
                        message += ", ";
                    first = false;

                    message += property.Name + ": ";

                    FormatValue(property.GetValue(target), ref message, bindingFlags);
                }

                message += "}";

                return;
            }

            if (value is IEnumerable enumerable)
            {
                message += "[ ";

                var first = true;
                foreach (var item in enumerable)
                {
                    if (!first)
                        message += ", ";
                    first = false;

                    FormatValue(item, ref message, bindingFlags);
                }

                message += "]";

                return;
            }

            message += value.ToString();
        }

        public class Message
        {
            public string Text;
            public float Time;
            public float CallTime;
        }
    }

    public interface ILogTarget { }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class LogInfo : Attribute { }
}