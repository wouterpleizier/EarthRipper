using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EarthRipperHook
{
    internal static class Logger
    {
        private static readonly ConcurrentQueue<object> _queue = new ConcurrentQueue<object>();

        public static void LogMessage(string message, string category = null)
        {
            if (string.IsNullOrEmpty(category))
            {
                _queue.Enqueue(message);
            }
            else
            {
                _queue.Enqueue($"[{category}] {message}");
            }
        }

        public static void LogException(Exception exception)
        {
            _queue.Enqueue(exception);
        }

        public static List<object> DequeueMessages()
        {
            List<object> messages = new List<object>();
            while (_queue.TryDequeue(out object message))
            {
                messages.Add(message);
            }

            return messages;
        }
    }
}
